using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventListenerManager))]
public class EventListenerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Events", EditorStyles.boldLabel);
        GUILayout.ExpandHeight(true);
        serializedObject.Update();

        EditorList.Show(serializedObject.FindProperty("listeners"));

        serializedObject.ApplyModifiedProperties();
    }
}
