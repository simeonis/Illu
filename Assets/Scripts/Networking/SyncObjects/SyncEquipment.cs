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

    //Hold local RB and Netowrk Identity
    //private Rigidbody equipmentBody;
    //private NetworkIdentity networkIdentity;

    private Equipment _equipment;
    private bool simulation;

    //local (Not Synced)
    private float lastClientSendTime;
    //private Vector3 oldPosition;
    //private float oldMagnitude;

    //private bool simulating;

    // protected void Awake()
    // {
    //     _equipment = GetComponentInChildren<Equipment>();
    //     // equipmentBody = GetComponent<Rigidbody>();
    //     // networkIdentity = GetComponent<NetworkIdentity>();
    // }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void FixedUpdate()
    {
        // //If the object has been drop by a player with authority 
        // if (simulating)
        // {
        //     //simulating physics
        //     if (hasAuthority)
        //     {
        //check if the velocity is increasing 
        //bool increasing = equipmentBody.velocity.magnitude > oldMagnitude;
        // check only each 'syncInterval'
        if (simulation)
        {
            if (hasAuthority)
            {
                if (Time.time - lastClientSendTime >= syncInterval)
                {

                    // float speed = (Vector3.Distance(oldPosition, transform.position)) / Time.fixedDeltaTime;

                    CmdSendPosition(transform.position);

                    //Update old data for checking against in the next loop
                    lastClientSendTime = Time.time;
                    // oldPosition = transform.position;
                    // oldMagnitude = equipmentBody.velocity.magnitude;
                }


                // //simulation ended
                // else if (!increasing && equipmentBody.velocity.magnitude <= 0.1)
                // {
                //     ///send one more position
                //     CmdSendPositionRotation(transform.position, transform.rotation, 2);
                //     CmdOnStop();
                // }
                //     }
                //     else
                //     {
                //         HandleRemotePositionUpdates(RemoteObjSpeed);
                //         transform.rotation = RemoteObjRotation;
                //         transform.position = RemoteObjPosition;
                //     }
                // }
            }
            else
            {
                CalculateCorrectionalForce();
                ADJPosition();
            }
        }

    }

    public void RegisterEquipment(Equipment equipment)
    {
        _equipment = equipment;
        simulation = true;
    }

    //Trigger the action being sent
    //Entry point for Syncing 
    public void SendAction(Vector3 direction, float force, Vector3 currVel)
    {
        //Give Authority 
        //calledFromAuthority = hasAuthority;
        // simulating = true;
        // oldPosition = transform.position;

        CmdSendAction(direction, force, currVel);
    }

    // Send that the obj has been dropped 
    [Command(channel = Channels.Unreliable)]
    public void CmdSendAction(Vector3 direction, float force, Vector3 currVel)
    {
        RpcAction(direction, force, currVel);
    }

    //If not the client with Authority TEMP disable RB
    [ClientRpc]
    public void RpcAction(Vector3 direction, float force, Vector3 currVel)
    {
        //Checking if not the client with authority 
        if (!hasAuthority)
        {
            _equipment.AddForce(direction, force, currVel);
        }
    }

    // //Command
    // [Command(channel = Channels.Unreliable)]
    // void CmdOnStop()
    // {
    //     RpcOnStopped();
    // }

    // //Called when the moving has stopped on Authority 
    // [ClientRpc]
    // protected void RpcOnStopped()
    // {
    //     simulating = false;

    //     if (!hasAuthority)
    //     {
    //         HandleRemotePositionUpdates(RemoteObjSpeed);
    //         HandleRemotePositionUpdates(0);
    //         // Enable rigidbody
    //         equipmentBody.isKinematic = false;
    //         equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
    //     }
    //     else
    //     {
    //         CmdRemoveAuth();
    //     }

    // }



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


    // //While moving on the client with authority that triggered the action
    // //Handle the received positional updates
    // [Client]
    // public void HandleRemotePositionUpdates(float speed)
    // {
    //     Debug.Log(speed);
    //     var LagDistance = RemoteObjPosition - transform.position;

    //     //High distance => sync is to much off => send to position
    //     if (LagDistance.magnitude > allowedLagDistance)
    //     {
    //         if (debug)
    //             Debug.LogWarning("Sync Position to Great");

    //         transform.position = RemoteObjPosition;
    //         LagDistance = Vector3.zero;
    //     }

    //     if (LagDistance.magnitude >= 0.025f)
    //     {
    //         //float step = speed * Time.fixedDeltaTime; // calculate distance to move
    //         equipmentBody.MovePosition(transform.position + LagDistance * Time.deltaTime * speed);
    //     }
    // }




    //check remote position against where it is 
    //determine a vector to correct on
    //apply force on this vector 
    public void CalculateCorrectionalForce()
    {
        var LagDistance = RemoteObjPosition - transform.position;

        //try speed later
        _equipment.AddCorrectionalForce(LagDistance);
    }

    public void ADJPosition()
    {
        Vector3 currentPos = _equipment.defaultParent.position;
        float speed = (Vector3.Distance(currentPos, RemoteObjPosition)) / Time.fixedDeltaTime;
        _equipment.equipmentBody.MovePosition(currentPos + RemoteObjPosition * speed);
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
        // draw start and goal points
        DrawDataPointGizmo(RemoteObjPosition, Color.yellow);
        // draw line between them
        DrawLineBetweenDataPoints(transP_equipment.defaultParent.positionosition, RemoteObjPosition, Color.cyan);
    }

}

