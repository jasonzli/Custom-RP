using UnityEngine;
using UnityEngine.Rendering;

public class CustomeRenderPipeline : RenderPipeline
{
    //RenderPipeline has a protected Render method that we override to make this pipeline work
    //This is what is called in the custom pipline *asset*
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    {

    }

}
