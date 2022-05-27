using UnityEngine;
using UnityEngine.Events;

public abstract class Variable : ScriptableObject
{
    protected UnityEvent _event = new UnityEvent();

    public void AddListener(UnityAction action) => _event.AddListener(action);
    public void RemoveListener(UnityAction action) => _event.RemoveListener(action);
    public void RemoveAllListener() => _event.RemoveAllListeners();

    ~Variable() => _event.RemoveAllListeners();
}
