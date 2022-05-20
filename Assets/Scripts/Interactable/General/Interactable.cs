using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable")] public string interactMessage;
    public abstract void Interact(Interactor interactor);
    public abstract void InteractCancel(Interactor interactor);

    protected virtual void Start()
    {
        ChangeChildrenLayerMask(transform, "Interactable", true);
    }

    /* 
     * Modifies layer of each child inside of parent.
     * If inclusive is true, parent's layer is also modified.
     * Useful for Camera Culling Mask.
     */
    protected void ChangeChildrenLayerMask(Transform parent, string layer, bool inclusive)
    {
        if (inclusive) ChangeLayerMask(parent, layer);
        foreach (Transform child in parent.transform)
        {
            if (child == null) continue;
            ChangeChildrenLayerMask(child, layer, true);
        }
    }

    /*
     * Modifies layer of specific element without modifying it's children.
     */
    protected void ChangeLayerMask(Transform element, string layer)
    {
        element.gameObject.layer = LayerMask.NameToLayer(layer);
    }
}