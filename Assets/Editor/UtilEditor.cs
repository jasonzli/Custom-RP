using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Utils))]
public class UtilEditor : Editor
{
    private Utils script;

    private void OnEnable()
    {
        script = target as Utils;
    }
    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Take Screenshot"))
        {
            script.Snapshot();
        }
        base.OnInspectorGUI();
    }
}
