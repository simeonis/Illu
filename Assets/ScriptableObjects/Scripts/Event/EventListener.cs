using UnityEngine.Events;

//Manages a reference to an Event and Response
//When an event is triggered it invokes the responses 
[System.Serializable]
public class EventListener
{
    public Event Event;
    public UnityEvent Response;

    public void OnEventTriggered()
    {
        Response.Invoke();
    }
}
