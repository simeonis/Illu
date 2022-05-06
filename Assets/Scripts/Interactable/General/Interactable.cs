using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable")] public string interactMessage;
    public abstract void Interact(Interactor interactor);
    public abstract void InteractCancel(Interactor interactor);
}