using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;

///////////////////////////////////////////////////////////////////////////
/// in equimpent if released from player
/// start sending position and rotation to all client copies
/// other client -> receive pos and rot and add to buffer 
///////////////////////////////////////////////////////////////////////////

//TODO
// add on collision with player pass auth 
// add on interacte change owner 
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
    private List<InteractableSyncData> receivedPositions;

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
        ni = GetComponent<NetworkIdentity>();
        syncInterval = 0.05f;
    }

    public void RegisterInteractableToSync(GameObject go)
    {
        c_Transform = go.GetComponent<Transform>();
        c_RB = go.GetComponent<Rigidbody>();
        Debug.Log("RB " + c_RB.isKinematic);
    }

    public void SetShouldTrack(bool track) { shouldTrack = track; }

    //Update loop called on both Authority and other Clients 
    //Checks who it's on internally 
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
            //here we should predict if we reach the end and were not done sending!
            //better if it was last packet received  /
            //*************->MUST BE IF<-************//
            if (!serverIsSending && simStep == count)
            {
                simStep = 0;
                ClearPositions();

                if (!isEquipped)
                    c_RB.isKinematic = false;

            }
        }
    }

    public void ClearPositions()
    {
        receivedPositions.Clear();
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

            percent += Time.deltaTime / timeDiff;

            var targetPos = receivedPositions[simStep].position;
            var targetRot = receivedPositions[simStep].rotation;

            c_Transform.position = Vector3.Lerp(currentPos, targetPos, percent);
            c_Transform.rotation = Quaternion.Lerp(currentRot, targetRot, percent);

            if (percent >= 1)
            {
                currentPos = c_Transform.position;
                currentRot = c_Transform.rotation;
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
        }
    }
}

