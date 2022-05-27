using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MovingPlatform projection = (MovingPlatform) target;
        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        if (GUILayout.Button(projection.Fill ? "Wireframe" : "Fill"))
        {
            projection.Fill = !projection.Fill;
        }
        if (GUILayout.Button(projection.Visualize ? "Hide Visualization" : "Show Visualization"))
        {
            projection.Visualize = !projection.Visualize;
        }
    }
}