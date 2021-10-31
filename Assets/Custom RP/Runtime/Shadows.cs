using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
//A class to actually handle the command buffer and render for the shadows
//The Lighting class will actually call an instance of Shadows to render
//Because every light has to cast its own shadows, duh.
/// </summary>
public class Shadows
{
    const int maxShadowedDirectionalLightCount = 4;//uhhhh i didn't know you had to limit this

    static int
        dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

    //transformation matrices for converting fragment positions to shadowmap UVs
    static Matrix4x4[]
        dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount]; int shadowedDirectionalLightCount;
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
    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        //add light if there's space in our count
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            //actually casting shadows check
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            //check if the light is in the bounds of the cull, returns if the bounds are valid
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount++] =
                new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex //index of the shadowable directional light
                };
        }
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

        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings =
            new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
            out ShadowSplitData splitData
        );
        shadowSettings.splitData = splitData; //splitData contains cull information about shadow casters
        //set the texture coords per light (this is the annoying thing we get to defer in deferred renders)
        SetTileViewport(index, split, tileSize);
        //add this light's conversion matrix to the array
        dirShadowMatrices[index] = ConvertToAtlasMatrix(
            projectionMatrix * viewMatrix,
            SetTileViewport(index, split, tileSize), split
            ); //conversion matrix from world to lightspace -> light shadow projection matrix by view matrix
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);//I think these are just available?        ExecuteBuffer();
        //Only draws for materialas that have a lightMode tag for "ShadowCaster" pases
        context.DrawShadows(ref shadowSettings); // actually tell the context to draw
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
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m31) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m31) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m31) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m31) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m31) * scale;
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
            )
            );
        return offset;
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