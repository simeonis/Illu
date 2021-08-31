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

    // Private
    private Interactable interactable;

    // Network
    [SyncVar]
    public GameObject networkInteractable;
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
        if (hasAuthority) return;

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

        }
    }

    protected virtual void Interact()
    {
        networkSimpleData.SendData("INTERACTION");
        
        if (canInteract)
        {
            interactable.Interaction(this);
            if (hasAuthority) { SetNetworkInteractable(interactable.gameObject); }
        }
        else if (equipmentSlot.HasEquipment())
        {
            equipmentSlot.GetEquipment().Interaction(this);
        }
    }

    protected virtual void InteractCanceled()
    {
        networkSimpleData.SendData("INTERACTION_CANCELLED");
        
        if (canInteract)
        {
            interactable.InteractionCancelled(this);
            if (hasAuthority) { SetNetworkInteractable(null); }
        }
    }

    private bool CheckInteraction(out Interactable interactable)
    {
        if (Physics.Raycast(source.position, source.forward, out RaycastHit hit, interactionRange)) 
        {
            interactable = hit.collider.GetComponent<Interactable>();
            return interactable != null;
        }
        // TODO: Make sure InteractionCancelled is called if player is no longer looking at interactable
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
