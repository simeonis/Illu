using UnityEngine;

public class PortableObject : MonoBehaviour
{
    protected readonly Quaternion flip = Quaternion.Euler(0.0f, 180.0f, 0.0f);
    private new Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Teleport(Transform source, Transform target)
    {
        // Update object's position
        Vector3 relativePos = source.InverseTransformPoint(transform.position);
        relativePos = flip * relativePos;
        transform.position = target.TransformPoint(relativePos);

        // Update object's rotation
        Quaternion relativeRot = Quaternion.Inverse(source.rotation) * transform.rotation;
        relativeRot = flip * relativeRot;
        transform.rotation = target.rotation * relativeRot;

        // Update object's velocity
        Vector3 relativeVel = source.InverseTransformDirection(rigidbody.velocity);
        relativeVel = flip * relativeVel;
        rigidbody.velocity = target.TransformDirection(relativeVel);
    }
}
