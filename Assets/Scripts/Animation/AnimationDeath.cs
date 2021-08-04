using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationDeath : AnimationHumanoid
{
    [SerializeField] private float minAngleThreshold = 15f;

    private Transform orientationHead;
    private Transform orientationBody;
    
    private float currentAngle;
    private float rotationPercentage;
    private float smoothnessFactor;

    void Start()
    {
        orientationHead = transform.Find("Orientation");
        orientationBody = orientationHead.Find("Model");     
    }

    void Update()
    {
        // Angle between head and body
        currentAngle = Quaternion.Angle(orientationHead.rotation, orientationBody.rotation);

        if (currentAngle >= minAngleThreshold)
        {
            // Normalize angle
            rotationPercentage = currentAngle / 180f;

            // Plug into smoothness function [ 14x^2 + 2 ]
            // - rotation percentage of 0% equals smoothness factor of 2
            // - Rotation percentage of 100% equals smoothness factor of 16
            smoothnessFactor = 14f * Mathf.Pow(rotationPercentage, 2f) + 2f;

            // Rotate body towards head
            // The faster the rotation, the rougher the animation 
            orientationBody.rotation = Quaternion.Slerp(orientationBody.rotation, orientationHead.rotation, Time.deltaTime * smoothnessFactor);
        }
    }

    public override void Walk(Vector2 direction)
    {
        animator.SetFloat("Horizontal_W", direction.x);
        animator.SetFloat("Vertical_W", direction.y);
    }

    public override void Sprint(Vector2 direction)
    {
        animator.SetFloat("Horizontal_S", direction.x);
        animator.SetFloat("Vertical_S", direction.y);
    }

    public override void Jump()
    {
        animator.SetBool("Jump", true);
    }

    public override void Land()
    {
        animator.SetBool("Jump", false);
    }

    public override void Crouch()
    {
        animator.SetBool("Crouch", true);
    }

    public override void UnCrouch()
    {
        animator.SetBool("Crouch", false);
    }

    public override void SetGrounded(bool status)
    {
        animator.SetBool("isGrounded", status);
    }

    public override bool IsGrounded()
    {
        return animator.GetBool("isGrounded");
    }
}
