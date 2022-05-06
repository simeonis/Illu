using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Text.RegularExpressions;


[CustomEditor(typeof(EventListenerManager))]
public class EventListenerManagerEditor : Editor
{
    SerializedProperty eventListenerList;
    ReorderableList list;

    private void OnEnable()
    {
        eventListenerList = serializedObject.FindProperty("listeners");
        list = new ReorderableList(serializedObject, eventListenerList, true, true, true, true);
        list.elementHeightCallback = delegate (int index)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            float elementHeight = EditorGUIUtility.singleLineHeight;
            var margin = EditorGUIUtility.standardVerticalSpacing;

            if (element.isExpanded)
            {
                SerializedProperty responses = element.FindPropertyRelative("Response");

                var height = 162f;
                int length = responses.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize;

                if (length != 0)
                {
                    height += 5f;
                }

                if (length > 1)
                {
                    for (var d = 1; d < length; d++)
                        height += 50f;
                }

                return height + margin;
            }
            return elementHeight;
        };

        list.drawElementCallback = DrawListItems;
        list.drawHeaderCallback = DrawHeader;
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
        myFoldoutStyle.fontSize = 12;
        myFoldoutStyle.normal.textColor = Color.white;

        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        var eventProperty = element.FindPropertyRelative("Event");

        if (eventProperty.objectReferenceValue != null)
        {
            SerializedObject serializedObject = new SerializedObject(eventProperty.objectReferenceValue);
            SerializedProperty propSP = serializedObject.FindProperty("m_Name");

            if (propSP != null)
            {
                Rect tmpRect = new Rect(rect);
                tmpRect.height = EditorGUIUtility.singleLineHeight;
                tmpRect.x = rect.x + 10f;
                var myStringWithSpaces = Regex.Replace(propSP.stringValue, "(?<=[a-z])([A-Z])", " $1");
                element.isExpanded = EditorGUI.Foldout(tmpRect, element.isExpanded, myStringWithSpaces, myFoldoutStyle);
            }
        }

        if (element.isExpanded)
        {
            var eventFieldHeight = 35f;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 10f, rect.width, eventFieldHeight), eventProperty, GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight * 2 + eventFieldHeight), rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Response"), GUIContent.none, true);
        }
    }

    void DrawHeader(Rect rect)
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 15;
        titleStyle.normal.textColor = Color.white;
        EditorGUI.LabelField(rect, "Events", titleStyle);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
