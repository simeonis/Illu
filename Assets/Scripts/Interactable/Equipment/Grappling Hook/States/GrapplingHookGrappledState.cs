using UnityEngine;

public class GrapplingHookGrappledState : GrapplingHookBaseState
{
    public GrapplingHookGrappledState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base (currentContext, grapplingHookStateFactory) {}

    Vector3 testPosition = Vector3.zero;
    Vector3 anchorToPlayer = Vector3.zero;

    public override void EnterState()
    {
        Ctx.RopeRenderer.positionCount = 2;
    }

    public override void UpdateState()
    {
        Ctx.RopeRenderer.SetPosition(0, Ctx.ExitPoint);
        Ctx.RopeRenderer.SetPosition(1, Ctx.GrapplePoint);
    }

    public override void FixedUpdateState()
    {
        // Predict where player (grapple exit point) will be next physics frame
        testPosition = Ctx.ExitPoint + (Ctx.PlayerRigidbody.velocity * Time.fixedDeltaTime);
        
        // Calculate if test position is outside of acceptable rope range
        anchorToPlayer = (testPosition - Ctx.GrapplePoint);
        if (anchorToPlayer.magnitude > Ctx.GrappleDistance) {
            testPosition = Ctx.GrapplePoint + (anchorToPlayer.normalized * Ctx.GrappleDistance);
        }
        
        // Constrain player
        Ctx.PlayerRigidbody.velocity = (testPosition - Ctx.ExitPoint) / Time.fixedDeltaTime;
    }

    public override void ExitState()
    {
        Ctx.RopeRenderer.positionCount = 0;
        RetractHook();
    }

    public override void CheckSwitchState()
    {
        if (!Ctx.IsPrimaryPressed)
            SwitchState(Factory.GetState<GrapplingHookIdleState>());
    }

    #if UNITY_EDITOR
    public override void GizmosState()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Ctx.GrapplePoint, Ctx.GrappleDistance);
    }
    #endif
}