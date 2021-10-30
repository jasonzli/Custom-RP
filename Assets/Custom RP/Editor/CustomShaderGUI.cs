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

    bool showPresets;

    public override void OnGUI(
        MaterialEditor materialEditor, MaterialProperty[] porperties
    )
    {
        base.OnGUI(materialEditor, porperties);
        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;

        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
        if (showPresets)
        {
            //Draw the custom GUI preset buttons
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }
    /// <summary>
    // this set of functions describes the material presets for the buttons
    /// </summary>

    //Creating a preset button that both does the undo handling with the editor
    //and also creates the btuton through GUILayout.Button
    bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }

    //separate method per preset
    void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    void TransparentPreset()
    {
        if (PresetButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    /// <summary>
    // a bunch of keyword and property setting functions
    // as well as setters for the properties
    /// </summary>
    bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties, false); //false here tells Unity to not log an error if property not found, giving null
        //We can use the null above the then decide to set a property or not
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }
        return false;//property is null and not found.
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
        //if property is found then set, otherwise ignore.
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
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