using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviourSingletonDontDestroy<GameManager>
{
    private Dictionary<Event, UnityEvent> _events = new Dictionary<Event, UnityEvent>
    {
        { Event.ServerStart,              new UnityEvent() },
        { Event.ClientStart,              new UnityEvent() },
        { Event.S_ClientConnected,        new UnityEvent() },
        { Event.C_ClientConnected,        new UnityEvent() },
        { Event.C_ClientDisconnected,     new UnityEvent() },
        { Event.ServerStop,               new UnityEvent() },
        { Event.ClientStop,               new UnityEvent() },
        { Event.LanHost,                  new UnityEvent() },
        { Event.LanJoin,                  new UnityEvent() },
        { Event.GameStarted,              new UnityEvent() },
        { Event.GamePaused,               new UnityEvent() },
        { Event.GameResumed,              new UnityEvent() },
        { Event.GameExited,               new UnityEvent() },
        { Event.GameLeft,                 new UnityEvent() },
        { Event.GameModeTraining,         new UnityEvent() },
        { Event.GameModeStandard,         new UnityEvent() },
        { Event.SteamLobbyCreateAttempt,  new UnityEvent() },
        { Event.SteamLobbyCreated,        new UnityEvent() },
        { Event.SteamLobbyEntered,        new UnityEvent() },
        { Event.SteamLobbyInvited,        new UnityEvent() },
        { Event.SteamLobbyLeft,           new UnityEvent() },
        { Event.SteamLobbyDisconnected,   new UnityEvent() },
        { Event.SteamLobbyKicked,         new UnityEvent() },
    };

    public enum Event
    {
        ServerStart,
        ClientStart,
        S_ClientConnected,
        C_ClientConnected,
        C_ClientDisconnected,
        ServerStop,
        ClientStop,
        LanHost,
        LanJoin,
        GameStarted,
        GamePaused,
        GameResumed,
        GameExited,
        GameLeft,
        GameModeTraining,
        GameModeStandard,
        SteamLobbyCreated,
        SteamLobbyCreateAttempt,
        SteamLobbyEntered,
        SteamLobbyInvited,
        SteamLobbyLeft,
        SteamLobbyKicked,
        SteamLobbyDisconnected,
        SteamFriendsRequested
    }


    //internal events to listen to
    // pause 
    // resume -> raises play and call resume 
    // exit -> exit 
    // left -> screen controller root  / change scene to main menu // raise steasm lobby left 
    // stopped nothing 
    // training -> raises gamestarted 
    // standard -> 


    public override void Awake()
    {
        base.Awake();
        // Event[] events = Resources.LoadAll<Event>("ScriptableObjects/Event");
        // foreach (var e in events) { _events.Add(e.name, e); }

        AddListener(Event.GameResumed, Resume);
        AddListener(Event.GameExited, Resume);
        AddListener(Event.GameLeft, Resume);
        AddListener(Event.GameModeTraining, StartGame);
    }

    public void TriggerEvent(Event name)
    {
        Debug.Log("TriggerEvent" + name.ToString());
        _events[name].Invoke();
    }

    public void TriggerEvent(string name)
    {
      Event.TryParse(name, out Event requestedEvent);
      TriggerEvent(requestedEvent);
    }

    public void AddListener(Event name, UnityAction listener) => _events[name].AddListener(listener);
    public void RemoveListener(Event name, UnityAction listener) => _events[name].RemoveListener(listener);
    

    public void Resume()
    {
        //TriggerEvent(Play?)
        InputManager.Instance.TogglePlayer();
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

    public void ChangeScene(string scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
    
    void StartGame()
    {
        TriggerEvent(Event.GameStarted);
    }

    void LeaveGame()
    {
        ScreenController.Instance.ChangeScreen(ScreenController.Screen.Root);
        ChangeScene("MainMenu");
        TriggerEvent(Event.SteamLobbyLeft);
    }

}
