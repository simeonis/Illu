using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventListenerManager))]
public class EventListenerManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 15;
        titleStyle.normal.textColor = Color.white;

        GUILayout.Space(10f);
        GUILayout.Label("Events", titleStyle);
        GUILayout.ExpandHeight(true);
        serializedObject.Update();

        EditorList.Show(serializedObject.FindProperty("listeners"));

        serializedObject.ApplyModifiedProperties();
    }
}
