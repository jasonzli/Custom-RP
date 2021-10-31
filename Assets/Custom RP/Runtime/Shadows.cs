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
    const int maxShadowedDirectionalLightCount = 1;//uhhhh i didn't know you had to limit this
    int ShadowedDirectionalLightCount;
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

    ShadowedDirectionalLight[] ShadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults,
        ShadowSettings settings
    )
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        ShadowedDirectionalLightCount = 0;
    }
    //Reserve space for the shadow atlas for hte light's shadow map
    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        //add light if there's space in our count
        if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            //actually casting shadows check
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            //check if the light is in the bounds of the cull, returns if the bounds are valid
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            ShadowedDirectionalLights[ShadowedDirectionalLightCount++] =
                new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex //index of the shadowable directional light
                };
        }
    }

    //skip if no shadowed lights
    public void Render()
    {
        if (ShadowedDirectionalLightCount > 0)
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
        ExecuteBuffer();
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