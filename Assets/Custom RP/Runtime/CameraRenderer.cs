using UnityEngine;
using UnityEngine.Rendering;


// Each camera .Render() will draw all the geo for that camera to see.
//We can isolate that in functions
// By having a class for camera renderer we can do whatever we want with
// each camera, allowing us to do different views, deferred, etc.
public class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;

    //Skybox has its own method, but everything else needs a command in the bufer
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };//object initializater syntax!

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        Setup();
        DrawVisibleGeometry();
        //You need a submit on the context to draw anything
        Submit();
    }

    void Setup()
    {
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        context.SetupCameraProperties(camera); //needed to set camera props
    }

    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    //again a piece of indirection
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear(); //we clear it explicitly to reuse this memory
    }
    void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera); // a skippable function
    }

}
