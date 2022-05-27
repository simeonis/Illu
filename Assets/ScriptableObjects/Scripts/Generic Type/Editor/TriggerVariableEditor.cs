using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriggerVariable))]
public class TriggerEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TriggerVariable gameEvent = (TriggerVariable) target;
        if (GUILayout.Button("Trigger Event"))
        {
            gameEvent.Trigger();
        }
    }
}