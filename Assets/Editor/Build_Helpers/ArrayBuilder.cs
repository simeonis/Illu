using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ArrayBuilder : EditorWindow
{
    int CountX = 5;
    int CountY = 1;

    int xMult = 1;
    int yMult = 1;
    string relativePath;
    GameObject TheObject;

    [MenuItem("Tools/CustomBuilder/ArrayBuilder")]
    static void Init()
    {
        GameObject go = Selection.activeObject as GameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Select GameObject", "You must select a GameObject first!", "OK");
            return;
        }
        // Get existing open window or if none, make a new one:
        ArrayBuilder window = (ArrayBuilder)EditorWindow.GetWindow(typeof(ArrayBuilder));
        window.Show();
    }

    void OnGUI()
    {
        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        //myString = EditorGUILayout.TextField("Text Field", myString);

        if (GUILayout.Button("Pick Prefab"))
        {
            string path = EditorUtility.OpenFilePanel("Select Model", "/Assets/Models", "prefab");
            relativePath = path.Substring(path.IndexOf("Assets/"));
        }

        CountX = EditorGUILayout.IntField("Count X", CountX);
        CountY = EditorGUILayout.IntField("Count Y", CountY);

        xMult = EditorGUILayout.IntField("X Multiplier", xMult);
        yMult = EditorGUILayout.IntField("Y Multiplier", yMult);

        if (GUILayout.Button("Create"))
        {
            if (relativePath == null)
                EditorUtility.DisplayDialog("Select Prefab", "You must select a Prefab!", "OK");

            if (relativePath.Length != 0)
            {
                //var fileContent = File.ReadAllBytes(path);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);

                TheObject = PrefabUtility.InstantiatePrefab(obj) as GameObject;

                TheObject.transform.parent = Selection.activeGameObject.transform;
            }

            CreateObjects();


        }
    }

    private void CreateObjects()
    {
        if (TheObject == null)
            return;

        Renderer renderer = TheObject.GetComponent<Renderer>();

        if (renderer != null)
        {

            foreach (Transform t in Selection.activeGameObject.transform)
            {
                // Destroy all children
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(t.gameObject);

                };
            }

        }

        float lastX = 0;
        float lastY = 0;

        for (int i = 0; i < CountY; i++)
        {
            for (int j = 0; j < CountX; j++)
            {
                //Positino of the object to create
                Vector3 pos = new Vector3(lastX + Selection.activeGameObject.transform.localPosition.x,
                    lastY + Selection.activeGameObject.transform.localPosition.y,
                    Selection.activeGameObject.transform.localPosition.z);

                // Create object

                GameObject go = Instantiate(TheObject, pos, Quaternion.identity, TheObject.transform.parent) as GameObject;

                go.name = TheObject.name + "_" + i + "_" + j;

                lastX -= (renderer.bounds.size.x * xMult);
            }

            // Reset the x axis position 
            lastX = 0;
            lastY += (renderer.bounds.size.y * yMult);
        }

    }
}


