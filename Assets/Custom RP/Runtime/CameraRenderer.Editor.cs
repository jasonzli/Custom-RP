using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


//This class contains all the render functinality for Editor only 
//and maybe development builds
partial class CameraRenderer
{
    //declare empty stuff here and then full bodies in the #if section
    partial void PrepareBuffer();
    partial void PrepareForSceneWindow();
    partial void DrawGizmos();
    partial void DrawUnsupportedShaders();


    //or #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR

    string SampleName {get; set;}

    static Material errorMaterial;
    static ShaderTagId[] legacylShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),

    };

    partial void PrepareBuffer(){
        Profiler.BeginSample("Editor Only");
        //this is the save on memory allocations
        buffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }

    partial void PrepareForSceneWindow(){
        //adds the UI to world geometry
        if (camera.cameraType == CameraType.SceneView){
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }

    partial void DrawGizmos(){
        if(Handles.ShouldRenderGizmos()){
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    //This allows a method to exist only in the editor
    partial void DrawUnsupportedShaders()
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

#else
    string SampleName => bufferName;
#endif
}
