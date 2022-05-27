using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "Scriptable Object/Event")]
public class Event : ScriptableObject
{
    protected List<EventListener> listeners = new List<EventListener>();

    public virtual void Trigger()
    {
        Debug.Log($"{this.name} has been triggered!");
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }

    public void RegisterListener(EventListener listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(EventListener listener)
    {
        listeners.Remove(listener);
    }
}
