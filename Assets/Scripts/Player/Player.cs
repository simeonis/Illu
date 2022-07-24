using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Interactor
{
    [Header("Player Interaction")]
    [SerializeField] Transform _playerCamera;
    [SerializeField] float _raycastRange;

    [Header("Equipment Slots")]
    [SerializeField] Transform _leftHand;
    Item _equipment = null;

    [Header("Scriptable Object")]
    [SerializeField] StringVariable _interactMessage;
    [SerializeField] TriggerVariable _rotateCrosshair;

    protected void OnEnable()
    {
        // Interact
        InputManager.Instance.playerControls.Player.Interact.started += Interact;
        InputManager.Instance.playerControls.Player.Interact.canceled += InteractCanceled;
    }

    protected void OnDisable()
    {
        // Interact
        InputManager.Instance.playerControls.Player.Interact.started -= Interact;
        InputManager.Instance.playerControls.Player.Interact.canceled -= InteractCanceled;
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
        _rotateCrosshair.Trigger();

        // Interactable in range
        if (GetInteractable(out var interactable))
        {
            if (IsEquipped() && (interactable is Item)) _equipment.Interact(this);
            else
            {
                state = InteractorState.Interacting;
                interactable.Interact(this);
            }
        }
        // No interactable in range & holding equipment
        else if (IsEquipped())
        {
            _equipment.Interact(this);
        }
    }

    void InteractCanceled(InputAction.CallbackContext context) => base.InteractCanceled();

    public void Equip(Item equipment)
    {
        equipment.transform.SetParent(_leftHand, false);
        equipment.transform.localPosition = Vector3.zero;
        equipment.transform.localRotation = Quaternion.identity;
        this._equipment = equipment;
    }

    public void Drop(Item equipment)
    {
        if (this._equipment == equipment)
        {
            this._equipment.transform.parent = null;
            this._equipment = null;
        }
    }

    public bool IsEquipped() => _equipment != null;

    private void UpdateUI()
    {
        // //Interact Message(Crosshair)
        // if (GetInteractable(out var interactable) && !_interactMessage.Equals(interactable.interactMessage))
        // {
        //     _interactMessage.Value = interactable.interactMessage;
        //     // GameManager.TriggerEvent("PlayerLookingAtInteractable");
        // }
        // else if (!GetInteractable(out _) && !_interactMessage.IsEmpty())
        // {
        //     _interactMessage.Value = "";
        //     // GameManager.TriggerEvent("PlayerNotLookingAtInteractable");
        // }
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
            if (Vector3.Angle(playerToCollision, _playerCamera.forward) <= 90f)
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
        raycastDistance = Vector3.Distance(_playerCamera.position, transform.position) + _raycastRange;
        if (Physics.Raycast(_playerCamera.position, _playerCamera.forward, out directHit, raycastDistance, layers))
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
    [Header("Debug")]
    public new bool enable = false;
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (enable)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_playerCamera.position, _playerCamera.forward * raycastDistance);
        }
    }
#endif
}
