using UnityEngine;
using Mirror;

public abstract class Interactor : NetworkBehaviour
{
    [Header("Interaction")]
    [SerializeField] public Transform source;
    [SerializeField] private float interactionRange;
    
    // Public
    [Header("Equipment")]
    [SerializeField] public Transform equipmentParent;
    public EquipmentSlot equipmentSlot;

    // Protected
    protected PlayerControls playerControls;
    protected bool canInteract;
    protected bool interaction = false;

    // Private
    protected Interactable interactable;

    // Network
    [SyncVar]
    [HideInInspector] public GameObject networkInteractable;
    protected NetworkSimpleData networkSimpleData;

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        networkSimpleData = GetComponent<NetworkSimpleData>();
    }

    protected virtual void OnEnable() 
    {
        // Network
        networkSimpleData.DataChanged += InteractEventHandler;

        // Input
        playerControls.Enable();
        playerControls.Land.Interact.performed += context => Interact();
        playerControls.Land.Interact.canceled += context => InteractCanceled();
    }

    protected virtual void Start()
    {
        equipmentSlot = new EquipmentSlot(transform);
    }

    protected virtual void OnDisable() 
    {
        // Network
        networkSimpleData.DataChanged -= InteractEventHandler;

        // Input
        playerControls.Disable();
        playerControls.Land.Interact.performed -= context => Interact();
        playerControls.Land.Interact.canceled -= context => InteractCanceled();
    }

    protected virtual void Update()
    {
        canInteract = CheckInteraction(out interactable);
    }

    private void InteractEventHandler(object sender, DataChangedEventArgs e)
    {
        switch(e.key)
        {
            case "INTERACTION_INTERACT":
                if (networkInteractable != null) 
                {
                    networkInteractable.GetComponent<Interactable>().Interaction(this);
                }
                break;
            case "INTERACTION_DROPPED":
                if (equipmentSlot.HasEquipment()) 
                {
                    equipmentSlot.GetEquipment().Interaction(this);
                }
                break;
            case "INTERACTION_CANCELLED":
                if (networkInteractable != null)
                {
                    networkInteractable.GetComponent<Interactable>().InteractionCancelled(this);
                }
                break;
            case "INTERACTION_ABORTED":
                if (networkInteractable != null)
                {
                    networkInteractable.GetComponent<Interactable>().InteractionCancelled(this);
                    interactable = null;

                    // Server [SyncVar]
                    if (hasAuthority) SetNetworkInteractable(null);
                }
                break;
            default:
                break;
        }
    }

    protected virtual void Interact()
    {   
        // Note: Only send over network if looking at interactable or holding equipment

        // Interact
        if (!interaction && canInteract)
        {
            interaction = true;
            networkSimpleData.SendData("INTERACTION_INTERACT");
        }
        // Drop
        else if (!canInteract && equipmentSlot.HasEquipment())
        {
            networkSimpleData.SendData("INTERACTION_DROPPED");
        }
    }

    protected virtual void InteractCanceled()
    {
        if (interaction)
        {
            interaction = false;
            networkSimpleData.SendData("INTERACTION_CANCELLED");
        }
    }

    private bool CheckInteraction(out Interactable interactable)
    {
        if (Physics.Raycast(source.position, source.forward, out RaycastHit hit, interactionRange)) 
        {
            interactable = hit.collider.GetComponent<Interactable>();

            // Possible interaction
            if (interactable != null)
            {
                // Server [SyncVar]
                if (hasAuthority) SetNetworkInteractable(hit.collider.gameObject);
            }
            // No possible interaction
            else
            {
                // Cancel any ongoing interaction (no longer looking at them)
                if (interaction && !equipmentSlot.HasEquipment())
                {
                    interaction = false;
                    networkSimpleData.SendData("INTERACTION_ABORTED");
                }
            }

            return interactable != null;
        }
        else
        {
            interactable = null;
            return false;
        }
    }

    [Command]
    private void SetNetworkInteractable(GameObject gameObject) 
    {
        networkInteractable = gameObject;
    }
}
