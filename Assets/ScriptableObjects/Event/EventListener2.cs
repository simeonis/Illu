using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventListenerNew
{
    public Event Event;
    public UnityEvent Response;

    public void OnEventTriggered()
    {
        Response.Invoke();
    }
}
