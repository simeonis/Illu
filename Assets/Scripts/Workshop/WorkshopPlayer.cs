using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using TMPro;

public class WorkshopPlayer : MonoBehaviour
{
    [Header("Grappling Hook")]
    [SerializeField, Range(0.1f, 100f)] float ropeLength = 5f;
    [SerializeField] Transform m_exitPoint;
    [SerializeField] Transform m_grapplePoint;

    [Header("Rig")]
    [SerializeField] Rig m_rightArmRig;
    [SerializeField] Transform m_rightArmTarget;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI m_ragdollInfo;

    Rigidbody m_rigidbody;
    CapsuleCollider m_collider;
    List<Collider> m_ragdollColliders;
    Animator m_animator;
    LineRenderer m_lineRenderer;
    bool isRagdoll = false;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        m_ragdollColliders = new List<Collider>();
        m_animator = GetComponentInChildren<Animator>();
        m_lineRenderer = GetComponentInChildren<LineRenderer>();

        //InitializeRagdoll();
        //DisableRagdoll();
    }

    void Start()
    {
        m_rightArmRig.weight = 1f;
        m_lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Right Arm
        // Vector3 direction = (m_grapplePoint.position - m_exitPoint.position).normalized;
        // m_rightArmTarget.position = m_exitPoint.position + (direction * 0.25f);
        // m_rightArmTarget.right = direction;

        // Body Rotation
        Vector3 bodyToGrapple = m_grapplePoint.position - transform.position;
        Vector3 velocityDirection = m_rigidbody.velocity - Vector3.Project(m_rigidbody.velocity, bodyToGrapple);
        Quaternion targetRotation = (velocityDirection == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(velocityDirection, bodyToGrapple);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 150f * Time.deltaTime);
    }

    void LateUpdate()
    {
        // Rope
        m_lineRenderer.SetPosition(0, m_exitPoint.position);
        m_lineRenderer.SetPosition(1, m_grapplePoint.position);
    }

    void FixedUpdate()
    {
        // Predict where player (grapple exit point) will be next physics frame
        Vector3 testPosition = m_exitPoint.position + (m_rigidbody.velocity * Time.fixedDeltaTime);

        // Calculate if test position is outside of acceptable rope range
        Vector3 anchorToPlayer = (testPosition - m_grapplePoint.position);
        if (anchorToPlayer.magnitude > ropeLength)
        {
            testPosition = m_grapplePoint.position + (anchorToPlayer.normalized * ropeLength);
        }

        // Constrain player
        m_rigidbody.velocity = (testPosition - m_exitPoint.position) / Time.fixedDeltaTime;
    }

    void InitializeRagdoll()
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            if (c != m_collider)
            {
                c.isTrigger = true;
                m_ragdollColliders.Add(c);
            }
        }
    }

    // Called via button
    public void ToggleRagdoll()
    {
        isRagdoll = !isRagdoll;
        if (isRagdoll) EnableRagdoll();
        else DisableRagdoll();
    }

    void EnableRagdoll()
    {
        m_ragdollInfo.text = "Ragdoll: On";
        m_collider.isTrigger = true;
        m_animator.enabled = false;

        foreach (Collider c in m_ragdollColliders)
        {
            c.isTrigger = false;
            c.attachedRigidbody.isKinematic = false;
            c.attachedRigidbody.velocity = Vector3.zero;
        }
    }

    void DisableRagdoll()
    {
        m_ragdollInfo.text = "Ragdoll: Off";
        m_collider.isTrigger = false;
        m_animator.enabled = true;

        foreach (Collider c in m_ragdollColliders)
        {
            c.isTrigger = true;
            c.attachedRigidbody.velocity = Vector3.zero;
            c.attachedRigidbody.isKinematic = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_grapplePoint.position, ropeLength);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_exitPoint.position, 0.05f);
    }
}
