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

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        Setup();
        DrawVisibleGeometry();
        //You need a submit on the context to draw anything
        context.Submit();
    }

    void Setup()
    {
        context.SetupCameraProperties(camera); //needed to set camera props
    }

    void Submit()
    {
        context.Submit();
    }
    void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera); // a skippable function
    }

}
