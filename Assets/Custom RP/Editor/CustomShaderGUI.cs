using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


//A class to handle the custom GUI
//We support multiple material rendering modes, and this will have one shader
//But the multiple properties can be set as presets.

public class CustomShaderGUI : ShaderGUI
{
    MaterialEditor editor;
    Object[] materials; //because materialEditor.targets is Object[] so we match it here
    MaterialProperty[] properties;

    public override void OnGUI(
        MaterialEditor materialEditor, MaterialProperty[] porperties
    )
    {
        base.OnGUI(materialEditor, porperties);
        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;
    }

    void SetProperty(string name, float value)
    {
        FindProperty(name, properties).floatValue = value;
    }
    void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach (Material m in materials)
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material m in materials)
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    //A combined SetProperty for when we have a shader_feature keyword we can use
    void SetProperty(string name, string keyword, bool value)
    {
        SetProperty(name, value ? 1f : 0f);
        SetKeyword(keyword, value);
    }

    //A group of functions that set shader properties
    bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }

    bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach (Material m in materials)
            {
                m.renderQueue = (int)value;
            }
        }
    }
}