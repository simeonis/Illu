using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour
{
    public Event Event;
    public UnityEvent Response;

    private void OnEnable()
    { 
        Event.RegisterListener(this); 
    }

    private void OnDisable()
    { 
        Event.UnregisterListener(this); 
    }

    public void OnEventTriggered()
    { 
        Response.Invoke();
    }
}
