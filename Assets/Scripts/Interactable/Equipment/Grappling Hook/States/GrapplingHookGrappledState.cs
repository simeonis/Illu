using UnityEngine;

public class GrapplingHookGrappledState : GrapplingHookBaseState
{
    public GrapplingHookGrappledState(GrapplingHookStateMachine currentContext, GrapplingHookStateFactory grapplingHookStateFactory)
    : base(currentContext, grapplingHookStateFactory) { }

    Vector3 testPosition = Vector3.zero;
    Vector3 anchorToPlayer = Vector3.zero;

    float desiredLength, currentLength;

    public override void EnterState()
    {
        Ctx.GrappledEvent?.Invoke();
        Ctx.RopeRenderer.positionCount = 2;

        // TODO: Shorten desiredLength to ensure the player won't hit the ground 
        currentLength = Vector3.Distance(Ctx.ExitPoint, Ctx.GrapplePoint);
        desiredLength = Ctx.GrappleDistance;
    }

    public override void UpdateState()
    {
        Ctx.RopeRenderer.SetPosition(0, Ctx.ExitPoint);
        Ctx.RopeRenderer.SetPosition(1, Ctx.GrapplePoint);
    }

    public override void FixedUpdateState()
    {
        currentLength = Mathf.MoveTowards(currentLength, desiredLength, 10f * Time.fixedDeltaTime);

        // Predict where player (grapple exit point) will be next physics frame
        testPosition = Ctx.ExitPoint + (Ctx.PlayerMotor.Rigidbody.velocity * Time.fixedDeltaTime);

        // Calculate if test position is outside of acceptable rope range
        anchorToPlayer = (testPosition - Ctx.GrapplePoint);
        if (anchorToPlayer.magnitude > currentLength)
        {
            testPosition = Ctx.GrapplePoint + (anchorToPlayer.normalized * currentLength);
        }

        // Constrain player
        Ctx.PlayerMotor.Rigidbody.velocity = (testPosition - Ctx.ExitPoint) / Time.fixedDeltaTime;
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