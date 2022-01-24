using UnityEngine;

public class NonEuclideanEquipment : Equipment
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float scaleFactor = 5f;
    
    private Transform source;
    private Vector3 initialScale;
    private Mesh mesh;

    private RaycastHit initialHit;
    private RaycastHit confirmationHit;
    private bool didHit;
    private bool didConfirmHit;

    protected override void Awake()
    {
        base.Awake();
        mesh = GetComponent<MeshFilter>().mesh;
        initialScale = transform.localScale;
    }

    public override void Interaction(Interactor interactor)
    {
        base.Interaction(interactor);
        source = isEquipped ? interactor.source : null;
    }

    void Update()
    {
        if (isEquipped)
        {
            didHit = Physics.BoxCast(source.position, MeshHalfExtents(), source.forward, out initialHit, transform.rotation, 100f, groundMask);
            if (didHit)
            {
                Vector3 scale = initialScale * (initialHit.distance / scaleFactor);
                // Growing
                if (scale.magnitude >= transform.localScale.magnitude)
                {
                    transform.position = source.position + (source.forward * initialHit.distance);
                    transform.localScale = scale;
                }
                // Shrinking
                else 
                {
                    Vector3 halfExtents = Vector3.Scale(mesh.bounds.extents, scale);
                    didConfirmHit = Physics.BoxCast(source.position, halfExtents, source.forward, out confirmationHit, transform.rotation, 100f, groundMask);
                    if (didConfirmHit)
                    {
                        transform.position = source.position + (source.forward * confirmationHit.distance);
                        transform.localScale = initialScale * (confirmationHit.distance / scaleFactor);
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (isEquipped)
        {
            // Collision
            if (didHit || didConfirmHit)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(didConfirmHit ? confirmationHit.point : initialHit.point, 0.005f * (didConfirmHit ? confirmationHit.distance : initialHit.distance));
                Gizmos.color = Color.red;
                Gizmos.DrawRay(source.position, source.forward * (didConfirmHit ? confirmationHit.distance : initialHit.distance));
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, MeshHalfExtents() * 2f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            // No Collision
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(source.position, source.forward * 100f);
            }
        }
    }

    private Vector3 MeshHalfExtents()
    {
        return Vector3.Scale(mesh.bounds.extents, transform.localScale);
    }
}