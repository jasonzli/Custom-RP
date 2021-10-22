using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{

    //We abstract the camera rendering to its own class
    CameraRenderer renderer = new CameraRenderer();

    //configurable pipeline
    bool useDynamicBatching, useGPUInstancing;

    //Add a constructor to the render pipeline to set values
    public CustomRenderPipeline(
        bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher
    )
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        //Takes precedence over dynamic batching
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }

    //RenderPipeline has a protected Render method that we override to make this pipeline work
    //This is what is called in the custom pipline *asset*
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    {
        foreach (Camera camera in cameras)
        {//This lets us divert different cameras to do different things later.
         //THis is the scriptable renderer of the URP essentially
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
        }
    }

}
