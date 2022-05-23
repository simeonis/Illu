using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string rootScene;

    private static Dictionary<string, Event> _events = new Dictionary<string, Event>();
    private bool loaded = false;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // These are loading twice doesn't work in the else !!
        // Load all Events into static dictionary for easy access
        var events = Resources.LoadAll<Event>("ScriptableObjects/Event");
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

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        TriggerEvent("SceneChanged");
        switch (scene.name)
        {
            case "Main Menu":
                TriggerEvent("SceneMenu");
                break;
            default:
                break;
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public void Resume()
    {
        InputManager.ToggleActionMap(InputManager.Instance.playerControls.Land);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        InputManager.ToggleActionMap(InputManager.Instance.playerControls.Menu);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
