using UnityEngine;

[CreateAssetMenu(fileName = "Float Variable", menuName = "Scriptable Object/Generic Type/Float")]
public class FloatVariable : Variable
{
    [SerializeField] float _value = 0.0f;
    public float Value
    {
        get { return _value; }
        set 
        {
            _value = value;
            _event.Invoke();
        }
    }
}