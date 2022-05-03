using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

[CustomPropertyDrawer(typeof(EventListenerNew))]
public class EventListener2_PropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var consoleBackground = new Texture2D(1000, 1, TextureFormat.RGBAFloat, false);

        GUIStyle box = new GUIStyle();
        box.normal.background = consoleBackground;

        EditorGUILayout.BeginVertical();

        GUIStyle SectionNameStyle = new GUIStyle();
        SectionNameStyle.fontSize = 35;

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        var p = property.FindPropertyRelative("Event");

        GUIStyle g = new GUIStyle();
        g.fontSize = 20;
        g.normal.textColor = Color.white;

        if (p != null)
        {
            EditorGUILayout.BeginHorizontal();

            if (p.objectReferenceValue != null)
            {
                SerializedObject serializedObject = new SerializedObject(p.objectReferenceValue);
                SerializedProperty propSP = serializedObject.FindProperty("m_Name");

                if (propSP != null)
                {
                    var myStringWithSpaces = Regex.Replace(propSP.stringValue, "([A-Z])([a-z]*)", " $1$2");
                    EditorGUILayout.TextField(myStringWithSpaces, g, GUILayout.Height(30));
                }
            }
            else
            {
                EditorGUILayout.TextField("No Event Assigned", g, GUILayout.Height(30));
            }


            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(p, GUIContent.none, GUILayout.Height(40));

            SerializedProperty responses = property.FindPropertyRelative("Response");

            responses.isExpanded = EditorGUILayout.Foldout(responses.isExpanded, "Responses()");

            if (responses.isExpanded)
                EditorGUILayout.PropertyField(responses, GUIContent.none, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Box(GUIContent.none, GUILayout.Height(10));
            GUILayout.Box(GUIContent.none, box, GUILayout.Height(1f));
            EditorGUILayout.EndVertical();
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;


    }

}
