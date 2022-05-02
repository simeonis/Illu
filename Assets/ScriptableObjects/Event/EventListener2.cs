using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventListener2
{
    public Event Event;
    public UnityEvent Response;

    public void OnEventTriggered()
    {
        Response.Invoke();
    }
}
