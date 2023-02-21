using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class JointConverter : MonoBehaviour
{
    // Character -> Configurable
    public void ReplaceJoint()
    {
        CharacterJoint oldJoint = gameObject.GetComponent<CharacterJoint>();
        ConfigurableJoint newJoint = gameObject.AddComponent<ConfigurableJoint>();

        newJoint.connectedBody = oldJoint.connectedBody;
        newJoint.anchor = oldJoint.anchor;
        newJoint.axis = oldJoint.axis;
        newJoint.autoConfigureConnectedAnchor = true;
        newJoint.secondaryAxis = oldJoint.swingAxis;

        newJoint.xMotion = ConfigurableJointMotion.Locked;
        newJoint.yMotion = ConfigurableJointMotion.Locked;
        newJoint.zMotion = ConfigurableJointMotion.Locked;
        
        newJoint.angularXMotion = ConfigurableJointMotion.Limited;
        newJoint.angularYMotion = ConfigurableJointMotion.Limited;
        newJoint.angularZMotion = ConfigurableJointMotion.Limited;

        newJoint.lowAngularXLimit = oldJoint.lowTwistLimit;
        newJoint.highAngularXLimit = oldJoint.highTwistLimit;
        newJoint.angularYLimit = oldJoint.swing1Limit;

        DestroyImmediate(oldJoint);
        DestroyImmediate(this);
    }
}

[CustomEditor(typeof(JointConverter))]
public class JointConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        JointConverter jointConverter = (JointConverter) target;

        if (GUILayout.Button("Replace"))
        {
            jointConverter.ReplaceJoint();
        }
    }
}
