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

    public void Setup(ScriptableRenderContext context)
    {
        buffer.BeginSample(bufferName);
        SetupDirectionalLight();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupDirectionalLight()
    {
        Light light = RenderSettings.sun; //a Unity light, not the hlsl light struct
        buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
        buffer.SetGlobalVector(dirLightDirectionID, -light.transform.forward);
    }
}