using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StairBuilder))]
public class StairBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StairBuilder builder = (StairBuilder) target;
        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        if (GUILayout.Button(builder.fill ? "Wireframe" : "Fill"))
        {
            builder.fill = !builder.fill;
        }
        if (GUILayout.Button(builder.visualize ? "Hide Visualization" : "Show Visualization"))
        {
            builder.visualize = !builder.visualize;
        }
        if (GUILayout.Button("Build"))
        {
            builder.Build();
        }
    }
}