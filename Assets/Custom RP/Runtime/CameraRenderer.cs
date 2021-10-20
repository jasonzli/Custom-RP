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

    CullingResults cullingResults;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry();
        //You need a submit on the context to draw anything
        Submit();
    }

    void Setup()
    {
        //PUt this setup before clear Render target because it also clears the camera
        context.SetupCameraProperties(camera); //needed to set camera props
        //Shows up as a Draw GL in the frame debuggers if separate from SetupCameraProperties
        buffer.ClearRenderTarget(true, true, Color.clear); //Depth clear, color clear, color to use);

        //Brings the name in the frame debugger
        buffer.BeginSample(bufferName); //beginsample is its own command that is executed
        ExecuteBuffer();//this first one is to begin the sample only
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
        context.ExecuteCommandBuffer(buffer); //copies commands to the buffer
        buffer.Clear(); //we clear it explicitly to reuse this memory
    }
    void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera); // a skippable function
    }

    bool Cull()
    {
        if (camera.TryGetCullingParamters(out ScriptableCullpingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

}
