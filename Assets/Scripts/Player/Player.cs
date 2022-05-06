using UnityEngine;

public class Player : Interactor
{
    [Header("Player Interaction")]
    [SerializeField] private float raycastRange;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private StringVariable message;

    protected void OnEnable()
    {
        // Interact
        InputManager.playerControls.Land.Interact.performed += context => Interact();
        InputManager.playerControls.Land.Interact.canceled += context => InteractCanceled();

        // Fire
        InputManager.playerControls.Land.Fire.performed += context => FirePressed();
        InputManager.playerControls.Land.Fire.canceled += context => FireReleased();

        // Alternate Fire
        InputManager.playerControls.Land.AlternateFire.performed += context => AlternateFirePressed();
        InputManager.playerControls.Land.AlternateFire.canceled += context => AlternateFireReleased();
    }

    protected void OnDisable()
    {
        // Interact
        InputManager.playerControls.Land.Interact.performed -= context => Interact();
        InputManager.playerControls.Land.Interact.canceled -= context => InteractCanceled();

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
        UpdateUI();
    }

    protected override void Interact()
    {
        base.Interact();
        GameManager.TriggerEvent("PlayerInteracting");
    }

    private void FirePressed() {}

    private void FireReleased() {}

    private void AlternateFirePressed() {}

    private void AlternateFireReleased() {}

    private void UpdateUI()
    {
        if (GetInteractable(out var interactable) && message.Value != interactable.interactMessage)
        {
            message.Value = interactable.interactMessage;
            GameManager.TriggerEvent("PlayerLookingAtInteractable");
        }
        else if (!GetInteractable(out _) && message.Value != "")
        {
            message.Value = "";
            GameManager.TriggerEvent("PlayerNotLookingAtInteractable");
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
