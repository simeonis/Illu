using UnityEngine;

public class GrapplingHookIdleState : GrapplingHookBaseState
{
    public GrapplingHookIdleState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}
    
    RaycastHit hit, gizmosHit;
    float distance = 0f;

    public override void EnterState()
    {
        Ctx.IsPrimaryPressed = false; // Setter prevents value from being set to true
        Ctx.RopeRemaining = Ctx.MaxRopeLength;
        Ctx.RopeRenderer.positionCount = 0;
        RetractHook();
    }

    public override void UpdateState() => CalculateGrappleTarget(out _);

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
        if (CalculateGrappleTarget(out gizmosHit))
        {
            Gizmos.color = Color.green;
            Vector3 direction = gizmosHit.point - Ctx.ExitPoint;
            Gizmos.DrawRay(Ctx.ExitPoint, direction);
            Gizmos.DrawSphere(gizmosHit.point, 0.125f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Ctx.ExitPoint, Ctx.PlayerViewpoint.forward * distance);
        }
    }
    #endif
}