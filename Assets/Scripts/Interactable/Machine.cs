using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Machine : Interactable
{
    [Header("Player Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerController playerController;
    
    protected PlayerControls playerControls;
    protected bool isInteracting = false;

    private Transform targetTransform;
    private Transform originalParent;
    private Vector3 originalPos;
    private Quaternion originalRot;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Load input system
        // playerControls = playerController.LocalPlayerControls;

        // Assign interaction exit key
        //playerControls.Lever.Interact.performed += context => Exit();

        // Target position for the player's camera
        targetTransform = transform.Find("Camera POV");
    }

    // public override void Seen()
    // {
    //     if (!isInteracting) base.Seen();
    //     else interactMessage = "";
    // }

    public override void Interaction(Interactor interactor)
    {
        if (isInteracting) return;
        isInteracting = true;

        // Lock player
        playerController.visionLocked = true;
        //playerControls.Land.Disable();
        //playerControls.Lever.Enable();

        // Save camera transform properties
        originalParent = playerCamera.transform.parent;
        originalPos = playerCamera.transform.localPosition;
        originalRot = playerCamera.transform.localRotation;

        // Move camera to machine POV
        playerCamera.transform.SetParent(targetTransform);
        playerCamera.transform.localPosition = new Vector3();
        playerCamera.transform.localRotation = new Quaternion();
    }

    public override void InteractionCancelled(Interactor interactor){}

    private void Exit()
    {
        if (!isInteracting) return;
        isInteracting = false;

        // Release player
        playerController.visionLocked = false;
        // playerControls.Land.Enable();
        // playerControls.Lever.Disable();

        // Reset camera to player body
        playerCamera.transform.SetParent(originalParent);
        playerCamera.transform.localPosition = originalPos;
        playerCamera.transform.localRotation = originalRot;
    }
}
