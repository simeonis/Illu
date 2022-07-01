using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Illu.Networking;

public class GameManager : MonoBehaviourSingletonDontDestroy<GameManager>
{
    Dictionary<Event, UnityEvent> _events = new Dictionary<Event, UnityEvent>
    {
        { Event.GameStart,        new UnityEvent() },
        { Event.GamePaused,       new UnityEvent() },
        { Event.GameResumed,      new UnityEvent() },
        { Event.GameLeft,         new UnityEvent() },
        { Event.GameLANLeft,      new UnityEvent() },
        { Event.GameModeTraining, new UnityEvent() },
        { Event.GameModeStandard, new UnityEvent() },
    };

    public enum Event
    {
        GameStart,
        GamePaused,
        GameResumed,
        GameLeft,
        GameLANLeft,
        GameModeTraining,
        GameModeStandard,
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
    
    public void StartGame() => TriggerEvent(Event.GameStart);

    public void LeaveGame()
    {
        if (NetworkManager.Instance.isLanConnection)
            TriggerEvent(Event.GameLANLeft);
        else
            TriggerEvent(Event.GameLeft);
    }

    public void Exit() =>  Application.Quit();
}
