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
    private Interactable interactable;

    // Network
    [SyncVar]
    [HideInInspector] public GameObject networkInteractable;
    protected NetworkSimpleData networkSimpleData;

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Land.Interact.performed += context => Interact();
        playerControls.Land.Interact.canceled += context => InteractCanceled();
    }

    protected virtual void Start()
    {
        equipmentSlot = new EquipmentSlot(transform);
        networkSimpleData = GetComponent<NetworkSimpleData>();
        networkSimpleData.DataChanged += InteractEventHandler;
    }

    void OnEnable() 
    {
        playerControls.Enable();
    }

    void OnDisable() 
    {
        playerControls.Disable();
    }

    protected virtual void Update()
    {
        if (canInteract = CheckInteraction(out interactable))
        {
            interactable.Seen();
        }
    }

    private void InteractEventHandler(object sender, DataChangedEventArgs e)
    {
        if (e.key == "INTERACTION")
        {
            // Interact
            if (networkInteractable != null)
            {
                networkInteractable.GetComponent<Interactable>().Interaction(this);
            }
            // Drop
            else if (equipmentSlot.HasEquipment())
            {
                equipmentSlot.GetEquipment().Interaction(this);
            }
        }
        else if (e.key == "INTERACTION_CANCELLED")
        {
            if (networkInteractable != null)
            {
                networkInteractable.GetComponent<Interactable>().InteractionCancelled(this);
            }
        }
    }

    protected virtual void Interact()
    {   
        if (!interaction)
        {
            interaction = true;
            networkSimpleData.SendData("INTERACTION");
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
                if (interaction)
                {
                    interaction = false;
                    networkSimpleData.SendData("INTERACTION_CANCELLED");
                }

                // -------------------------- WARNING --------------------------
                // Below code might be necessary to ensure that players 
                // can only interact with object they themselves are looking at. 
                //
                // Currently disabled because it clears the networkInteractable 
                // before the above code can finish.
                // -------------------------------------------------------------

                // Server [SyncVar]
                // if (hasAuthority) SetNetworkInteractable(null);
            }

            return interactable != null;
        }
        else
        {
            interactable = null;
            // Server [SyncVar]
            if (hasAuthority) SetNetworkInteractable(null);
            return false;
        }
    }

    [Command]
    private void SetNetworkInteractable(GameObject gameObject) 
    {
        networkInteractable = gameObject;
    }
}
