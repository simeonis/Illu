using UnityEngine;
using TMPro;

public class Player : Interactor
{
    [Range(1f, 50f)] public float dropForce = 5f;
    [HideInInspector] public new Rigidbody rigidbody;
    private TextMeshProUGUI interactUI;

    protected override void Awake()
    {
        base.Awake();

        // Fire
        playerControls.Land.Fire.performed += context => FirePressed();
        playerControls.Land.Fire.canceled += context => FireReleased();

        // Alternate Fire
        playerControls.Land.AlternateFire.performed += context => AlternateFirePressed();
        playerControls.Land.AlternateFire.canceled += context => AlternateFireReleased();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UIManager.OnPlayScreen += playerHUD => LoadInteractUI(playerHUD);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UIManager.OnPlayScreen -= playerHUD => LoadInteractUI(playerHUD);
        interactUI = null;
    }

    private void LoadInteractUI(PlayerHUD playerHUD)
    {
        interactUI = playerHUD.interactUI;
    }

    protected override void Start()
    {
        base.Start();
        rigidbody = GetComponent<Rigidbody>();
        equipmentSlot.SetLocation(equipmentParent);
    }

    protected override void Update()
    {
        base.Update();

        if (interactUI)
        {
            interactUI.text = canInteract ? interactable.interactMessage : "";
        }
    }

    private void FirePressed()
    {
        if (equipmentSlot.HasEquipment())
        {
            equipmentSlot.GetEquipment().EquipmentPrimaryPressed();
        }
    }

    private void FireReleased()
    {
        if (equipmentSlot.HasEquipment())
        {
            equipmentSlot.GetEquipment().EquipmentPrimaryReleased();
        }
    }

    private void AlternateFirePressed()
    {
        if (equipmentSlot.HasEquipment())
        {
            equipmentSlot.GetEquipment().EquipmentSecondaryPressed();
        }
    }

    private void AlternateFireReleased()
    {
        if (equipmentSlot.HasEquipment())
        {
            equipmentSlot.GetEquipment().EquipmentSecondaryReleased();
        }
    }
}
