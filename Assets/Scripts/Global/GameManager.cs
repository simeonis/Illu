using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Dictionary<string, Event> _events = new Dictionary<string, Event>();

    void Awake()
    {
        if (Instance != null) 
        {
            // There exist an instance, and it is not me, kill...
            if (Instance != this) 
                Destroy(gameObject);
            return;
        }

        // There does not exist an instance, create...
        Instance = this;
        Event[] events = Resources.LoadAll<Event>("ScriptableObjects/Event");
        foreach (var e in events) { _events.Add(e.name, e); }
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerEvent(string name)
    {
        _events[name].Trigger();
    }

    public void Resume()
    {
        InputManager.Instance.ToggleLand();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        InputManager.Instance.ToggleMenu();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
