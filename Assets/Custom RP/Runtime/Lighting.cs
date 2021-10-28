using UnityEngine;
using UnityEngine.Rendering;

//a public class Lighting with a command buffer
public class Lighting
{
    const string bufferName = "Lighting";
    static int
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionID = Shader.PropertyToID("_DirectionalLightDirection");
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    CullingResults cullingResults;

    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults
        )
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        //SetupDirectionalLight();
        SetupLights();//Use the culling results to set up lights instead of searching for it
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
    }
    void SetupDirectionalLight()
    {
        Light light = RenderSettings.sun; //a Unity light, not the hlsl light struct
        buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
        buffer.SetGlobalVector(dirLightDirectionID, -light.transform.forward);
    }
}