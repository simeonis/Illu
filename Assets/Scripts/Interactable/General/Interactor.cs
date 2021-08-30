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
            if (canInteract)
            {
                interactable.Interaction(this);
            }
            else if (equipmentSlot.HasEquipment())
            {
                equipmentSlot.GetEquipment().Interaction(this);
            }
        }
    }

    protected virtual void Interact()
    {
        networkSimpleData.SendData("INTERACTION");
        if (canInteract)
        {
            interactable.Interaction(this);
        }
        else if (equipmentSlot.HasEquipment())
        {
            equipmentSlot.GetEquipment().Interaction(this);
        }
    }

    protected virtual void InteractCanceled()
    {
        if (canInteract)
        {
            interactable.InteractionCancelled(this);
        }
    }

    private bool CheckInteraction(out Interactable interactable)
    {
        if (Physics.Raycast(source.position, source.forward, out RaycastHit hit, interactionRange)) 
        {
            interactable = hit.collider.GetComponent<Interactable>();
            return interactable != null;
        }
        else
        {
            interactable = null;
            return false;
        }
    }
}
