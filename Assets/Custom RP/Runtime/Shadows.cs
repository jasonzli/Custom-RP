using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
//A class to actually handle the command buffer and render for the shadows
//The Lighting class will actually call an instance of Shadows to render
//Because every light has to cast its own shadows, duh.
/// </summary>
public class Shadows
{
    const int maxShadowedDirectionalLightCount = 4, maxCascades = 4;//uhhhh i didn't know you had to limit this

    static int
        dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCountId = Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
        cascadeDataId = Shader.PropertyToID("_CascadeData"),
        shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize"),
        shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
    //keywords for filtering the shadows
    static string[] directionalFilterKeywords = {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };

    static string[] cascadeBlendKeywords = {
        "_CASCADE_BLEND_SOFT",
        "_CASCADE_BLEND_DITHER"
    };

    //This is actually the split data that is computed
    static Vector4[]
        cascadeCullingSpheres = new Vector4[maxCascades],
        cascadeData = new Vector4[maxCascades]; //xyz position w is radius
    //transformation matrices for converting fragment positions to shadowmap UVs
    static Matrix4x4[]
        dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];
    int shadowedDirectionalLightCount;
    const string bufferName = "Shadows";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings settings;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
        public float slopeScaleBias;
        public float nearPlaneOffset;
    }

    ShadowedDirectionalLight[] shadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults,
        ShadowSettings settings
    )
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        shadowedDirectionalLightCount = 0;
    }
    //Reserve space for the shadow atlas for hte light's shadow map
    //this is called by the lighting class
    public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        //add light if there's space in our count
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            //actually casting shadows check
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            //check if the light is in the bounds of the cull, returns if the bounds are valid
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount] =
                new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex, //index of the shadowable directional light
                    slopeScaleBias = light.shadowBias, //use light bias as slope bias. we hijacked the original bias value
                    nearPlaneOffset = light.shadowNearPlane //use this to pull the near plane so shadows still draw
                };
            //return the index of the light, which corresponds to the shadow tile in the atlas
            return new Vector3(
                light.shadowStrength,
                //multiply this here so teh cascades make multiple tiles
                settings.directional.cascadeCount * shadowedDirectionalLightCount++,
                light.shadowNormalBias
            );
        }
        return Vector3.zero;
    }

    //skip if no shadowed lights
    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            /*
            This has an interesting explanation
            So we only can release textures that exist (when camera releases it)
            But if we have no shadows, we don't make a texture. This makes a problem
            in webgl 2.0, which binds samplers *and* textures together. So we'd have a
            mismatch without creating a texture.

            We could do shader variants, but this is easier and *consistent
            */

            //a 1x1 texture just as a placeholder.
            buffer.GetTemporaryRT(
                dirShadowAtlasId, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize,
        //bits, filtering, type
        32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //16 is used for the unity RP, we'll use 32 for ours
        //we have to tell the cmera to render to the shadow atlas instead of the camera target
        buffer.SetRenderTarget(
            dirShadowAtlasId, //id of the target
            RenderBufferLoadAction.DontCare, //what to do to the initial state: dontcare is clear
            RenderBufferStoreAction.Store //we store data
        );
        buffer.ClearRenderTarget(true, false, Color.clear);//clear the target
        buffer.BeginSample(bufferName);
        ExecuteBuffer();

        //calculate the number of tiles we need for the shadows
        int tiles = shadowedDirectionalLightCount * settings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;//split into 1x2,2x2, or 4x4
        int tileSize = atlasSize / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        //we've calcualted the cascade information in the RenderDirectionalShadows, now send it to shader
        buffer.SetGlobalInt(cascadeCountId, settings.directional.cascadeCount);
        buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);

        //set the shadow transformation matrices
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

        float f = 1f - settings.directional.cascadeFade;
        buffer.SetGlobalVector(
            shadowDistanceFadeId,
            new Vector4(1f / settings.maxDistance, 1f / settings.distanceFade,
            1f / (1f - f * f)
            ) //cascade fade value is an inverse square because cascades are calculated in squared values
        );

        SetKeywords(
            directionalFilterKeywords, (int)settings.directional.filter - 1
        );
        SetKeywords(
            cascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1
        );
        buffer.SetGlobalVector( //we need this vector to do filtering
            shadowAtlasSizeId, new Vector4(atlasSize, 1f / atlasSize)
        );
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    //Draws the shadows for each cascade, putting them into tiles for the atlas
    //The atlas has all the shadows broken down into cascaded tiles
    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings =
            new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        //how mnay cascades and thus tiles, plus the detail ratios
        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = settings.directional.CascadeRatios;

        //a culling factor for the shadow cascade sphere. Unity is more conservative that we are
        float cullingFactor =
            Mathf.Max(0f, 0.8f - settings.directional.cascadeFade * 0.2f);

        //anywhere from 1 to 4 cascades
        //we render this for # of cascades * # of lights        
        for (int i = 0; i < cascadeCount; i++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, i, cascadeCount, ratios, tileSize,
            light.nearPlaneOffset, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
            out ShadowSplitData splitData);

            //modulate the radius of a cascade's culling sphere.
            splitData.shadowCascadeBlendCullingFactor = cullingFactor;

            shadowSettings.splitData = splitData; //splitData contains cull information about shadow casters
                                                  //set the texture coords per light (this is the annoying thing we get to defer in deferred renders)
            if (index == 0) //for the first light, because all culling is same for all lights
            {
                SetCascadeData(i, splitData.cullingSphere, tileSize);
            }
            int tileIndex = tileOffset + i; //which tile we're rendering
            //add this light's conversion matrix to the array
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                projectionMatrix * viewMatrix,
                SetTileViewport(tileIndex, split, tileSize), split
                ); //conversion matrix from world to lightspace -> light shadow projection matrix by view matrix
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);//I think these are just available?        ExecuteBuffer();
                                                                           //Only draws for materialas that have a lightMode tag for "ShadowCaster" pases

            //Depth bias only approach
            //buffer.SetGlobalDepthBias(50000f, 0f);
            //Stronger depth bias, peter panning
            //buffer.SetGlobalDepthBias(500000f, 0f);
            //Slope scale bias, increases bias linearly with slope of the angle between surface and light
            //buffer.SetGlobalDepthBias(0f, 3f);

            //use configured light slope bias
            buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
            ExecuteBuffer(); //You forgot this and no shadows drew, keep track of how many ExecuteBuffer() commands there are
            context.DrawShadows(ref shadowSettings); // actually tell the context to draw
            buffer.SetGlobalDepthBias(0f, 0f); //remember to turn off depth bias after shadows are drawn
        }

    }

    void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
    {
        float texelSize = 2f * cullingSphere.w / tileSize; //this is worldspace texelSize
        float filterSize = texelSize * ((float)settings.directional.filter + 1f); //when we filter we need to move the normal bias by its filter size
        cullingSphere.w -= filterSize; //move the sphere by filterSize to prevent sampling outside of the sphere
        cullingSphere.w *= cullingSphere.w; //precaulate square radius for distance comparison in shader
        cascadeCullingSpheres[index] = cullingSphere;
        cascadeData[index] = new Vector4(
            1f / cullingSphere.w, //inverse squared radius
            filterSize * 1.4142136f //multiply by the filterSize basis for texel offset
                                    //texelSize * 1.4142136f // without filter //sqrt 2
        );
    }

    //A function that takes the light matrix and the tile offset to make a conversion
    //from wolrd to *shadow atlas* space UV coordinates that are corrected for the split
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        //Usually the depth is intuitively 0 depth and 1 is max
        //other apis use 1 is 0 and 0 is max because of bit accuracy
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        //Now we convert the clip coords from -1 to 1 to 0 to 1
        //The texture coords are zero to one
        //this is manual because the matrix multipication is too many operations

        //the split and tile offset is calculated as a scalar
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }

    //This function sets the viewport for each directional light on the atlas
    Vector2 SetTileViewport(int index, int split, int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect
        (
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
        ));
        return offset;
    }

    void SetKeywords(string[] keywords, int enabledIndex)
    {
        //enabled the correct keyword for tthe filter
        for (int i = 0; i < keywords.Length; i++)
        {
            if (i == enabledIndex)
            {
                buffer.EnableShaderKeyword(keywords[i]);
            }
            else
            {
                buffer.DisableShaderKeyword(keywords[i]);
            }
        }
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();//never forget this clear
    }

    //release texture to free up memory. we have to be explict
    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
}