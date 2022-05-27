using UnityEngine;


[CreateAssetMenu(fileName = "Bool Variable", menuName = "Scriptable Object/Generic Type/Bool")]
public class BoolVariable : Event
{
    public delegate void BoolCallback();
    public BoolCallback Updated;
    private bool _value = false;

    public bool Value
    {
        set
        {
            _value = value;
            this.Trigger();
            Updated?.Invoke();
        }
        get
        {
            return _value;
        }
    }

    public void ToggleValue()
    {
        Value = !Value;
    }
}
