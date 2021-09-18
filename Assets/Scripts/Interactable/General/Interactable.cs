using UnityEngine;
using TMPro;
using Mirror;

public abstract class Interactable : NetworkBehaviour
{
    [Header("Interactable")]
    public string interactMessage;

    [HideInInspector] public new bool enabled = true;

    public abstract void Interaction(Interactor interactor);

    public abstract void InteractionCancelled(Interactor interactor);
}
