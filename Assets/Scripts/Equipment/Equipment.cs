using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Equipment : Interactable
{
    public Rigidbody equipmentBody;
    protected Collider equipmentCollider;
    protected bool isEquipped = false;
    public Transform defaultParent;

    //adding SyncEquipment
    private SyncEquipment syncEquipment;

    protected override void Awake()
    {
        equipmentBody = GetComponent<Rigidbody>();
        equipmentCollider = GetComponent<Collider>();
        defaultParent = transform.parent;

        //Added for SyncEquipment 
        syncEquipment = GetComponent<SyncEquipment>();

    }

    public override void Interaction(Interactor interactor)
    {
        if (!interactor.equipmentSlot.HasEquipment())
        {
            interactor.equipmentSlot.Equip(this);
            Disable();
        }
        else
        {
            interactor.equipmentSlot.Unequip();
            Enable();

            if (interactor.TryGetComponent(out Player player))
            {
                player.syncEquipment.RegisterEquipment(this);

                if (player.hasAuthority)
                {
                    Debug.Log("Player Authority");

                    player.syncEquipment.SendAction(player.source.forward, player.dropForce, player.rigidbody.velocity);
                    AddForce(player.source.forward, player.dropForce, player.rigidbody.velocity);
                }
                else
                {
                    Debug.Log("Player No Authority");
                }
            }
        }
    }

    public override void OnStartAuthority()
    {
        Debug.Log("I now have authority");
    }

    public override void InteractionCancelled(Interactor interactor) { }

    public void Disable()
    {
        if (isEquipped) return;

        isEquipped = true;

        // Disable rigidbody
        equipmentBody.isKinematic = true;
        equipmentBody.interpolation = RigidbodyInterpolation.None;

        // Disable collider
        equipmentCollider.enabled = false;

        // Reseting transform
        transform.localPosition = new Vector3();
        transform.localRotation = Quaternion.Euler(new Vector3());

        // Modify Layer Mask to render equipment on-top
        ChangeChildrenLayerMask(transform, "Equipment", true);
    }

    public void Enable()
    {
        if (!isEquipped) return;

        isEquipped = false;
        transform.SetParent(defaultParent);

        // Enable rigidbody
        equipmentBody.isKinematic = false;
        equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;

        // Enable collider
        equipmentCollider.enabled = true;

        // Modify Layer Mask back to default
        ChangeChildrenLayerMask(transform, "Default", true);
    }

    public void AddForce(Vector3 direction, float force, Vector3 currVel = new Vector3())
    {
        float random = force * 2.0f;
        // float random = Random.Range(-1f, 1f) * force * 2.0f;
        equipmentBody.velocity = currVel;
        equipmentBody.AddForce(direction * force, ForceMode.Impulse); // Movement force
        equipmentBody.AddTorque(new Vector3(random, random, random)); // Rotation throw force
    }

    public void AddCorrectionalForce(Vector3 direction)
    {
        //float random = force * 2.0f;
        // float random = Random.Range(-1f, 1f) * force * 2.0f;
        //equipmentBody.velocity = currVel;
        equipmentBody.AddForce(direction, ForceMode.Force); // Movement force
        //equipmentBody.AddTorque(new Vector3(random, random, random)); // Rotation throw force
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
