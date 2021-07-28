using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : AnimationController
{
    private PlayerControls playerControls;
    private Vector2 movementInput;

    void Awake() {
        playerControls = new PlayerControls();
    }

    void OnEnable() {
        playerControls.Enable();
    }

    void OnDisable() {
        playerControls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        movementInput = playerControls.Land.Movement.ReadValue<Vector2>();

        if (Mathf.Abs(movementInput.x) == Mathf.Abs(movementInput.y))
        {
            animator.SetFloat("Horizontal", 0, 1f, Time.deltaTime * 10f);
            animator.SetFloat("Vertical", 0, 1f, Time.deltaTime * 10f);
        }
        else
        {
            animator.SetFloat("Horizontal", movementInput.x, 1f, Time.deltaTime * 10f);
            animator.SetFloat("Vertical", movementInput.y, 1f, Time.deltaTime * 10f);
        }
    }
}
