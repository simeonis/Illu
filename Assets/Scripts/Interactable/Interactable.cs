using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected string interactMessage;
    protected Text interactUI;

    protected virtual void Awake()
    {
        interactUI = GameObject.Find("Interact Message").GetComponent<Text>();
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
