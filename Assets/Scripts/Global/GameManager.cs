using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Illu.Utility;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string rootScene;

    private static Dictionary<string, Event> _events = new Dictionary<string, Event>();

    void Awake()
    {
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

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        TriggerEvent("SceneChanged");
        switch(scene.name)
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
        InputManager.ToggleActionMap(InputManager.playerControls.Land);
        
        // Temporary solution
        // Cinemachine doesn't ignore NEW Input System action maps when disabled
        CameraUtility.singleton.LockCinemachine(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        InputManager.ToggleActionMap(InputManager.playerControls.Menu);
        
        // Temporary solution
        // Cinemachine doesn't ignore NEW Input System action maps when disabled
        CameraUtility.singleton.LockCinemachine(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
