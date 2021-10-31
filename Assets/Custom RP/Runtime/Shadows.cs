using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
//A class to actually handle the command buffer and render for the shadows
//The Lighting class will actually call an instance of Shadows to render
//Because every light has to cast its own shadows, duh.
/// </summary>
public class Shadows
{
    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    const int maxShadowedDirectionalLightCount = 4;//uhhhh i didn't know you had to limit this
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
        SetTileViewport(index, split, tileSize);
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);//I think these are just available?
        ExecuteBuffer();
        //Only draws for materialas that have a lightMode tag for "ShadowCaster" pases
        context.DrawShadows(ref shadowSettings); // actually tell the context to draw
    }

    //This function sets the viewport for each directional light on the atlas
    void SetTileViewport(int index, int split, int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect
        (
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
            ));
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