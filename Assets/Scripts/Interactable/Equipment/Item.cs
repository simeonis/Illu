using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : Interactable
{
    [HideInInspector] public Rigidbody equipmentBody;
    [HideInInspector] public Collider equipmentCollider;

    protected override void Start()
    {
        base.Start();
        equipmentBody = GetComponent<Rigidbody>();
        equipmentCollider = GetComponent<Collider>();
    }

    public override void Interact(Interactor interactor)
    {
        if (interactor is Player)
        {
            Player player = interactor as Player;

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
}
