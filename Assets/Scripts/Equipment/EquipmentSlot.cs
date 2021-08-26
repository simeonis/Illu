using UnityEngine;

public class EquipmentSlot
{
    public Equipment equipment = null;
    private Transform equipmentLocation;

    public EquipmentSlot(Transform location)
    {
        this.equipmentLocation = location;
    }

    public void SetLocation(Transform location)
    {
        this.equipmentLocation = location;
    }

    public void Equip(Equipment equipment)
    {
        this.equipment = equipment;
        this.equipment.transform.SetParent(equipmentLocation);
    }

    public Equipment Unequip()
    {
        Equipment temp = this.equipment;
        this.equipment = null;
        return temp;
    }

    public void TransferFrom(EquipmentSlot other)
    {
        Equip(other.Unequip());

        equipment.transform.localPosition = Vector3.zero;
        equipment.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public Equipment GetEquipment()
    {
        return equipment;
    }

    public void ApplyTransform(Vector3 targetPosition = default(Vector3), Quaternion targetRotation = default(Quaternion))
    {
        if (equipment)
        {
            equipment.transform.localPosition = targetPosition;
            equipment.transform.localRotation = targetRotation;
        }
    }

    public bool HasEquipment()
    {
        return equipment != null;
    }
}
