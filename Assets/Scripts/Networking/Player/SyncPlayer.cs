using UnityEngine;
using Mirror;

//////////////////////////////////////////////////////////////////////////
///Sync Player
///Handles sending data over the network and smoothing the recieved values
//////////////////////////////////////////////////////////////////////////

///To Do: Top level transform send the rotation for death in player its (player -> )

public class SyncPlayer : NetworkBehaviour
{

    [Header("Authority")]
    [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
    public bool clientAuthority = true;

    [Header("Networking Parameters")]
    [Tooltip("How much the local player moves before triggering an Update to all clients")]
    public float moveTriggerSensitivity = 0.01f;

    [Tooltip("How far the remote player can be off before snapping to remote position")]
    public float allowedLagDistance = 10.0f;

    [Tooltip("How fast a remote player corrects its look")]
    [SerializeField] private float correntRotSpeed = 2.0f;

    [Tooltip("How fast a remote player corrects its look")]
    [SerializeField] private float allowRotLagAmount = 0.25f;

    [Tooltip("Apply Rotation Smoothing")]
    [SerializeField] private bool smoothRot = false;

    [Header("Debug Position")]
    public bool debug = false;

    //Transforms remember to pass in in Inspector
    [Header("Players Transforms")]
    [Tooltip("Pass in the Controller")]
    public Transform controller;

    [Tooltip("Pass in the Player Camera")]
    public Transform playerCamera;

    [Tooltip("Pass in Orientation")]
    public Transform orientation;

    private Vector3 RemotePlayerPosition;
    private Quaternion RemotePlayerRotation;
    private Quaternion RemotePlayerBodyRotation;
    private Quaternion RemotePlayerRootRotation;

    Vector3 lastPosition;
    Quaternion lastRotation;
    Quaternion lastBodyRotation;
    Quaternion lastRootRotation;
    Vector3 transPosition;

    // local authority send time
    float lastClientSendTime;

    private NetworkPlayerController networkPlayerController;

    // Start is called before the first frame update
    void Start()
    {
        networkPlayerController = GetComponentInChildren<NetworkPlayerController>();

        if (isClient)
        {
            // send to server if we are local player
            if (hasAuthority)
            {
                networkPlayerController.LocalPlayerControls.Land.Crouch.performed += context => CmdHandleCrouch(true);
                networkPlayerController.LocalPlayerControls.Land.Crouch.canceled += context => CmdHandleCrouch(false);

                networkPlayerController.LocalPlayerControls.Land.Sprint.performed += context => CmdHandleSprint(true);
                networkPlayerController.LocalPlayerControls.Land.Sprint.canceled += context => CmdHandleSprint(false);

                networkPlayerController.LocalPlayerControls.Land.Jump.performed += context => CmdSendJump();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // send to server if we are local player and have authority
        if (isClient)
        {
            if (hasAuthority)
            {
                // check only each 'syncInterval'
                if (Time.time - lastClientSendTime >= syncInterval)
                {
                    if (HasEitherMovedRotated())
                        CMDSendPosAndRot(transform.position, playerCamera.rotation, orientation.rotation, networkPlayerController.GetRotation());

                    lastClientSendTime = Time.time;
                }
            }
            else
            {
                HandleRemotePositionUpdates();
                HandleRemoteRotationUpdates();
            }

            transPosition = transform.position;
        }
    }

    //return whether there's been a change in position or rotation 
    //to prevent spamming the network when not moving 
    bool HasEitherMovedRotated()
    {
        Quaternion RootRotation = networkPlayerController.GetRotation();
        // moved or rotated?
        bool moved = Vector3.Distance(lastPosition, transform.position) > moveTriggerSensitivity;
        bool headRotated = Quaternion.Angle(lastRotation, playerCamera.rotation) > moveTriggerSensitivity;
        bool bodyRotated = Quaternion.Angle(lastBodyRotation, orientation.rotation) > moveTriggerSensitivity;
        bool rootRotated = Quaternion.Angle(lastRootRotation, RootRotation) > moveTriggerSensitivity;

        bool change = moved || headRotated || bodyRotated || rootRotated;

        if (change)
        {
            //position/rotation/bodyrot
            lastPosition = transform.position;
            lastRotation = playerCamera.rotation;
            lastBodyRotation = orientation.rotation;
            lastRootRotation = RootRotation;
        }

        return change;
    }

    // local authority client sends sync message to server for broadcasting
    [Command(channel = Channels.Unreliable)]
    void CMDSendPosAndRot(Vector3 position, Quaternion headRot, Quaternion bodyRot, Quaternion rootRot)
    {
        // Ignore messages from client if notCmdClientToServerSync in client authority mode
        if (!clientAuthority)
            return;

        RPCSyncPosition(position, headRot, bodyRot, rootRot);
    }

    [ClientRpc]
    public void RPCSyncPosition(Vector3 position, Quaternion headRot, Quaternion bodyRot, Quaternion rootRot)
    {
        if (debug)
            Debug.Log("RPC Position: " + position + "HeadRot " + headRot + "bodyRot " + bodyRot);
        RemotePlayerPosition = position;
        RemotePlayerRotation = headRot;
        RemotePlayerBodyRotation = bodyRot;
        RemotePlayerRootRotation = rootRot;
    }

    [Client]
    public void HandleRemotePositionUpdates()
    {
        var LagDistance = RemotePlayerPosition - transform.position;

        //High distance => sync is to much off => send to position
        if (LagDistance.magnitude > networkPlayerController.moveSpeed / 3)
        {
            if (debug)
                Debug.LogWarning("Sync Position to Great");
            controller.position = RemotePlayerPosition;
            LagDistance = Vector3.zero;
        }

        //ignore the y distance
        // LagDistance -= LagDistance.up;

        Vector3 unwantedUp = Vector3.Dot(LagDistance, orientation.up) * orientation.up;
        Vector3 FinalLagDistance = LagDistance - unwantedUp;

        

        if (LagDistance.magnitude < 0.025f)
        {   //Player is nearly at the point
            networkPlayerController.moveDirection = Vector3.zero;
        }
        else //Player has to go to the point
        {
            networkPlayerController.moveDirection = FinalLagDistance;
        }
    }


    [Client]
    private void HandleRemoteRotationUpdates()
    {
        // The step size is equal to speed times frame time.
        var step = correntRotSpeed * Time.deltaTime;

        float camDiff = Quaternion.Dot(playerCamera.rotation, RemotePlayerRotation);
        float bodyDiff = Quaternion.Dot(orientation.rotation, RemotePlayerBodyRotation);
        float rootDiff = Quaternion.Dot(networkPlayerController.GetRotation(), RemotePlayerRootRotation);

        if (debug)
            Debug.Log("camDiff " + camDiff + " bodyDiff" + bodyDiff);

        if (smoothRot && camDiff <= allowRotLagAmount)
        {
            // Rotate our playerCamera a step closer to the target's.
            playerCamera.rotation = Quaternion.RotateTowards(playerCamera.rotation, RemotePlayerRotation, step);
        }
        else
        {
            playerCamera.rotation = RemotePlayerRotation;
        }

        if (smoothRot && bodyDiff <= allowRotLagAmount)
        {
            // Rotate our orientation a step closer to the target's.
            orientation.rotation = Quaternion.RotateTowards(orientation.rotation, RemotePlayerBodyRotation, step);
        }
        else
        {
            orientation.rotation = RemotePlayerBodyRotation;
        }

        if (smoothRot && rootDiff <= allowRotLagAmount)
        {
            // Rotate our orientation a step closer to the target's.
            networkPlayerController.SetRotation(Quaternion.RotateTowards(networkPlayerController.GetRotation(), RemotePlayerRootRotation, step));
        }
        else
        {
            networkPlayerController.SetRotation(RemotePlayerRootRotation);

        }
    }


    //Handle Sending Crouch, Jump, Sprint
    //--------------------------------------------------------------
    [Command(channel = Channels.Unreliable)]
    private void CmdHandleCrouch(bool CrouchState)
    {
        RpcCrouch(CrouchState);
    }

    [ClientRpc]
    private void RpcCrouch(bool CrouchState)
    {
        if (CrouchState)
        {
            networkPlayerController.PerformCrouch();
        }
        else
        {
            networkPlayerController.PerformUnCrouch();
        }
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdHandleSprint(bool SprintState)
    {
        RpcSprint(SprintState);
    }

    [ClientRpc]
    private void RpcSprint(bool SprintState)
    {
        if (SprintState)
        {
            networkPlayerController.PerformSprint();
        }
        else
        {
            networkPlayerController.PerformWalk();
        }
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendJump()
    {
        RpcSendJump();
    }

    [ClientRpc]
    private void RpcSendJump()
    {
        networkPlayerController.PerformJump();
    }

    //Static Draw Methods
    //--------------------------------------------------------------
    static void DrawDataPointGizmo(Vector3 pos, Color color)
    {
        // use a little offset because transform.localPosition might be in
        // the ground in many cases
        Vector3 offset = Vector3.up * 0.01f;

        // draw position
        Gizmos.color = color;
        Gizmos.DrawSphere(pos + offset, 0.5f);

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
        DrawDataPointGizmo(RemotePlayerPosition, Color.green);
        DrawDataPointGizmo(transPosition, Color.red);

        // draw line between them
        DrawLineBetweenDataPoints(transPosition, RemotePlayerPosition, Color.cyan);
    }
}
