using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

//a public class Lighting with a command buffer
public class Lighting
{
    const int maxDirLightCount = 4;
    const string bufferName = "Lighting";
    static int
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    static Vector4[]
        dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount];
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    CullingResults cullingResults;

    Shadows shadows = new Shadows();
    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults,
        ShadowSettings shadowSettings
        )
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();//Use the culling results to set up lights instead of searching for it
        shadows.Render(); //After the lights are setup, render the shadow atlas, remember that objects will be lit in their own fragment step
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(i, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break; //If we have more than maxDirLightCount of lights, we can't use them
                }
            }

        }
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    //This function sets the array of directional lights that we pass to the CBUFFER on the GPU
    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor; //not in linear space, must be converted in pipeline with GraphicsSettings.lightsUseLinearIntensity
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2); //the forward vector!
        shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    //cleanup is called by camera, which is the thing calling the render
    public void Cleanup()
    {
        shadows.Cleanup();
    }
}