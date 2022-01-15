using UnityEngine;

public class PerspectiveEquipment : Equipment
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float scaleFactor = 5f;
    private Transform cameraTransform;
    private Vector3 initialScale;
    private Mesh mesh;

    private RaycastHit growHit;
    private RaycastHit shrinkHit;
    private bool didGrowHit;
    private bool didShrinkHit;

    void Start()
    {
        initialScale = transform.localScale;
        mesh = GetComponent<MeshFilter>().mesh;
    }

    public override void Interaction(Interactor interactor)
    {
        base.Interaction(interactor);
        if (isEquipped)
        {
            cameraTransform = interactor.source;
            Debug.Log("Perspective Equipment equipped");
        } 
        else
        {
            cameraTransform = null;
            Debug.Log("Perspective Equipment dropped");
        }
    }

    void Update()
    {
        if (cameraTransform)
        {
            didGrowHit = Physics.BoxCast(cameraTransform.position, MeshExtents(), cameraTransform.forward, out growHit, transform.rotation, 100f, groundMask);
            if (didGrowHit)
            {
                Vector3 scale = initialScale * (growHit.distance / scaleFactor);
                // Growing
                if (scale.magnitude >= transform.localScale.magnitude)
                {
                    transform.position = cameraTransform.position + (cameraTransform.forward * growHit.distance);
                    transform.localScale = scale;
                }
                // Shrinking
                else 
                {
                    Vector3 halfExtents = Vector3.Scale(mesh.bounds.extents, scale);
                    didShrinkHit = Physics.BoxCast(cameraTransform.position, halfExtents, cameraTransform.forward, out shrinkHit, transform.rotation, 100f, groundMask);
                    if (didShrinkHit)
                    {
                        transform.position = cameraTransform.position + (cameraTransform.forward * shrinkHit.distance);
                        transform.localScale = initialScale * (shrinkHit.distance / scaleFactor);
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (cameraTransform)
        {
            // Collision
            if (didGrowHit || didShrinkHit)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(didShrinkHit ? shrinkHit.point : growHit.point, 0.005f * (didShrinkHit ? shrinkHit.distance : growHit.distance));
                Gizmos.color = Color.red;
                Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * (didShrinkHit ? shrinkHit.distance : growHit.distance));
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, MeshExtents() * 2f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            // No Collision
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * 100f);
            }
        }
    }

    private Vector3 MeshExtents()
    {
        return Vector3.Scale(mesh.bounds.extents, transform.localScale);
    }

    public override void EquipmentPrimaryPressed() { Debug.Log("Perspective Equipment Primary Pressed"); }
    public override void EquipmentSecondaryPressed() { Debug.Log("Perspective Equipment Secondary Pressed"); }
    public override void EquipmentPrimaryReleased() { Debug.Log("Perspective Equipment Primary Released"); }
    public override void EquipmentSecondaryReleased() { Debug.Log("Perspective Equipment Secondary Released"); }
}
