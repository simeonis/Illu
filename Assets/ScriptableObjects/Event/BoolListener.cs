using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BoolListener : MonoBehaviour
{
    [SerializeField] private BoolVariable boolVariable;

    public UnityEvent ResponseWhenTrue;
    public UnityEvent ResponseWhenFalse;

    void OnEnable()
    {
        boolVariable.Updated += handleUpdate;
        Debug.Log("OnEnable");
    }

    void OnDisable()
    {
        boolVariable.Updated -= handleUpdate;
        Debug.Log("OnEnableDisable");
    }

    private void handleUpdate()
    {
        Debug.Log("Handle Update " + boolVariable.Value);
        if (boolVariable.Value)
        {
            Debug.Log("Truee");
            ResponseWhenTrue.Invoke();
        }
        else
        {
            Debug.Log("Very False");
            ResponseWhenFalse.Invoke();
        }
    }
}
