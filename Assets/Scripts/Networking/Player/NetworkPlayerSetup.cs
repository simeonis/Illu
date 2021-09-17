using UnityEngine;
using Mirror;

public class NetworkPlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerModel;

    private string _netID;
    private new string name;

    void Start()
    {
        // Player
        if (hasAuthority)
        {
            HideModel();
        }
        // Dummy-Player
        else if (!hasAuthority)
        {
            DisableCameraAndAudio();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _netID = GetComponent<NetworkIdentity>().netId.ToString();
        name = GetComponent<NetworkIdentity>().name;
    }

    private void HideModel()
    {
        ChangeChildrenLayerMask(playerModel, "Invisible", true);
    }

    private void DisableCameraAndAudio()
    {
        playerCamera.enabled = false;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }

    // Modifies layer of each child inside of parent.
    // If inclusive is true, parent's layer is also modified.
    // Useful for Camera Culling Mask. 
    private void ChangeChildrenLayerMask(Transform parent, string layer, bool inclusive = false)
    {
        if (inclusive) ChangeLayerMask(parent, layer);
        foreach (Transform child in parent.transform)
        {
            if (child == null) continue;
            ChangeChildrenLayerMask(child, layer, true);
        }
    }

    // Modifies layer of specific element without modifying it's children.
    private void ChangeLayerMask(Transform transform, string layer)
    {
        transform.gameObject.layer = LayerMask.NameToLayer(layer);
    }
}
