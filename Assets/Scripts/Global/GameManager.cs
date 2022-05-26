using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    //public static GameManager Instance { get; private set; }
    private Dictionary<string, Event> _events = new Dictionary<string, Event>();

    void Awake()
    {
        Event[] events = Resources.LoadAll<Event>("ScriptableObjects/Event");
        foreach (var e in events) { _events.Add(e.name, e); }
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
