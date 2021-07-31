using UnityEngine;

public class PortalRender : MonoBehaviour
{
    // Target that the portal is rendering
    [HideInInspector]
    public Camera playerCamera;

    // Portal this renderer belongs to
    private Portal portal;
    private MeshRenderer screen;
    private readonly Quaternion flip = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    void Start()
    {
        portal = GetComponentInParent<Portal>();
        screen = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Check if screen is visible to player
        // if (!VisibleFromCamera(screen, playerCamera))
        // {
        //     return;
        // }

        Transform selfPortal = portal.camera.transform; // Current camera
        Transform targetPortal = portal.targetPortal.camera.transform.parent; // Target camera

        // Simulate player looking through portalB's position
        Vector3 relativePosition = targetPortal.InverseTransformPoint(playerCamera.transform.position);
        relativePosition = flip * relativePosition;
        selfPortal.position = selfPortal.parent.TransformPoint(relativePosition);

        // Simulate player looking through portalB's rotation
        Quaternion relativeRotation = Quaternion.Inverse(targetPortal.rotation) * playerCamera.transform.rotation;
        relativeRotation = flip * relativeRotation;
        selfPortal.rotation = selfPortal.parent.rotation * relativeRotation;

        // Calculate camera's new clipping plane
        // Reference: https://danielilett.com/2019-12-18-tut4-3-matrix-matching/
        Plane p = new Plane(-targetPortal.forward, targetPortal.position);
        Vector4 clipPlane = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        Vector4 clipPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(portal.targetPortal.camera.worldToCameraMatrix)) * clipPlane;

        // Assign clipping plane
        var newMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        portal.targetPortal.camera.projectionMatrix = newMatrix;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PortableObject _))
            portal.canTeleport = false;
    }

    private bool VisibleFromCamera(MeshRenderer renderer, Camera camera) {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }
}
