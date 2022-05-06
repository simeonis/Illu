using UnityEngine;

public abstract class Interactor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] protected float range;
    [SerializeField] protected Vector3 offset;
    [SerializeField] protected LayerMask layers;

    protected Interactable cachedInteractable;
    protected Collider colliderInteractable; // The closest, valid interactable's collider
    protected Collider[] potentialCollisions = new Collider[10];
    private InteractorState state = InteractorState.Searching;
    private enum InteractorState
    {
        Searching,
        Interacting,
    }

    protected virtual void Update()
    {
        switch(state)
        {
            case InteractorState.Searching:
                SearchInteractable();
                break;
            case InteractorState.Interacting:
                InteractionRange();
                break;
        }
    }
    
    protected virtual void Interact()
    {
        if (GetInteractable(out var interactable))
        {
            state = InteractorState.Interacting;
            interactable.Interact(this);
        }
    }

    protected virtual void InteractCanceled()
    {
        if (GetInteractable(out var interactable) && IsInteracting())
        {
            state = InteractorState.Searching;
            interactable.InteractCancel(this);
            cachedInteractable = null;
        }
    }

    // Checks if interactor is currently interacting with an interactable
    protected bool IsInteracting()
    {
        return state == InteractorState.Interacting;
    }
    
    // Checks if interactable has been found and returns it
    // Used to avoid calling GetComponent in every update loop
    protected bool GetInteractable(out Interactable interactable)
    {
        if (!colliderInteractable)
        {
            interactable = null;
            return false;
        }
        else if (!cachedInteractable)
        {
            cachedInteractable = colliderInteractable.GetComponent<Interactable>();
        }

        interactable = cachedInteractable;
        return true;
    }

    // Searches for interactables (max. 10) nearby interactor and choses the closest one
    protected virtual void SearchInteractable()
    {
        // Find all potential interactables in range of the interactor
        int collisionsFound = Physics.OverlapSphereNonAlloc(transform.position + transform.TransformDirection(offset), range, potentialCollisions, layers);

        // No interactables in range
        if (collisionsFound <= 0)
        {
            colliderInteractable = null;
        }

        // Default: No validation
        Collider[] validCollisions;
        int validAmount = ValidateCollider(potentialCollisions, collisionsFound, out validCollisions);

        // No interactables in range and in-front of interactor
        if (validAmount <= 0)
        {
            colliderInteractable = null;
        }
        // One interactable in range and in-front of interactor
        else if (validAmount == 1)
        {
            colliderInteractable = validCollisions[0];
            cachedInteractable = null; // Invalidate cache
        }
        // Multiple interactables in range and in-front of interactor
        else
        {
            Collider closestCollider = null;
            float minDistance = float.MaxValue;
            
            // Search for closest interactable to interactor
            for (int i=0; i < validAmount; i++)
            {
                Collider collider = validCollisions[i];
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = collider;
                }
            }

            colliderInteractable = closestCollider;
            cachedInteractable = null; // Invalidate cache
        }
    }

    // Currently does nothing, useful for overriding
    // Example: Player only validates colliders that are within 90 degrees of camera's forward vector
    protected virtual int ValidateCollider(Collider[] colliders, int collidersFound, out Collider[] validatedColliders)
    {
        validatedColliders = colliders;
        return collidersFound;
    }

    // Cancels interaction if interactor gets too far from interactable
    protected virtual void InteractionRange()
    {
        if (GetInteractable(out var interactable))
        {
            if (Vector3.Distance(transform.position, interactable.transform.position) > range)
            {
                InteractCanceled();
            }
        }
    }

    #if UNITY_EDITOR
    [Header("Debug"), InspectorName("enable")] 
    public bool enable = false;
    protected virtual void OnDrawGizmos()
    {
        if (enable)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(offset), range);
        }
    }
    #endif
}
