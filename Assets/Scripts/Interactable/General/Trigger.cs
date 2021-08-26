using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    public virtual void Activate() { }
    public virtual void Activate(Illu_Interactable.Button button) { }
}