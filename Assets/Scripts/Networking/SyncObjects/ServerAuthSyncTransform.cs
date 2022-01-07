using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;
using System.Collections;

///////////////////////////////////////////////////////////////////////////
/// in equimpent if released from player
/// start sending position and rotation to all client copies
/// other client -> receive pos and rot and follows 
/// after send once more and stop syncing  
///////////////////////////////////////////////////////////////////////////

//Problem: if I wait for permission The throw already happed
//either send threw obj with authority or simulate on server only 

//idea object with authority has list of interacables listens for changes and sends them 

//TODO
// add on collision with player pass auth 
// add on interacte change owner 

// start filling a buffer of positions
//try sending a time / by distance to get speed = inerpolate with that 

public class ServerAuthSyncTransform : NetworkBehaviour
{
    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 2.0f;
    public float allowedRotationAngle = 5f;

    [SerializeField] private bool debug;

    //Store the Remote Obj data on the client it was sent too
    //private Vector3 RemoteObjPosition;
    private Quaternion RemoteObjRotation;

    private Equipment _equipment;
    private Rigidbody equipmentBody;

    //local (Not Synced)
    private float lastClientSendTime = 0.0f;

    /*
    *   Means the cube is still following positional updates
    *   Stays true until procceded all data received 
    *   Even after the client is finished 
    */
    private bool simulation = false;

    /*
    *   Index into the list of positions
    *   Only moves forward when the position is reached    
    */
    private int simStep = 0;

    /*
    *   Set to true when the client is done setting
    *   Note: may still be simulating when true
    */
    private bool serverIsSending = false;


    //List of Positions and time sent from the initiating cube 
    //List of reached positions on the reciving client for debugging 
    private List<MyNetworkData> receivedPositions;
    private List<Vector3> vistedPositions;


    private float oldMagnitude = 0f;
    private long triggerTimeStamp;

    private const int processBufferCount = 3;


    void Start()
    {
        //get ref to equipment 
        _equipment = GetComponent<Equipment>();
        equipmentBody = _equipment.equipmentBody;

        //Initialize lists 
        receivedPositions = new List<MyNetworkData>();
        vistedPositions = new List<Vector3>();

        Debug.Log("Sync Is Server " + isServer);

        // //Not server turn off simulation 
        // if (!isServer)
        // {
        //     equipmentBody.isKinematic = true;
        //     equipmentBody.interpolation = RigidbodyInterpolation.None;
        // }

    }

    void FixedUpdate()
    {
        //[Server] has authority send commands to other clients
        if (isServer && hasAuthority)
        {
            bool increasing = equipmentBody.velocity.magnitude > oldMagnitude;

            if (increasing && Time.time - lastClientSendTime >= syncInterval)
            {
                MyNetworkData currentPos = new MyNetworkData(transform.position, DateTime.Now.Ticks);
                CmdSendPositionRotation(currentPos, transform.rotation);

                // Update old data for checking against in the next loop
                lastClientSendTime = Time.time;
                oldMagnitude = equipmentBody.velocity.magnitude;
            }
            else if (!increasing && equipmentBody.velocity.magnitude <= 0.1)
            {
                // Send one more position
                // CmdSendPosition(transform.position);
                CmdOnStop();
            }
        }
        //[client]
        //Don't have authority listen for position updates and handle 
        else if (!isServer)
        {
            int count = receivedPositions.Count;

            if (count >= processBufferCount && simStep <= count)
            {
                HandleRemotePositionUpdates();
            }
            //better if it was last packet received 
            else if (!serverIsSending && simStep == count)  //here we should predict if we reach the end and were not done sending!
            {
                simStep = 0;
                equipmentBody.isKinematic = false;

                //Probably should clear here .... not for debugging reasons
                receivedPositions.Clear();
                vistedPositions.Clear();

                equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
                equipmentBody.velocity = new Vector3();
                simulation = false;

            }

        }
    }

    // private int numPositions = 0;

    // void Update()
    // {
    //     numPositions = receivedPositions.Count;

    //     if (!hasAuthority && simulation)
    //     {
    //         int count = receivedPositions.Count;

    //         if (count >= processBufferCount && simStep <= numPositions)
    //         {
    //             HandleRemotePositionUpdates();
    //         }
    //         //better if it was last packet received 
    //         else if (!serverIsSending && simStep == numPositions)
    //         {
    //             simStep = 0;
    //             equipmentBody.isKinematic = false;

    //             //Probably should clear here .... not for debugging reasons
    //             //receivedPositions.Clear();
    //             equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
    //             equipmentBody.velocity = new Vector3();
    //             simulation = false;
    //             Debug.Log("REACHED THE END OF THE SENDDDDDDD");
    //             serverIsSending = false;

    //         }
    //     }
    // }

    //Trigger the action being sent
    // //Entry point for Syncing 
    // public void Trigger(long now) { CmdTrigger(now); }

    //Tell server to execute position trigger on everyone
    // [Command(channel = Channels.Unreliable)]
    // private void CmdTrigger(long now) { RPCTrigger(now); }

    //Server says run on client
    // [ClientRpc]
    // private void RPCTrigger(long now)
    // {
    //     if (!hasAuthority)
    //     {
    //         percent = 0;
    //     }
    //     triggerTimeStamp = now;

    //     //Temp clear of lists move later
    //     receivedPositions.Clear();
    //     vistedPositions.Clear();

    //     simulation = true;
    // }

    //Inform server that we've stopped 
    [Command(channel = Channels.Unreliable)]
    void CmdOnStop()
    {
        //should just sent a positional check simulation runs until the end of the execution
        RpcOnStopped();
    }

    //Called when the moving has stopped on Authority
    [ClientRpc]
    protected void RpcOnStopped()
    {
        serverIsSending = false;
    }

    //Removes network authority from the Equipment
    // [Command]
    // private void RemoveAuthority() { GetComponent<NetworkIdentity>().RemoveClientAuthority(); }

    // Server sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPositionRotation(MyNetworkData position, Quaternion rotation)
    {
        Debug.Log("CmdSendPositionRotation");
        RPCSyncPosition(position, rotation);
    }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPosition(MyNetworkData position, Quaternion rotation)
    {
        //make sure were not the server
        serverIsSending = true;
        Debug.Log("RPCSyncPosition");
        // if (!isServer)
        // {
        Debug.Log("Adding Positions");
        receivedPositions.Add(position);
        //RemoteObjPosition = position;
        RemoteObjRotation = rotation;
        // RemoteObjSpeed = Speed;
        // }
    }

    //Handle the received positional updates
    //Runs until the list reaches the end 
    private float percent = 0.0f;

    // [Client]
    private void HandleRemotePositionUpdates()
    {
        int count = receivedPositions.Count;

        if (count > 0 && simStep <= count - 1)
        {
            //get difference 
            //fixedDeltaTime
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = receivedPositions[simStep].position - transform.position;

            double timeDiff = 0;

            if (count == 1 && simStep <= 0)
            {
                Debug.Log("count == 1 && simStep <= 0");

                transform.position = receivedPositions[simStep].position;

            }

            if (count > 1 && simStep > 0)
            {
                timeDiff = new TimeSpan(receivedPositions[simStep].timeSent - receivedPositions[simStep - 1].timeSent).TotalSeconds;
            }
            else //dont have a first value
            {
                timeDiff = new TimeSpan(receivedPositions[simStep].timeSent - triggerTimeStamp).TotalSeconds;
            }

            percent += Time.deltaTime / (float)timeDiff;
            var currentTarget = receivedPositions[simStep].position;
            var LagDistance = currentTarget - transform.position;

            // High distance => sync is to much off => send to position
            // if (LagDistance.magnitude > allowedLagDistance)
            // {
            //     if (debug) Debug.LogWarning("Sync Position too Great! Teleporting equipment.");
            //     transform.position = RemoteObjPosition;
            //     LagDistance = Vector3.zero;
            // }

            vistedPositions.Add(currentTarget);

            transform.position = Vector3.Lerp(transform.position, currentTarget, percent);

            if (percent >= 1)
            {
                percent = 0;
                simStep++;
            }
        }
    }

    // [Client]
    // private void HandleRemoteRotationUpdates()
    // {
    //     var LagRotation = Quaternion.Angle(RemoteObjRotation, transform.rotation);

    //     if (LagRotation > allowedRotationAngle)
    //     {
    //         if (debug) Debug.LogWarning("Sync Rotation too Great! Snapping equipment rotation.");
    //         transform.rotation = RemoteObjRotation;
    //         LagRotation = 0f;
    //     }
    //     if (LagRotation >= 0.5f)
    //     {
    //         transform.rotation = Quaternion.Lerp(transform.rotation, RemoteObjRotation, 0.5f);
    //     }
    // }


    //Gizmo stuff for testing 
    //---------------------------------------------------------------------------------------------

    static void DrawDataPointGizmo(Vector3 pos, Color color)
    {
        // use a little offset because transform.localPosition might be in
        // the ground in many cases
        Vector3 offset = Vector3.up * 0.01f;

        // draw position
        Gizmos.color = color;
        Gizmos.DrawSphere(pos + offset, 0.05f);

    }

    static void DrawLineBetweenDataPoints(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
    }

    // draw the data points for easier debugging
    void OnDrawGizmos()
    {
        foreach (MyNetworkData data in receivedPositions)
        {
            //draw start and goal points
            DrawDataPointGizmo(data.position, Color.yellow);
            // draw line between them
            //DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
        }

        foreach (Vector3 pos in vistedPositions)
        {
            //draw start and goal points
            DrawDataPointGizmo(pos, Color.red);
            // draw line between them
            //DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
        }
    }
}

// public struct MyNetworkData
// {
//     public Vector3 position { get; private set; }
//     public long timeSent { get; private set; }

//     public MyNetworkData(Vector3 position, long timeSent)
//     {
//         this.position = position;
//         this.timeSent = timeSent;
//     }
// }

// public static class CustomReadWriteFunctions
// {
//     public static void WriteMyNetworkData(this NetworkWriter writer, MyNetworkData MyNetworkData)
//     {
//         writer.WriteVector3(MyNetworkData.position);
//         writer.WriteLong(MyNetworkData.timeSent);
//     }

//     public static MyNetworkData ReadMyNetworkData(this NetworkReader reader)
//     {

//         return new MyNetworkData(reader.ReadVector3(), reader.ReadLong());
//     }
// }