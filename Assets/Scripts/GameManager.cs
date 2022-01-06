using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    [Scene]
    private string rootScene;

    void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.LoadScene(rootScene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnLevelFinishedLoading;
    //     SceneManager.sceneUnloaded += OnLevelFinishedUnloading;
    // }

    // void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    //     SceneManager.sceneUnloaded -= OnLevelFinishedUnloading;
    // }

    // void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
    //     Debug.Log($"Scene: {scene.name} was loaded");
    // }

    // void OnLevelFinishedUnloading(Scene scene) {
    //     Debug.Log($"Scene: {scene.name} was unloaded");
    // }

    public enum Level
    {
        Menu, Level1, Level2, Level3, TestingRange
    }
}
