using UnityEngine;
using UnityEngine.Rendering;


// Each camera .Render() will draw all the geo for that camera to see.
//We can isolate that in functions
// By having a class for camera renderer we can do whatever we want with
// each camera, allowing us to do different views, deferred, etc.
partial class CameraRenderer
{
    partial void DrawUnsupportedShaders();
    //or #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR
    static Material errorMaterial;
    static ShaderTagId[] legacylShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),

    };
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
#endif
}
