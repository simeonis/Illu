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

// start filling a buffer of positions
//try sending a time / by distance to get speed = inerpolate with that 

public class SyncEquipment : NetworkBehaviour
{
    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 2.0f;

    [SerializeField] private bool debug;

    //Store the Remote Obj data on the client it was sent too
    private Vector3 RemoteObjPosition;
    //protected Quaternion RemoteObjRotation;
    //protected float RemoteObjSpeed;

    private Equipment _equipment;
    private Rigidbody equipmentBody;

    //local (Not Synced)
    private float lastClientSendTime;

    private bool simulation = false;
    private float oldMagnitude = 0f;

    void Start()
    {
        _equipment = GetComponent<Equipment>();
        equipmentBody = _equipment.equipmentBody;
    }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void FixedUpdate()
    {
        if (simulation && hasAuthority)
        {
            bool increasing = equipmentBody.velocity.magnitude > oldMagnitude;
            if (Time.time - lastClientSendTime >= syncInterval)
            {
                CmdSendPosition(transform.position);

                // Update old data for checking against in the next loop
                lastClientSendTime = Time.time;
                oldMagnitude = equipmentBody.velocity.magnitude;
            }
            // Simulation ended
            else if (!increasing && equipmentBody.velocity.magnitude <= 0.1)
            {
                // Send one more position
                // CmdSendPosition(transform.position);
                CmdOnStop();
            }
        }
    }

    void Update()
    {
        if (simulation && !hasAuthority)
        {
            HandleRemotePositionUpdates();
        }
    }

    //Trigger the action being sent
    //Entry point for Syncing 
    public void Trigger()
    {
        CmdTrigger();
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdTrigger()
    {
        RPCTrigger();
    }

    [ClientRpc]
    private void RPCTrigger()
    {
        if (!hasAuthority) 
        {
            equipmentBody.isKinematic = true;
            equipmentBody.interpolation = RigidbodyInterpolation.None;
        }

        simulation = true;
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
        simulation = false;
        if (!hasAuthority)
        {
            equipmentBody.isKinematic = false;
            equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else RemoveAuthority();
    }

    [Command]
    private void RemoveAuthority()
    {
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }

    // Authority sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPosition(Vector3 position)
    {
        RPCSyncPosition(position);
    }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPosition(Vector3 position)
    {
        RemoteObjPosition = position;
        // RemoteObjRotation = Rotation;
        // RemoteObjSpeed = Speed;
    }


    //While moving on the client with authority that triggered the action
    //Handle the received positional updates
    [Client]
    public void HandleRemotePositionUpdates()
    {
        var LagDistance = RemoteObjPosition - transform.position;

        // High distance => sync is to much off => send to position
        if (LagDistance.magnitude > allowedLagDistance)
        {
            if (debug) Debug.LogWarning("Sync Position too Great! Teleporting equipment.");
            transform.position = RemoteObjPosition;
            LagDistance = Vector3.zero;
        }

        if (LagDistance.magnitude >= 0.025f)
        {
            transform.position = Vector3.Lerp(transform.position, RemoteObjPosition, 0.5f);
        }
    }

    //Gizmo stuff for testing 
    //---------------------------------------------------------------------------------------------

    static void DrawDataPointGizmo(Vector3 pos, Color color)
    {
        // use a little offset because transform.localPosition might be in
        // the ground in many cases
        Vector3 offset = Vector3.up * 0.01f;

        // draw position
        Gizmos.color = color;
        Gizmos.DrawSphere(pos + offset, 0.1f);

    }

    static void DrawLineBetweenDataPoints(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
    }

    // draw the data points for easier debugging
    void OnDrawGizmos()
    {
        if (simulation)
        {
            // draw start and goal points
            DrawDataPointGizmo(RemoteObjPosition, Color.yellow);
            // draw line between them
            DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
        }
    }

}

