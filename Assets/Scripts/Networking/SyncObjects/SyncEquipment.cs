using UnityEngine;
using Mirror;


///////////////////////////////////////////////////////////////////////////
/// in equimpent if released from player
/// start sending position and rotation to all client copies
/// other client -> receive pos and rot and follows 
/// after send once more and stop syncing  
///////////////////////////////////////////////////////////////////////////

//TODO
// add on collision with player pass auth 
// add on interacte change owner 

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

    //Hold local RB and Netowrk Identity
    protected Rigidbody equipmentBody;
    private NetworkIdentity networkIdentity;

    //local (Not Synced)
    private float lastClientSendTime;
    private Vector3 oldPosition;
    private float oldMagnitude;

    private bool simulating;

    protected void Awake()
    {
        equipmentBody = GetComponent<Rigidbody>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void FixedUpdate()
    {
        //If the object has been drop by a player with authority 
        if (simulating)
        {
            //simulating physics
            if (hasAuthority)
            {
                //check if the velocity is increasing 
                bool increasing = equipmentBody.velocity.magnitude > oldMagnitude;
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {

                    float speed = (Vector3.Distance(oldPosition, transform.position)) / Time.fixedDeltaTime;
                    CmdSendPositionRotation(transform.position, transform.rotation, speed);


                    //Update old data for checking against in the next loop
                    lastClientSendTime = Time.time;
                    oldPosition = transform.position;
                    oldMagnitude = equipmentBody.velocity.magnitude;
                }
                //simulation ended
                else if (!increasing && equipmentBody.velocity.magnitude <= 0.1)
                {
                    ///send one more position
                    CmdSendPositionRotation(transform.position, transform.rotation, 2);
                    CmdOnStop();
                }
            }
            else
            {
                HandleRemotePositionUpdates(RemoteObjSpeed);
                transform.rotation = RemoteObjRotation;
                transform.position = RemoteObjPosition;
            }
        }
    }

    //Trigger the action being sent
    //Entry point for Syncing 
    public void SendAction()
    {
        //Give Authority 
        //calledFromAuthority = hasAuthority;
        simulating = true;
        oldPosition = transform.position;
        CmdSendDropped();
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
        if (!hasAuthority)
        {
            // Disable rigidbody // DO this Once not in update
            equipmentBody.isKinematic = true;
            equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    //Command
    [Command(channel = Channels.Unreliable)]
    void CmdOnStop()
    {
        RpcOnStopped();
    }

    //Called when the moving has stopped on Authority 
    [ClientRpc]
    protected void RpcOnStopped()
    {
        simulating = false;

        if (!hasAuthority)
        {
            HandleRemotePositionUpdates(RemoteObjSpeed);
            HandleRemotePositionUpdates(0);
            // Enable rigidbody
            equipmentBody.isKinematic = false;
            equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            CmdRemoveAuth();
        }

    }

    [Command]
    public void CmdRemoveAuth()
    {
        networkIdentity.RemoveClientAuthority();
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


    //While moving on the client with authority that triggered the action
    //Handle the received positional updates
    [Client]
    public void HandleRemotePositionUpdates(float speed)
    {
        Debug.Log(speed);
        var LagDistance = RemoteObjPosition - transform.position;

        //High distance => sync is to much off => send to position
        if (LagDistance.magnitude > allowedLagDistance)
        {
            if (debug)
                Debug.LogWarning("Sync Position to Great");

            transform.position = RemoteObjPosition;
            LagDistance = Vector3.zero;
        }

        if (LagDistance.magnitude >= 0.025f)
        {
            //float step = speed * Time.fixedDeltaTime; // calculate distance to move
            equipmentBody.MovePosition(transform.position + LagDistance * Time.deltaTime * speed);
        }
    }
}
