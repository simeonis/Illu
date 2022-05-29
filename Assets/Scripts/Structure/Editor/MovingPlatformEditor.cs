using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform)), CanEditMultipleObjects]
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

    protected virtual void OnSceneGUI()
    {
        MovingPlatform projection = (MovingPlatform) target;
        
        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(projection.TargetPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(projection, "Change Moving Platform Target Position");
            projection.TargetPosition = newTargetPosition;
        }
    }
}