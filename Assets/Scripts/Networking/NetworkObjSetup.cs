using UnityEngine;
using Mirror;

public class NetworkObjSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    private string _netID;
    private string Name;

    void Start()
    {
        Debug.Log("Name: " + name + " " + "isServer: " + isServer + " " + "isClient: " + isClient + " " + "isLocalPlayer: " + isLocalPlayer);
        if (!hasAuthority)
        {
            DisableComponents();
        }

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Name = GetComponent<NetworkIdentity>().name;
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

}
