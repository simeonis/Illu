using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static List<GameObject> destroyList = new List<GameObject>();

    void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.LoadScene("MainMenu");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        SceneManager.sceneUnloaded += OnLevelFinishedUnloading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        SceneManager.sceneUnloaded -= OnLevelFinishedUnloading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        Debug.Log($"Scene: {scene.name} was loaded");
    }

    void OnLevelFinishedUnloading(Scene scene) {
        Debug.Log($"Scene: {scene.name} was unloaded");
        foreach (GameObject obj in destroyList) 
        {
            Destroy(obj);
        }
    }

    public static void DestroyOnSceneChange(GameObject obj)
    {
        destroyList.Add(obj);
    }
}
