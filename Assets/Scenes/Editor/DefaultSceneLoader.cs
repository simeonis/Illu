#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using Illu.Utility;

[InitializeOnLoadAttribute]
public static class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (state == PlayModeStateChange.EnteredPlayMode && CameraUtility.singleton == null)
        {
            EditorSceneManager.LoadScene(0);
        }
    }
}
#endif