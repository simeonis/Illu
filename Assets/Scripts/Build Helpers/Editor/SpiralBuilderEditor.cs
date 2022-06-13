using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpiralBuilder))]
public class SpiralBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpiralBuilder builder = (SpiralBuilder) target;
        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        if (GUILayout.Button(builder.Visualize ? "Hide Visualization" : "Show Visualization"))
        {
            builder.Visualize = !builder.Visualize;
        }
        if (GUILayout.Button("Build"))
        {
            builder.Build();
        }
    }
}