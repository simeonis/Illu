using UnityEngine;

public class CopyLimb : MonoBehaviour
{
    [SerializeField] Transform m_targetLimb;
    ConfigurableJoint m_characterJoint;
    Quaternion m_targetInitialRotation;

    void Start()
    {
        m_characterJoint = GetComponent<ConfigurableJoint>();
        m_targetInitialRotation = m_targetLimb.localRotation;
    }

    void FixedUpdate() => m_characterJoint.targetRotation = CopyRotation();

    Quaternion CopyRotation() => Quaternion.Inverse(m_targetLimb.localRotation) * m_targetInitialRotation;
}
