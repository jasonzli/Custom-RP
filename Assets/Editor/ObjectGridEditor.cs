using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectGrid))]
public class ObjectGridEditor : Editor
{
    private ObjectGrid script;

    private void OnEnable()
    {
        script = target as ObjectGrid;
    }
    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Create Objects"))
        {
            script.CreateObjects();
        }
        base.OnInspectorGUI();
    }
}
