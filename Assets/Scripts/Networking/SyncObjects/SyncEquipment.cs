using UnityEngine;
using Mirror;


///////////////////////////////////////////////////////////////////////////
/// in equimpent if released from player
/// start sending position and rotation to all client copies
/// other client -> receive pos and rot and follows 
/// after send once more and stop syncing  
///////////////////////////////////////////////////////////////////////////

///Alternative Idea: Sim on both and when it recieves updates let it tweak its pos 

public class SyncEquipment : NetworkBehaviour
{
    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 2.0f;

    [SerializeField] private bool debug;

    //Store the Remote Obj data on the client it was sent too
    protected Vector3 RemoteObjPosition;
    protected Quaternion RemoteObjRotation;
    protected float RemoteObjSpeed;

    //Hold local RB 
    protected Rigidbody equipmentBody;
    private NetworkIdentity ni;

    //local send time 
    private float lastClientSendTime;

    //local (Not Synced)
    private bool calledFromAuthority = false;
    private Vector3 oldPosition; // used to track the the speed

    private bool dropped;

    protected void Awake()
    {
        equipmentBody = GetComponent<Rigidbody>();
        ni = GetComponent<NetworkIdentity>();
    }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void Update()
    {
        Debug.Log("Called From Authority " + calledFromAuthority);
        //If the object has been drop by a player with authority 
        if (dropped)
        {
            if (calledFromAuthority)
            {
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {

                    float speed = (Vector3.Distance(oldPosition, transform.position)) / Time.deltaTime;
                    CmdSendPositionRotation(transform.position, transform.rotation, speed);
                    lastClientSendTime = Time.time;

                    //Update the old position to pos now
                    oldPosition = transform.position;
                }
                if (equipmentBody.velocity.magnitude == 0)
                {
                    OnStopped();
                }
            }
            else
            {
                HandleRemotePositionUpdates(RemoteObjSpeed);
                transform.rotation = RemoteObjRotation;
            }
        }
    }

    //Trigger the action being sent
    //Entry point for Syncing 
    public void SendAction(bool hasAuthority)
    {
        Debug.Log("Send Action hasAuthority " + hasAuthority);
        //Give Authority 
        calledFromAuthority = hasAuthority;
        dropped = true;
        CmdSendDropped();
    }

    //Called when the moving has stopped on Authority 
    protected void OnStopped()
    {

        if (calledFromAuthority)
        {

            CmdSendPositionRotation(transform.position, transform.rotation, 0);
        }
        else
        {
            // Enable rigidbody
            equipmentBody.isKinematic = false;
            equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    // Authority sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPositionRotation(Vector3 Position, Quaternion Rotation, float Speed)
    {
        RPCSyncPositionRotation(Position, Rotation, Speed);
    }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPositionRotation(Vector3 Position, Quaternion Rotation, float Speed)
    {
        if (debug)
            Debug.Log("RPC Equip Position: " + Position + "Rotation " + Rotation + "Speed " + Speed);
        RemoteObjPosition = Position;
        RemoteObjRotation = Rotation;
        RemoteObjSpeed = Speed;
    }

    // Send that the obj has been dropped 
    [Command(channel = Channels.Unreliable)]
    public void CmdSendDropped()
    {
        RpcDropped();
    }

    //If not the client with Authority TEMP disable RB
    [ClientRpc]
    public void RpcDropped()
    {
        //Checking if not the client with authority 
        if (!calledFromAuthority)
        {
            // Disable rigidbody // DO this Once not in update
            equipmentBody.isKinematic = true;
            equipmentBody.interpolation = RigidbodyInterpolation.None;
        }
        //Reset Authority to false // stops sending on OBJ // Can be re interacted with 
        calledFromAuthority = false;
        dropped = false;

        Debug.Log("RPC Dropped: calledFromAuthority" + calledFromAuthority + " dropped " + dropped);
    }

    //While moving on the client with authority that triggered the action
    //Handle the received positional updates 
    [Client]
    public void HandleRemotePositionUpdates(float speed)
    {
        var LagDistance = RemoteObjPosition - transform.position;

        //High distance => sync is to much off => send to position
        if (LagDistance.magnitude > allowedLagDistance)
        {
            if (debug)
                Debug.LogWarning("Sync Position to Great");
            transform.position = RemoteObjPosition;

            LagDistance = Vector3.zero;
        }

        if (LagDistance.magnitude < 0.025f)
        {
            //object is nearly at the same point 
            transform.position = Vector3.zero;
        }
        else
        {
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, RemoteObjPosition, step);
        }

    }
}
