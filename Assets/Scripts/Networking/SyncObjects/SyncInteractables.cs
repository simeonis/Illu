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
    // [Tooltip("How much the local player moves before triggering an Update to all clients")]
    // public float moveTriggerSensitivity = 0.01f;

    // [Tooltip("How far the remote player can be off before snapping to remote position")]
    // public float allowedLagDistance = 2.0f;
    // public float allowedRotationAngle = 5f;

    [SerializeField] private int processBufferCount = 3;

    [SerializeField] private bool debug;

    //private Equipment _equipment;
    //private Rigidbody equipmentBody;

    /*
    *   Set to true when the client is done setting
    *   Note: may still be processing positions on client when false
    */
    [SyncVar]
    private bool serverIsSending = false;

    /*
    *   Index into the list of positions
    *   Only moves forward when the position is reached    
    */
    private int simStep = 0;
    private bool shouldTrack = false;

    //List of Positions and time sent from the initiating cube 
    //List of reached positions on the reciving client for debugging 
    private List<InteractableSyncData> receivedPositions;
    private List<Vector3> vistedPositions;
    private List<Vector3> sentPositions;

    //local (Not Synced)
    private float lastClientSendTime;
    private float oldMagnitude = 0f;
    private Vector3 sentPosition = Vector3.zero;

    private NetworkIdentity ni;
    private Transform c_Transform = null;
    private Rigidbody c_RB = null;

    [SyncVar]
    public bool isEquipped = false;
    public bool clientUnEquiped = true;

    void Start()
    {
        receivedPositions = new List<InteractableSyncData>();
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

    public void SetShouldTrack(bool track) { shouldTrack = track; }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 

    private TimeSpan serverStartTime;
    void FixedUpdate()
    {
        //[Server] has authority send commands to other clients
        if (ni.hasAuthority)
        {
            if (shouldTrack == false)
                return;

            if (c_Transform == null)
                return;

            if (c_Transform.position == sentPosition)
                return;

            bool increasing = c_RB.velocity.magnitude > oldMagnitude;

            if (Time.time - lastClientSendTime >= syncInterval)
            {
                serverIsSending = true;
                InteractableSyncData dataFrame = new InteractableSyncData(c_Transform.position, c_Transform.rotation, DateTime.Now.Ticks);
                CmdSendPositionRotation(dataFrame);

                sentPositions.Add(c_Transform.position);

                if (sentPositions.Count == 1)
                {
                    serverStartTime = DateTime.Now.TimeOfDay;
                }

                sentPosition = c_Transform.position;

                // Update old data for checking against in the next loop
                lastClientSendTime = Time.time;
                oldMagnitude = c_RB.velocity.magnitude;
            }
            else if (!increasing && c_RB.velocity.magnitude <= 0.1)
            {
                // Send one more position
                // CmdSendPosition(transform.position);
                serverIsSending = false;
                shouldTrack = false;
                sentPositions.Clear();
                Debug.Log($"Server Time diff: {DateTime.Now.TimeOfDay - serverStartTime}");
            }
        }
        // //[client]
        //Don't have authority listen for position updates and handle 
        // else
        // {
        //     int count = receivedPositions.Count;

        //     if (count >= processBufferCount && simStep <= count)
        //     {
        //         HandleRemotePositionUpdates();
        //     }
        //     //better if it was last packet received  /
        //     //*************->MUST BE IF<-************//
        //     if (!serverIsSending && simStep == count)  //here we should predict if we reach the end and were not done sending!
        //     {
        //         simStep = 0;
        //         receivedPositions.Clear();
        //         vistedPositions.Clear();
        //     }
        // }
    }

    private TimeSpan startTime;
    void Update()
    {
        if (c_Transform == null)
            return;

        if (c_RB == null)
            return;

        if (!ni.hasAuthority)
        {
            int count = receivedPositions.Count;

            if (!clientUnEquiped)
                return;

            if (count == 1)
            {
                startTime = DateTime.Now.TimeOfDay;
            }

            if (count >= processBufferCount && simStep <= count)
            {
                HandleRemotePositionUpdates();
            }

            // if (serverIsSending && simStep == count)
            // {
            //     Debug.Log("I'm waiting for information");
            // }

            //better if it was last packet received  /
            //*************->MUST BE IF<-************//
            if (!serverIsSending && simStep == count)  //here we should predict if we reach the end and were not done sending!
            {
                simStep = 0;

                if (receivedPositions.Count > 0)
                {
                    Debug.Log($"Time diff: {startTime - DateTime.Now.TimeOfDay}");
                }
                receivedPositions.Clear();
                vistedPositions.Clear();

                if (!isEquipped)
                {
                    c_RB.isKinematic = false;
                }
            }
        }
    }

    // Server sends Pos and Rot 
    [Command(channel = Channels.Unreliable)]
    void CmdSendPositionRotation(InteractableSyncData dataFrame) { RPCSyncPosition(dataFrame); }

    //Server broadcasts Pos and Rot to all clients  
    [ClientRpc]
    public void RPCSyncPosition(InteractableSyncData dataFrame)
    {
        //make sure were not the server
        if (!ni.hasAuthority)
            receivedPositions.Add(dataFrame);

        Debug.Log("Time " + DateTime.Now.TimeOfDay);
    }

    //Handle the received positional updates
    //Runs until the list reaches the end 
    private float percent = 0.0f;
    private Vector3 currentPos;
    private Quaternion currentRot;

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
                currentPos = c_Transform.position = receivedPositions[simStep].position;
                currentRot = c_Transform.rotation = receivedPositions[simStep].rotation;
            }
            else if (count > 1 && simStep > 0)
            {
                c_RB.isKinematic = true;
                timeDiff = (float)new TimeSpan(receivedPositions[simStep].timeSent - receivedPositions[simStep - 1].timeSent).TotalSeconds;
            }
            // else //dont have a first value
            // {
            //     timeDiff = new TimeSpan(receivedPositions[simStep].timeSent - triggerTimeStamp).TotalSeconds;
            // }

            percent += Time.fixedTime / timeDiff;

            var targetPos = receivedPositions[simStep].position;
            var targetRot = receivedPositions[simStep].rotation;

            c_Transform.position = Vector3.Lerp(currentPos, targetPos, percent);
            c_Transform.rotation = Quaternion.Lerp(currentRot, targetRot, percent);

            if (percent >= 1)
            {
                currentPos = c_Transform.position;
                currentRot = c_Transform.rotation;
                vistedPositions.Add(targetPos);
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
            foreach (InteractableSyncData data in receivedPositions)
            { DrawDataPointGizmo(data.position, Color.yellow); }

            foreach (Vector3 pos in vistedPositions)
            { DrawDataPointGizmo(pos, Color.red); }

            foreach (Vector3 pos in sentPositions)
            { DrawDataPointGizmo(pos, Color.blue); }
        }
    }
}

public struct InteractableSyncData
{
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public long timeSent { get; private set; }

    public InteractableSyncData(Vector3 position, Quaternion rotation, long timeSent)
    {
        this.position = position;
        this.rotation = rotation;
        this.timeSent = timeSent;
    }
}

public struct PlayerSyncData
{
    public Vector3 position { get; private set; }
    public Quaternion headRot { get; private set; }
    public Quaternion bodyRot { get; private set; }
    public Quaternion rootRot { get; private set; }

    public PlayerSyncData(Vector3 position, Quaternion headRot, Quaternion bodyRot, Quaternion rootRot)
    {
        this.position = position;
        this.headRot = headRot;
        this.bodyRot = bodyRot;
        this.rootRot = rootRot;
    }
}

public static class CustomReadWriteFunctions
{
    public static void WriteInteractableSyncData(this NetworkWriter writer, InteractableSyncData interactableSyncData)
    {
        writer.WriteVector3(interactableSyncData.position);
        writer.WriteQuaternion(interactableSyncData.rotation);
        writer.WriteLong(interactableSyncData.timeSent);
    }

    public static InteractableSyncData ReadInteractableSyncData(this NetworkReader reader)
    {
        return new InteractableSyncData(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadLong());
    }

    public static void WritePlayerSyncData(this NetworkWriter writer, PlayerSyncData playerSyncData)
    {
        writer.WriteVector3(playerSyncData.position);
        writer.WriteQuaternion(playerSyncData.headRot);
        writer.WriteQuaternion(playerSyncData.bodyRot);
        writer.WriteQuaternion(playerSyncData.rootRot);
    }
    public static PlayerSyncData ReadPlayerSyncData(this NetworkReader reader)
    {
        return new PlayerSyncData(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadQuaternion(), reader.ReadQuaternion());
    }
}