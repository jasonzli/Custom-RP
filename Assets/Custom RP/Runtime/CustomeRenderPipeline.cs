using UnityEngine;
using UnityEngine.Rendering;

public class CustomeRenderPipeline : RenderPipeline
{

    //We abstract the camera rendering to its own class
    CameraRenderer renderer = new CameraRenderer();

    //Add a constructor to the render pipeline to set values
    public CustomeRenderPipeline()
    {
        //Takes precedence over dynamic batching
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
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
            renderer.Render(context, camera);
        }
    }

}
