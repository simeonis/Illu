using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;

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

public class SyncInteractables : NetworkBehaviour
{
    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 2.0f;
    public float allowedRotationAngle = 5f;

    [SerializeField] private bool debug;

    //private Equipment _equipment;
    //private Rigidbody equipmentBody;

    /*
    *   Set to true when the client is done setting
    *   Note: may still be simulating when true
    */
    private bool serverIsSending = false;

    //local (Not Synced)
    private float lastClientSendTime;

    /*
    *   Index into the list of positions
    *   Only moves forward when the position is reached    
    */
    private int simStep = 0;

    //List of Positions and time sent from the initiating cube 
    //List of reached positions on the reciving client for debugging 
    private List<MyNetworkData> receivedPositions;
    private List<Vector3> vistedPositions;
    private List<Vector3> sentPositions;
    private float oldMagnitude = 0f;
    //private long triggerTimeStamp;
    private const int processBufferCount = 3;

    private NetworkIdentity ni;

    private Vector3 positionToSend = Vector3.zero;
    private Vector3 sentPosition = Vector3.zero;
    private Transform c_Transform = null;
    private Rigidbody c_RB = null;
    private bool beginSending = false;

    private bool shouldTrack = false;

    void Start()
    {
        receivedPositions = new List<MyNetworkData>();
        vistedPositions = new List<Vector3>();
        sentPositions = new List<Vector3>();

        ni = GetComponent<NetworkIdentity>();

        syncInterval = 0.05f;
    }

    public void RegisterInteractableToSync(GameObject go)
    {
        c_Transform = go.GetComponent<Transform>();
        c_RB = go.GetComponent<Rigidbody>();
    }

    public void SetShouldTrack(bool track)
    {
        shouldTrack = track;
    }

    public void SendTransform(Vector3 position) // DO I NEED THIS
    {
        positionToSend = position;
    }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void FixedUpdate()
    {
        //[Server] has authority send commands to other clients
        if (ni.hasAuthority)
        {
            if (shouldTrack == false)
                return;

            if (positionToSend == Vector3.zero)
                return;

            if (positionToSend != sentPosition)
                beginSending = true;

            bool increasing = c_RB.velocity.magnitude > oldMagnitude;

            if (beginSending && Time.time - lastClientSendTime >= syncInterval)
            {
                MyNetworkData currentPos = new MyNetworkData(positionToSend, DateTime.Now.Ticks);
                CmdSendPositionRotation(currentPos);

                sentPositions.Add(positionToSend);

                sentPosition = positionToSend;

                // Update old data for checking against in the next loop
                lastClientSendTime = Time.time;
                oldMagnitude = c_RB.velocity.magnitude;
            }
            else if (!increasing && c_RB.velocity.magnitude <= 0.1)
            {
                // Send one more position
                // CmdSendPosition(transform.position);
                CmdOnStop();
                beginSending = false;
                shouldTrack = false;
                sentPositions.Clear();
            }
        }
        // //[client]
        //Don't have authority listen for position updates and handle 
        else
        {
            int count = receivedPositions.Count;

            if (count >= processBufferCount && simStep <= count)
            {
                HandleRemotePositionUpdates();
            }
            //better if it was last packet received  /
            //*************->MUST BE IF<-************//
            if (!serverIsSending && simStep == count)  //here we should predict if we reach the end and were not done sending!
            {
                simStep = 0;
                receivedPositions.Clear();
                vistedPositions.Clear();
            }
        }
    }

    // Server sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPositionRotation(MyNetworkData position)
    {
        RPCSyncPosition(position);
    }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPosition(MyNetworkData position)
    {
        //make sure were not the server
        serverIsSending = true;

        if (!ni.hasAuthority)
        {
            receivedPositions.Add(position);
        }
    }

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

    //Handle the received positional updates
    //Runs until the list reaches the end 
    private float percent = 0.0f;

    [Client]
    private void HandleRemotePositionUpdates()
    {
        if (c_Transform == null)
            return;

        int count = receivedPositions.Count;

        if (count > 0 && simStep <= count - 1)
        {
            float timeDiff = 0.05f;

            if (simStep <= 0)
            {
                c_Transform.position = receivedPositions[simStep].position;
            }
            else if (count > 1 && simStep > 0)
            {
                timeDiff = (float)new TimeSpan(receivedPositions[simStep].timeSent - receivedPositions[simStep - 1].timeSent).TotalSeconds;
            }
            // else //dont have a first value
            // {
            //     timeDiff = new TimeSpan(receivedPositions[simStep].timeSent - triggerTimeStamp).TotalSeconds;
            // }

            percent += Time.deltaTime / timeDiff;

            var currentTarget = receivedPositions[simStep].position;

            c_Transform.position = Vector3.Lerp(c_Transform.position, currentTarget, percent);

            if (percent >= 1)
            {
                vistedPositions.Add(currentTarget);
                percent = 0;
                simStep++;
            }
        }
    }

    //Gizmo stuff for testing 
    //---------------------------------------------------------------------------------------------
    static void DrawDataPointGizmo(Vector3 pos, Color color)
    {
        Vector3 offset = Vector3.up * 0.01f;
        Gizmos.color = color;
        Gizmos.DrawSphere(pos + offset, 0.05f);
    }

    // draw the data points for easier debugging
    void OnDrawGizmos()
    {
        if (debug)
        {
            foreach (MyNetworkData data in receivedPositions)
            { DrawDataPointGizmo(data.position, Color.yellow); }

            foreach (Vector3 pos in vistedPositions)
            { DrawDataPointGizmo(pos, Color.red); }

            foreach (Vector3 pos in sentPositions)
            { DrawDataPointGizmo(pos, Color.blue); }
        }
    }
}