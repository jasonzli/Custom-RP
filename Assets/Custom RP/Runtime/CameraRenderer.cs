using UnityEngine;
using UnityEngine.Rendering;


// Each camera .Render() will draw all the geo for that camera to see.
//We can isolate that in functions
// By having a class for camera renderer we can do whatever we want with
// each camera, allowing us to do different views, deferred, etc.
public class CameraRenderer
{
    static Material errorMaterial;
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId[] legacylShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),

    };
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
        DrawUnsupportedShaders();

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
        //get renderers' settings
        //this is where the draw order is determined!
        var sortingSettings = new SortingSettings(camera) //takes a camera, calculated based on camera
        {
            criteria = SortingCriteria.CommonOpaque //has a bunch of defaults, check docs
        }; //determin ortho or distance sorting

        var drawingSettings = new DrawingSettings( //takes a shader and sorting setting to render
            unlitShaderTagId, sortingSettings
        );
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

    void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial =
                new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(
            legacylShaderTagIds[0], new SortingSettings(camera)
        )
        {
            overrideMaterial = errorMaterial
        };
        //SetShaderPassNames adds to the available shaders
        for (int i = 1; i < legacylShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacylShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;


        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

}
