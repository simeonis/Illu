using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : Interactable
{
    [Header("Display")]
    [SerializeField] Equipment displayObject;

    [Header("Spin Options")]
    [SerializeField] private float rotationPerSecond = 15.0f;
    
    [Header("Hover Options")]
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float frequency = 1f;

    private Vector3 currentPosition = new Vector3();
    private Vector3 currentRotation = new Vector3();

    private EquipmentSlot equipmentSlot;

    void Start()
    {
        equipmentSlot = new EquipmentSlot(transform.GetChild(0));

        if (displayObject)
        {
            displayObject.Disable();
            equipmentSlot.Equip(displayObject);
        }
    }

    void Update()
    {
        if (equipmentSlot.HasEquipment())
        {
            // Hover up & down
            currentPosition.y = 0;
            currentPosition.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

            // Rotate around Y-axis
            currentRotation.y += Time.deltaTime * rotationPerSecond;

            equipmentSlot.ApplyTransform(currentPosition, Quaternion.Euler(currentRotation));
        }
    }

    public override void Interaction(Interactor interactor)
    {
        // Place equipment on pedestal
        if (!equipmentSlot.HasEquipment() && interactor.equipmentSlot.HasEquipment())
        {
            equipmentSlot.TransferFrom(interactor.equipmentSlot);
        }
        // Take equipment from pedestal
        else if (equipmentSlot.HasEquipment() && !interactor.equipmentSlot.HasEquipment())
        {
            interactor.equipmentSlot.TransferFrom(equipmentSlot);
        }
    }

    public override void InteractionCancelled(Interactor interactor) {}
}