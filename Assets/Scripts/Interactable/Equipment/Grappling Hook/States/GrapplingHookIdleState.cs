using UnityEngine;

public class GrapplingHookIdleState : GrapplingHookBaseState
{
    public GrapplingHookIdleState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    public override void EnterState()
    {
        Ctx.IdleEvent?.Invoke();
        Ctx.IsPrimaryPressed = false; // Setter prevents value from being set to true
        Ctx.RopeRemaining = Ctx.MaxRopeLength;
        Ctx.RopeRenderer.positionCount = 0;
        RetractHook();
    }

    public override void UpdateState() => CalculateGrappleTarget();

    bool CalculateGrappleTarget()
    {
        Vector3 origin = FindNearestPointOnLine(Ctx.PlayerMotor.Viewpoint.position, Ctx.PlayerMotor.Viewpoint.forward, Ctx.ExitPoint);
        return SimulateGrapple(origin, Ctx.PlayerMotor.Viewpoint.forward);
    }

    Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }

    public override void CheckSwitchState()
    {
        if (Ctx.IsPrimaryPressed)
        {
            SwitchState(Factory.GetState<GrapplingHookFiredState>());
        }
    }

    #if UNITY_EDITOR
    public override void GizmosState()
    {
        if (CalculateGrappleTarget())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Ctx.GrapplePoint, 0.125f);
        }
        else
        {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawLine(Ctx.ExitPoint, Ctx.GrapplePoint);
    }
    #endif
}