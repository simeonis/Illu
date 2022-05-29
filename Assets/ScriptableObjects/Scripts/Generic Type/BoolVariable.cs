using UnityEngine;


[CreateAssetMenu(fileName = "Bool Variable", menuName = "Scriptable Object/Generic Type/Bool")]
public class BoolVariable : Variable
{
    [SerializeField] bool _value = false;
    public bool Value
    {
        get { return _value; }
        set
        {
            _value = value;
            _event.Invoke();
        }
    }
    public void Toggle() { Value = !Value; }
}