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

[CustomEditor(typeof(SteamEvent))]
public class TriggerSteamEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Event gameEvent = (SteamEvent) target;
        if (GUILayout.Button("Trigger Event"))
        {
            gameEvent.Trigger();
        }
    }
}