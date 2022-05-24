using UnityEngine;

public class Player : Interactor
{
    [Header("Player Interaction")]
    public Transform playerCamera;
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private float raycastRange;

    [Header("Equipment Slots")]
    [SerializeField] private Transform rightHand;
    private Equipment equipment = null;

    void Awake()
    {

    }

    protected void OnEnable()
    {
        // Interact
        InputManager.Instance.playerControls.Land.Interact.performed += context => Interact();
        InputManager.Instance.playerControls.Land.Interact.canceled += context => InteractCanceled();

        // Fire
        InputManager.Instance.playerControls.Land.Fire.performed += context => FirePressed();
        InputManager.Instance.playerControls.Land.Fire.canceled += context => FireReleased();

        // Alternate Fire
        InputManager.Instance.playerControls.Land.AlternateFire.performed += context => AlternateFirePressed();
        InputManager.Instance.playerControls.Land.AlternateFire.canceled += context => AlternateFireReleased();
    }

    protected void OnDisable()
    {
        // Interact
        InputManager.Instance.playerControls.Land.Interact.performed -= context => Interact();
        InputManager.Instance.playerControls.Land.Interact.canceled -= context => InteractCanceled();

        // Fires
        InputManager.Instance.playerControls.Land.Fire.performed -= context => FirePressed();
        InputManager.Instance.playerControls.Land.Fire.canceled -= context => FireReleased();

        // Alternate Fire
        InputManager.Instance.playerControls.Land.AlternateFire.performed -= context => AlternateFirePressed();
        InputManager.Instance.playerControls.Land.AlternateFire.canceled -= context => AlternateFireReleased();
    }

    protected override void Update()
    {
        base.Update();
        UpdateUI();
    }

    protected override void Interact()
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

    public bool IsEquipped()
    {
        return equipment != null;
    }

    private void FirePressed() => equipment?.EquipmentPrimaryPressed();
    private void FireReleased() => equipment?.EquipmentPrimaryReleased();
    private void AlternateFirePressed() => equipment?.EquipmentSecondaryPressed();
    private void AlternateFireReleased() => equipment?.EquipmentSecondaryReleased();

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
        for (int i = 0; i < collidersFound; i++)
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
