using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoolVariable))]
public class BoolVariableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoolVariable boolVar = (BoolVariable)target;
        if (GUILayout.Button("Toggle"))
        {
            boolVar.Toggle();
        }
    }
}