using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EventListenerManager : MonoBehaviour
{
    [SerializeField]
    protected List<EventListenerNew> listeners = new List<EventListenerNew>();

    private void OnEnable()
    {
        foreach (EventListenerNew listener in listeners)
        {
            //listener.Event.RegisterListener(listener);
        }

    }

    private void OnDisable()
    {
        foreach (EventListenerNew listener in listeners)
        {
            // listener.Event.UnregisterListener(listener);
        }
    }
}
