using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Equipment : Interactable
{
    [HideInInspector] public Rigidbody equipmentBody;

    void Start()
    {
        equipmentBody = GetComponent<Rigidbody>();
    }

    public override void Interact(Interactor interactor)
    {
        Debug.Log($"INTERACTION_INTERACT: {transform.name}");
    }

    public override void InteractCancel(Interactor interactor)
    {
        Debug.Log($"INTERACTION_CANCELLED: {transform.name}");
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

    public void EquipmentPrimaryPressed() { Debug.Log("Equipment Primary Pressed"); }
    public void EquipmentPrimaryReleased() { Debug.Log("Equipment Primary Released"); }
    public void EquipmentSecondaryPressed() { Debug.Log("Equipment Secondary Pressed"); }
    public void EquipmentSecondaryReleased() { Debug.Log("Equipment Secondary Released"); }
}
