using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Interactor
{
    [Header("Player Interaction")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float raycastRange;
    private PlayerHUD playerHUD;

    [Header("Equipment Slots")]
    [SerializeField] private Transform rightHand;
    private Equipment equipment = null;

    void Start()
    {
        playerHUD = FindObjectOfType<PlayerHUD>();
        if (!playerHUD)
            Debug.Log("No PlayerHUD could be found");
    }

    protected void OnEnable()
    {
        // Interact
        InputManager.playerControls.Land.Interact.started += Interact;
        InputManager.playerControls.Land.Interact.canceled += InteractCanceled;

        // Fire
        InputManager.playerControls.Land.Fire.started += FirePressed;
        InputManager.playerControls.Land.Fire.canceled += FireReleased;

        // Alternate Fire
        InputManager.playerControls.Land.AlternateFire.started += AlternateFirePressed;
        InputManager.playerControls.Land.AlternateFire.canceled += AlternateFireReleased;
    }

    protected void OnDisable()
    {
        // Interact
        InputManager.playerControls.Land.Interact.started -= Interact;
        InputManager.playerControls.Land.Interact.canceled -= InteractCanceled;

        // Fire
        InputManager.playerControls.Land.Fire.started -= FirePressed;
        InputManager.playerControls.Land.Fire.canceled -= FireReleased;

        // Alternate Fire
        InputManager.playerControls.Land.AlternateFire.started -= AlternateFirePressed;
        InputManager.playerControls.Land.AlternateFire.canceled -= AlternateFireReleased;
    }

    protected override void Update()
    {
        base.Update();
        UpdateUI();
    }

    // OVERRIDE
    void Interact(InputAction.CallbackContext context)
    {
        // GameManager.TriggerEvent("PlayerInteracting");
        playerHUD.RotateCrosshair();

        // Interactable in range
        if (GetInteractable(out var interactable))
        {
            if (IsEquipped() && (interactable as Equipment)) equipment.Interact(this);
            else 
            {
                state = InteractorState.Interacting;
                interactable.Interact(this);
            }
        }
        // No interactable in range & holding equipment
        else if (IsEquipped())
        {
            equipment.Interact(this);
        }
    }

    void InteractCanceled(InputAction.CallbackContext context) => base.InteractCanceled();

    public void Equip(Equipment equipment)
    {
        equipment.transform.SetParent(rightHand, false);
        equipment.transform.localPosition = Vector3.zero;
        equipment.transform.localRotation = Quaternion.identity;
        this.equipment = equipment;
    }

    public void Drop(Equipment equipment)
    {
        if (this.equipment == equipment)
        {
            this.equipment.transform.parent = null;
            this.equipment = null;
        }
    }

    public bool IsEquipped() => equipment != null;

    public Transform GetViewpoint() => playerCamera;

    private void FirePressed(InputAction.CallbackContext context) => equipment?.EquipmentPrimaryPressed();
    private void FireReleased(InputAction.CallbackContext context) => equipment?.EquipmentPrimaryReleased();
    private void AlternateFirePressed(InputAction.CallbackContext context) => equipment?.EquipmentSecondaryPressed();
    private void AlternateFireReleased(InputAction.CallbackContext context) => equipment?.EquipmentSecondaryReleased();

    private void UpdateUI()
    {
        if (GetInteractable(out var interactable) && !playerHUD.CrosshairTextEqual(interactable.interactMessage))
        {
            // GameManager.TriggerEvent("PlayerLookingAtInteractable");
            playerHUD.SetCrosshairText(interactable.interactMessage);
        }
        else if (!GetInteractable(out _) && !playerHUD.CrosshairTextEqual(""))
        {
            // GameManager.TriggerEvent("PlayerNotLookingAtInteractable");
            playerHUD.ClearCrosshairText();
        }
    }

    // Validate colliders that are within 90 degrees of camera's forward vector
    protected override int ValidateCollider(Collider[] colliders, int collidersFound, out Collider[] validatedColliders)
    {
        validatedColliders = new Collider[collidersFound];

        int validIndex = 0;
        int validAmount = 0;
        for (int i=0; i < collidersFound; i++)
        {
            Vector3 playerToCollision = colliders[i].transform.position - transform.position;
            if (Vector3.Angle(playerToCollision, playerCamera.forward) <= 90f)
            {
                validatedColliders[validIndex++] = colliders[i];
                validAmount++;
            }
        }

        return validAmount;
    }

    private RaycastHit directHit;
    private float raycastDistance;
    protected override void SearchInteractable()
    {
        raycastDistance = Vector3.Distance(playerCamera.position, transform.position) + raycastRange;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out directHit, raycastDistance, layers))
        {
            colliderInteractable = directHit.collider;
            cachedInteractable = null;
        }
        else
        {
            base.SearchInteractable();
        }
    }

    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (enable)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerCamera.position, playerCamera.forward * raycastDistance);
        }
    }
    #endif
}
