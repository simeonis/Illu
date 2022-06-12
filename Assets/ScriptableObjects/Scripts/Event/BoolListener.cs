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
        boolVariable.AddListener(handleUpdate);
    }

    private void handleUpdate()
    {
        if (boolVariable.Value)
        {
            ResponseWhenTrue.Invoke();
        }
        else
        {
            ResponseWhenFalse.Invoke();
        }
    }
}
