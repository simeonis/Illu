using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Lever : Machine
{
    [Header("Animation")]
    [SerializeField] protected Animator animator;
    
    [Header("Activation Settings")]
    [SerializeField, ColorUsageAttribute(true, true)] private Color poweredOnColor;
    [SerializeField] private bool powered;

    private Material activationMaterial;
    private Vector2 movementInput;
    private string originalMessage;

    private enum Orientation { Up, Down, Right, Left, Null };
    private Orientation currentOrientation;

    protected override void Start()
    {
        base.Start();

        // Store interact message to append Powered/Unpowered
        originalMessage = interactMessage;

        // Search for light material
        foreach (Material material in GetComponent<Renderer>().materials)
        {
            if (material.name.Replace(" (Instance)", "") == "Inner Face")
            {
                activationMaterial = material;
            }
        }

        // Initial state
        if (powered) Enable();
        else Disable();
    }

    public void Enable()
    {
        powered = true;
        interactMessage = originalMessage + " (Powered)";
        activationMaterial.SetColor("_EmissiveColor", poweredOnColor);
    }

    public void Disable()
    {
        powered = false;
        interactMessage = originalMessage + " (Unpowered)";
        activationMaterial.SetColor("_EmissiveColor", poweredOnColor * 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInteracting || !powered) return;

        // Get User Input
        movementInput = playerControls.Lever.Movement.ReadValue<Vector2>();

        // Nullify diagonals
        if (Mathf.Abs(movementInput.x) == Mathf.Abs(movementInput.y))
        {
            movementInput = new Vector2();
        }

        // Set Animation
        animator.SetFloat("Horizontal", movementInput.x, 1f, Time.deltaTime * 10f);
        animator.SetFloat("Vertical", movementInput.y, 1f, Time.deltaTime * 10f);

        // Up
        if (movementInput.y > 0f && movementInput.x == 0f)
        {
            if (currentOrientation != Orientation.Up)
            {
                currentOrientation = Orientation.Up;
                Up();
            }
        }
        // Down
        else if (movementInput.y < 0f && movementInput.x == 0f)
        {
            if (currentOrientation != Orientation.Down)
            {
                currentOrientation = Orientation.Down;
                Down();
            }
        }
        // Right
        else if (movementInput.y == 0f && movementInput.x > 0f)
        {
            if (currentOrientation != Orientation.Right)
            {
                currentOrientation = Orientation.Right;
                Right();
            }
        }
        // Left
        else if (movementInput.y == 0f && movementInput.x < 0f)
        {
            if (currentOrientation != Orientation.Left)
            {
                currentOrientation = Orientation.Left;
                Left();
            }
        }
        // Null
        else
        {
            if (currentOrientation != Orientation.Null)
            {
                currentOrientation = Orientation.Null;
            }
        }
    }

    protected abstract void Up();
    protected abstract void Down();
    protected abstract void Right();
    protected abstract void Left();
}
