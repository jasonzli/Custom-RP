
using UnityEngine;
using UnityEngine.Rendering;


// Each camera .Render() will draw all the geo for that camera to see.
//We can isolate that in functions
// By having a class for camera renderer we can do whatever we want with
// each camera, allowing us to do different views, deferred, etc.
public partial class CameraRenderer
{
    static ShaderTagId
        unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        litShaderTagId = new ShaderTagId("CustomLit");

    ScriptableRenderContext context;
    Camera camera;

    //Skybox has its own method, but everything else needs a command in the bufer
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };//object initializater syntax!

    CullingResults cullingResults;

    //an instance of lighting
    Lighting lighting = new Lighting();

    public void Render(
        ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance))
        {
            return;
        }

        Setup();
        lighting.Setup(context, cullingResults, shadowSettings);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();

        //cleanup the memory usage
        lighting.Cleanup();

        //You need a submit on the context to draw anything
        Submit();
    }

    void Setup()
    {
        //PUt this setup before clear Render target because it also clears the camera
        context.SetupCameraProperties(camera); //needed to set camera props
                                               //Shows up as a Draw GL in the frame debuggers if separate from SetupCameraProperties
        CameraClearFlags flags = camera.clearFlags;
        //there's four enum flags, 1-4, Skybox, Color, Depth and Nothing
        //there is less and less clearing for each one.

        //Clear colors based on comparison. ClearRenderTarget( bool, bool, color);
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, //if flags is less than depth clear then yes
            flags == CameraClearFlags.Color, //if flags is equal to clear color then yes
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear
            ); //Depth clear, color clear, color to use);

        //Brings the name in the frame debugger
        //Note that Begin and EndSamples have to match ** BY NAME **
        buffer.BeginSample(SampleName); //beginsample is its own command that is executed
        ExecuteBuffer();//this first one is to begin the sample only
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    //again a piece of indirection
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer); //copies commands to the buffer
        buffer.Clear(); //we clear it explicitly to reuse this memory
    }
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //get renderers' settings
        //this is where the draw order is determined!
        var sortingSettings = new SortingSettings(camera) //takes a camera, calculated based on camera
        {
            criteria = SortingCriteria.CommonOpaque //has a bunch of defaults, check docs
        }; //determin ortho or distance sorting

        var drawingSettings = new DrawingSettings( //takes a shader and sorting setting to render
            unlitShaderTagId, sortingSettings
        )
        {
            //dynamic batching settings
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        //add a shader pass
        drawingSettings.SetShaderPassName(1, litShaderTagId);

        //draw opaques first
        var filteringSettings = new FilteringSettings( //filters renderers out
            RenderQueueRange.opaque
        );

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        context.DrawSkybox(camera); // a skippable function

        //draw transparents

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent; //set for transparents

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }



    bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane); //culling params already have a shadow distance for this
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

}
