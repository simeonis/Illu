using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class EditorList
{
    private static GUILayoutOption miniButtonWidth = GUILayout.Width(50f);

    public static void Show(SerializedProperty list)
    {

        for (int i = 0; i < list.arraySize; i++)
        {
            EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = Color.black;

        if (GUILayout.Button("+", EditorStyles.miniButtonLeft, miniButtonWidth))
        {
            list.InsertArrayElementAtIndex(list.arraySize);
        }
        if (GUILayout.Button("-", EditorStyles.miniButtonRight, miniButtonWidth))
        {
            list.arraySize -= 1;
        }

        EditorGUILayout.EndHorizontal();
    }
}