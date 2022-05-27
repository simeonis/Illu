using UnityEngine;

[CreateAssetMenu(fileName = "String Variable", menuName = "Scriptable Object/Generic Type/String")]
public class StringVariable : Variable
{
    [SerializeField] string _value = "";
    public string Value
    {
        get { return _value; }
        set 
        {
            _value = value;
            _event.Invoke();
        }
    }

    public bool IsEmpty() => _value == "";
    public bool Equals(string value) => _value == value;
}
