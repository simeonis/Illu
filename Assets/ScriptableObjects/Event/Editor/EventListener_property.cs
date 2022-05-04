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

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        var eventProperty = property.FindPropertyRelative("Event");

        GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);

        myFoldoutStyle.fontSize = 12;
        myFoldoutStyle.normal.textColor = Color.white;

        if (eventProperty != null)
        {
            if (eventProperty.objectReferenceValue != null)
            {
                SerializedObject serializedObject = new SerializedObject(eventProperty.objectReferenceValue);
                SerializedProperty propSP = serializedObject.FindProperty("m_Name");

                if (propSP != null)
                {
                    var myStringWithSpaces = Regex.Replace(propSP.stringValue, "([A-Z])([a-z]*)", " $1$2");
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, myStringWithSpaces, myFoldoutStyle);
                }
            }
            else
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, "No Event Assigned", myFoldoutStyle);
            }


            if (property.isExpanded)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.PropertyField(eventProperty, GUIContent.none, GUILayout.Height(40));
                SerializedProperty responses = property.FindPropertyRelative("Response");

                EditorGUILayout.PropertyField(responses, GUIContent.none, true);

                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
                GUILayout.Box(GUIContent.none, box, GUILayout.Height(1f));
            }

            EditorGUILayout.EndVertical();
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;


    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight - 15;
    }

}
