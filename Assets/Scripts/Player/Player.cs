using UnityEngine;
using TMPro;
using Mirror;

public class Player : Interactor
{
    [Range(1f, 50f)] public float dropForce = 5f;
    [HideInInspector] public new Rigidbody rigidbody;

    [Header("UI")]
    [SerializeField] private StringVariable interactMessage;
    private bool interactMessageLocked = false;

    public bool Authority;
    public NetworkConnection networkConnection;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // Fire
        InputManager.playerControls.Land.Fire.performed += context => FirePressed();
        InputManager.playerControls.Land.Fire.canceled += context => FireReleased();

        // Alternate Fire
        InputManager.playerControls.Land.AlternateFire.performed += context => AlternateFirePressed();
        InputManager.playerControls.Land.AlternateFire.canceled += context => AlternateFireReleased();
    }

    protected override void Start()
    {
        base.Start();
        rigidbody = GetComponent<Rigidbody>();
        equipmentSlot.SetLocation(equipmentParent);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Fire
        InputManager.playerControls.Land.Fire.performed -= context => FirePressed();
        InputManager.playerControls.Land.Fire.canceled -= context => FireReleased();

        // Alternate Fire
        InputManager.playerControls.Land.AlternateFire.performed -= context => AlternateFirePressed();
        InputManager.playerControls.Land.AlternateFire.canceled -= context => AlternateFireReleased();
    }

    protected override void Update()
    {
        base.Update();

        if (canInteract && !interactMessageLocked)
        {
            interactMessageLocked = true;
            interactMessage.Value = interactable.interactMessage;
            GameManager.TriggerEvent("PlayerLookingAtInteractable");
        } 
        else if (!canInteract && interactMessageLocked)
        {
            interactMessageLocked = false;
            interactMessage.Value = "";
            GameManager.TriggerEvent("PlayerNotLookingAtInteractable");
        }
    }

    private bool test = false;

    private void FirePressed()
    {
        test = true;
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

    //Sync Equipment
    [Command]
    public void GiveAuthority(NetworkIdentity equipNI)
    {
        equipNI.AssignClientAuthority(connectionToClient);
    }
}
