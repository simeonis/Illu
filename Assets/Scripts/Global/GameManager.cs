using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{
    [Scene]
    [SerializeField] private string rootScene;

    private static Dictionary<string, Event> _events = new Dictionary<string, Event>();

    void Awake()
    {
        // Load all Events into static dictionary for easy access
        var events = Resources.LoadAll<Event>("ScriptableObjects/Events");
        foreach (var e in events)
        {
            _events.Add(e.name, e);
        }

        DontDestroyOnLoad(this);
        SceneManager.LoadScene(rootScene);
    }

    public static void TriggerEvent(string name)
    {
        _events[name].Trigger();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        TriggerEvent("SceneChanged");
        if (scene.name == "Main Menu")
        {
            TriggerEvent("SceneMenu");
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Resume()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Land);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Menu);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
