using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationHumanoid : AnimationController
{
    public abstract void Walk(Vector2 direction);
    public abstract void Sprint(Vector2 direction);
    public abstract void Jump();
    public abstract void Land();
    public abstract void Crouch();
    public abstract void UnCrouch();
    public abstract void SetGrounded(bool status);
    public abstract bool IsGrounded();
}
