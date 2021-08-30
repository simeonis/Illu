using UnityEngine;
using Mirror;
using TMPro;

public abstract class Interactable : NetworkBehaviour
{
    [Header("Interactable")]
    public string interactMessage;
    [HideInInspector] public new bool enabled = true;

    private NetworkIdentity networkIdentity;
    private TextMeshProUGUI interactUI;

    protected virtual void Awake()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
        interactUI = GameObject.Find("Interact Message").GetComponent<TextMeshProUGUI>();
    }

    public virtual void Seen()
    {
        if (interactUI)
        {
            interactUI.text = interactMessage;
        }
    }

    public virtual void Interaction(Interactor interactor)
    {
        interactor.GetAuthority(networkIdentity);
    }

    public virtual void InteractionCancelled(Interactor interactor)
    {
        interactor.RemoveAuthority(networkIdentity);
    }

    public abstract override void OnStartAuthority();

}
