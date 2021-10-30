using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public class CustomShaderGUI : CustomShaderGUI
{
    public override void OnGUI(
        MaterialEdtiro materialEdtor, MaterialProperty[] porperties
    )
    {
        base.OnGUI(materialEditor, porperties);
    }
}