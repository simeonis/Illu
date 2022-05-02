using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EventListenerManager : MonoBehaviour
{
    [SerializeField]
    protected List<EventListener2> listeners = new List<EventListener2>();

    private void OnEnable()
    {
        foreach (EventListener2 listener in listeners)
        {
            //listener.Event.RegisterListener(listener);
        }

    }

    private void OnDisable()
    {
        foreach (EventListener2 listener in listeners)
        {
            // listener.Event.UnregisterListener(listener);
        }
    }
}
