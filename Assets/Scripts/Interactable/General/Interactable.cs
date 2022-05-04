using UnityEngine;
using Mirror;

public abstract class Interactable : NetworkBehaviour
{
    [Header("Interactable")]
    public string interactMessage;

    [HideInInspector] public new bool enabled = true;

    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioEvent[] audioEvent;

    public abstract void Interaction(Interactor interactor);

    public abstract void InteractionCancelled(Interactor interactor);
}
