using UnityEngine;
public interface IPlayerMotor {

    /// <summary>
    /// The player's orientation Transform component.
    /// </summary>
    public Transform Orientation { get; }

    /// <summary>
    /// The player's camera Transform component.
    /// </summary>
    public Transform Viewpoint { get; }

    /// <summary>
    /// The player's Rigidbody component.
    /// </summary>
    public Rigidbody Rigidbody { get; }

    /// <summary>
    /// If the player is in the "Grounded" state.
    /// </summary>
    public bool IsGrounded { get; }

    /// <summary>
    /// If the player is pressing a movement key.
    /// </summary>
    public bool IsMovementPressed { get; }

    /// <summary>
    /// Notifies the player when to move.
    /// </summary>
    public void SetMovement(Vector2 input);

    /// <summary>
    /// Notifies the player when to jump.
    /// </summary>
    public void SetJump(bool isPressed);

    /// <summary>
    /// Notifies the player when to sprint.
    /// </summary>
    public void SetSprint(bool isPressed);

    public void EnableRagdoll();

    public void DisableRagdoll();
}