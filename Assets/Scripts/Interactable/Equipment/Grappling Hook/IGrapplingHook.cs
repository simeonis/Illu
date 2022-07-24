using UnityEngine;
using UnityEngine.Events;
public interface IGrapplingHook 
{
    /// <summary>
    /// The point that the grappling hook is attached.
    /// </summary>
    Vector3 GrapplePoint { get; }

    /// <summary>
    /// The exit point of the grappling hook.
    /// </summary>
    Vector3 ExitPoint { get; }

    /// <summary>
    /// If the grappling hook is in the "Idle" state.
    /// </summary>
    public bool IsIdle { get; }

    /// <summary>
    /// If the grappling hook is in the "Fired" state.
    /// </summary>
    public bool IsFired { get; }

    /// <summary>
    /// If the grappling hook is in the "Grappled" state.
    /// </summary>
    public bool IsGrappled { get; }

    /// <summary>
    /// Invoked when the grappling hook enters the "Idle" state.
    /// </summary>
    public UnityEvent IdleEvent { get; }

    /// <summary>
    /// Invoked when the grappling hook enters the "Fired" state.
    /// </summary>
    public UnityEvent FiredEvent { get; }

    /// <summary>
    /// Invoked when the grappling hook enters the "Grappled" state.
    /// </summary>
    public UnityEvent GrappledEvent { get; }
}