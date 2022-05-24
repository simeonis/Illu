using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Equipment : Interactable
{
    [HideInInspector] public Rigidbody equipmentBody;
    [HideInInspector] public Collider equipmentCollider;
    protected Player player;

    protected override void Start()
    {
        equipmentBody = GetComponent<Rigidbody>();
        equipmentCollider = GetComponent<Collider>();
        ChangeChildrenLayerMask(transform, "Equipment", true);
    }

    public override void Interact(Interactor interactor)
    {
        if (interactor is Player)
        {
            player = interactor as Player;

            // Drop
            if (player.IsEquipped())
            {
                // 1. Remove equipment from interactor's hand
                player.Drop(this);
                // 2. Enable rigidbody
                equipmentBody.isKinematic = false;
                equipmentBody.useGravity = true;
                // 3. Enable collider
                equipmentCollider.enabled = true;
                // 4. Remove reference to player
                player = null;
            }
            // Pick-Up
            else
            {
                // 1. Disable rigidbody
                equipmentBody.isKinematic = true;
                equipmentBody.useGravity = false;
                // 2. Disable collider
                equipmentCollider.enabled = false;
                // 3. Move equipment to interactor's hand
                player.Equip(this);
            }
        }
    }

    public override void InteractCancel(Interactor interactor) {}

    public virtual void EquipmentPrimaryPressed() { Debug.Log("Equipment Primary Pressed"); }
    public virtual void EquipmentPrimaryReleased() { Debug.Log("Equipment Primary Released"); }
    public virtual void EquipmentSecondaryPressed() { Debug.Log("Equipment Secondary Pressed"); }
    public virtual void EquipmentSecondaryReleased() { Debug.Log("Equipment Secondary Released"); }
}
