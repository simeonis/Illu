using UnityEngine;
using TMPro;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable")]
    public string interactMessage;
    [HideInInspector] public new bool enabled = true;

    private TextMeshProUGUI interactUI;

    protected virtual void Awake()
    {
        interactUI = GameObject.Find("Interact Message").GetComponent<TextMeshProUGUI>();
    }

    public virtual void Seen()
    {
        if (interactUI)
        {
            interactUI.text = interactMessage;
        }
    }

    public abstract void Interaction(Interactor interactor);
    public abstract void InteractionCancelled(Interactor interactor);
}
