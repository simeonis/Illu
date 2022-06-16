using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CircleBuilder))]
public class CircleBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CircleBuilder builder = (CircleBuilder) target;
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
