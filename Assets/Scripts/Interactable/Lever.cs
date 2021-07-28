using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Machine
{
    [Header("Animation")]
    [SerializeField] protected Animator animator;
    
    [Header("Activation Settings")]
    [SerializeField, ColorUsageAttribute(true, true)] private Color poweredOnColor;
    [SerializeField] private bool powered;

    private Material activationMaterial;
    private Vector2 movementInput;
    private string originalMessage;

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

        movementInput = playerControls.Lever.Movement.ReadValue<Vector2>();

        // Nullify diagonals
        // Example: 
        // #1   (1, 1) -> (0, 0)
        // #2   (-1, 1) -> (0, 0)
        if (Mathf.Abs(movementInput.x) == Mathf.Abs(movementInput.y))
        {
            animator.SetFloat("Horizontal", 0, 1f, Time.deltaTime * 10f);
            animator.SetFloat("Vertical", 0, 1f, Time.deltaTime * 10f);
        }
        // Strictly horizontal or vertical
        // Example:
        // #1   (1, 0)
        // #2   (-1, 0)
        // #3   (0, 1)
        // #4   (0, -1)
        else
        {
            animator.SetFloat("Horizontal", movementInput.x, 1f, Time.deltaTime * 10f);
            animator.SetFloat("Vertical", movementInput.y, 1f, Time.deltaTime * 10f);
        }
    }
}
