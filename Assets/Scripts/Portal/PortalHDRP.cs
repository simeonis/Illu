using UnityEngine;
using UnityEngine.Rendering;
using Illu.Utility;

namespace Illu.Portal 
{
    public class PortalHDRP : MonoBehaviour
    {
        [SerializeField] private PortalHDRP linkedPortal;

        private Camera playerCamera;
        private Camera portalCamera;
        private MeshRenderer screen;

        private RenderTexture viewTexture;

        // Called on PlayerSpawned event
        public void Setup()
        {
            playerCamera = Camera.main;
            portalCamera = GetComponentInChildren<Camera>();
            portalCamera.enabled = false;
            screen = GetComponentInChildren<MeshRenderer>();
        }

        private readonly Quaternion flip = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        void Update()
        {
            Transform selfPortal = portalCamera.transform; // Current camera
            Transform targetPortal = linkedPortal.portalCamera.transform.parent; // Target camera

        //     Transform selfPortal = portal.camera.transform; // Current camera
        // Transform targetPortal = portal.targetPortal.camera.transform.parent; // Target camera

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
            Plane p = new Plane(targetPortal.forward, targetPortal.position);
            Vector4 clipPlane = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
            Vector4 clipPlaneCameraSpace =
                Matrix4x4.Transpose(Matrix4x4.Inverse(linkedPortal.portalCamera.worldToCameraMatrix)) * clipPlane;

            // Assign clipping plane
            var newMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            linkedPortal.portalCamera.projectionMatrix = newMatrix;

        //     Plane p = new Plane(-targetPortal.forward, targetPortal.position);
        // Vector4 clipPlane = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        // Vector4 clipPlaneCameraSpace =
        //     Matrix4x4.Transpose(Matrix4x4.Inverse(portal.targetPortal.camera.worldToCameraMatrix)) * clipPlane;

        // // Assign clipping plane
        // var newMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        // portal.targetPortal.camera.projectionMatrix = newMatrix;
        }
    }
}
