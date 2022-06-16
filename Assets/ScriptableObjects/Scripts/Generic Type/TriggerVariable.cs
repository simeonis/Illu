using UnityEngine;

[CreateAssetMenu(fileName = "Trigger Variable", menuName = "Scriptable Object/Generic Type/Trigger")]
public class TriggerVariable : Variable
{
    public void Trigger() => _event.Invoke();
}
