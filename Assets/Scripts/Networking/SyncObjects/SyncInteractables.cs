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

public class SyncInteractables : NetworkBehaviour
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

    private List<Interactable> interactables;

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
    private bool initiatorDoneSending = false;


    //List of Positions and time sent from the initiating cube 
    //List of reached positions on the reciving client for debugging 
    private List<MyNetworkData> receivedPositions;
    private List<Vector3> vistedPositions;
    private List<Vector3> sentPositions;


    private float oldMagnitude = 0f;
    private long triggerTimeStamp;

    private const int processBufferCount = 3;

    private NetworkIdentity ni;

    void Start()
    {
        interactables = new List<Interactable>();

        //_equipment = GetComponent<Equipment>();
        //equipmentBody = _equipment.equipmentBody;
        receivedPositions = new List<MyNetworkData>();
        vistedPositions = new List<Vector3>();
        sentPositions = new List<Vector3>();

        ni = GetComponent<NetworkIdentity>();
    }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
    void FixedUpdate()
    {

        //Debug.Log("Sync Interactables: Is Local Player " + ni.hasAuthority + " NetID: " + ni.netId.ToString());
        //[Server] has authority send commands to other clients
        if (ni.hasAuthority)
        {
            if (interactables.Count <= 0)
                return;

            Debug.Log(interactables[0].transform.position.x);

            var equipmentBody = interactables[0].GetComponent<Rigidbody>();

            bool increasing = equipmentBody.velocity.magnitude > oldMagnitude;


            if (increasing && Time.time - lastClientSendTime >= syncInterval)
            {
                MyNetworkData currentPos = new MyNetworkData(transform.position, DateTime.Now.Ticks);
                CmdSendPositionRotation(currentPos, transform.rotation);

                sentPositions.Add(transform.position);

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
        else
        {
            if (interactables.Count <= 0)
                return;


            int count = receivedPositions.Count;

            if (count >= processBufferCount && simStep <= count)
            {
                HandleRemotePositionUpdates();
            }
            //better if it was last packet received 
            else if (!serverIsSending && simStep == count)  //here we should predict if we reach the end and were not done sending!
            {

                simStep = 0;
                //equipmentBody.isKinematic = false;

                //Probably should clear here .... not for debugging reasons
                // receivedPositions.Clear();
                // vistedPositions.Clear();

                // equipmentBody.interpolation = RigidbodyInterpolation.Interpolate;
                // equipmentBody.velocity = new Vector3();
                // simulation = false;
            }

        }

        // if (receivedPositions.Count > 0)
        //     Debug.Log("Pos Len " + receivedPositions.Count);

    }

    public void RegisterInteractableToSync(Interactable go)
    {
        interactables.Add(go);
    }

    // Server sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPositionRotation(MyNetworkData position, Quaternion rotation)
    {
        RPCSyncPosition(position, rotation);
    }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPosition(MyNetworkData position, Quaternion rotation)
    {
        //make sure were not the server
        serverIsSending = true;
        if (!ni.hasAuthority)
        {
            receivedPositions.Add(position);
            //RemoteObjPosition = position;
            // RemoteObjRotation = rotation;
            // RemoteObjSpeed = Speed;
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

    // [Client]
    private void HandleRemotePositionUpdates()
    {
        int count = receivedPositions.Count;

        Debug.Log("SimStep " + simStep + " Count " + count + " interactables LEN" + interactables.Count);

        if (count > 0 && simStep <= count - 1)
        {
            //get difference 
            //fixedDeltaTime
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = receivedPositions[simStep].position - transform.position;

            double timeDiff = 0;

            var ITransform = interactables[0].GetComponent<Transform>();

            if (simStep <= 0)
            {

                ITransform.position = receivedPositions[simStep].position;
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



            ITransform.position = Vector3.Lerp(ITransform.position, currentTarget, percent);

            if (percent >= 1)
            {
                Debug.Log("Percent Equals One");
                percent = 0;
                simStep++;
            }
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
        Debug.Log("Sent Positions");
        int i = 0;
        foreach (MyNetworkData data in receivedPositions)
        {
            Debug.Log("REC POS (" + i + ")-> " + data.position);
            //draw start and goal points
            DrawDataPointGizmo(data.position, Color.yellow);
            // draw line between them
            //DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
            i++;
        }

        foreach (Vector3 pos in vistedPositions)
        {
            //draw start and goal points
            DrawDataPointGizmo(pos, Color.red);
            // draw line between them
            //DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
        }

        int j = 0;
        foreach (Vector3 pos in sentPositions)
        {
            Debug.Log("Sent POS (" + j + ")-> " + pos);
            //draw start and goal points
            DrawDataPointGizmo(pos, Color.red);
            // draw line between them
            //DrawLineBetweenDataPoints(transform.position, RemoteObjPosition, Color.cyan);
            j++;
        }
    }
}