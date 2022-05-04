using UnityEngine;
using System.Collections.Generic;

// EventListenerManager
// Manages a list of EventListeners on an Object
// Registers them all on enable 
// Deregisters them all on disable 
public class EventListenerManager : MonoBehaviour
{
    [SerializeField]
    protected List<EventListener> listeners = new List<EventListener>();

    private void OnEnable()
    {
        foreach (EventListener listener in listeners)
        {
            listener.Event.RegisterListener(listener);
        }

    }

    private void OnDisable()
    {
        foreach (EventListener listener in listeners)
        {
            listener.Event.UnregisterListener(listener);
        }
    }
}
