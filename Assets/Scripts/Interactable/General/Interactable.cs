using UnityEngine;
using TMPro;
using Mirror;

public abstract class Interactable : NetworkBehaviour
{
    [Header("Interactable")]
    public string interactMessage;

    [HideInInspector] public new bool enabled = true;
    // private TextMeshProUGUI interactUI;

    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioEvent[] audioEvent;

    // protected virtual void Awake()
    // {
    //     interactUI = GameObject.Find("Interact Message").GetComponent<TextMeshProUGUI>();
    // }

    // public virtual void Seen()
    // {
    //     if (interactUI)
    //     {
    //         interactUI.text = interactMessage;
    //     }
    // }

    public abstract void Interaction(Interactor interactor);

    public abstract void InteractionCancelled(Interactor interactor);
}
