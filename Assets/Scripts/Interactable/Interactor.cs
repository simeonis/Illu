using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactor : MonoBehaviour
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

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Land.Interact.performed += context => Interact();
        playerControls.Land.Interact.canceled += context => InteractCanceled();
    }

    protected virtual void Start()
    {
        equipmentSlot = new EquipmentSlot(transform);
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

    protected virtual void Interact()
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
