using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    // Portal this renderer belongs to
    private Portal portal;

    void Start()
    {
        portal = GetComponentInParent<Portal>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PortableObject obj) && portal.canTeleport)
        {
            obj.Teleport(portal.collider, portal.targetPortal.collider);
            portal.targetPortal.canTeleport = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PortableObject _))
            portal.canTeleport = true;
    }
}
