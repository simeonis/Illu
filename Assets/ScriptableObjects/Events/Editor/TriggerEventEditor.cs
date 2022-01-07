using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Event))]
public class TriggerEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Event gameEvent = (Event) target;
        if (GUILayout.Button("Trigger Event"))
        {
            gameEvent.Trigger();
        }
    }
}