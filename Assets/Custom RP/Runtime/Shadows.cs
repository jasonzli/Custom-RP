using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
//A class to actually handle the command buffer and render for the shadows
//The Lighting class will actually call an instance of Shadows to render
//Because every light has to cast its own shadows, duh.
/// </summary>
public class Shadows
{
    const string bufferName = "Shadows";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings settings;

    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults,
        ShadowSettings settings
    )
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();//never forget this clear
    }
}