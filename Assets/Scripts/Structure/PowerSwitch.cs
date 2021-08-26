using UnityEngine;

public class PowerSwitch : Trigger
{
    [SerializeField] private Electronic[] electronics;
    private bool OnState = false;

    public override void Activate()
    {
        OnState = !OnState;
        foreach(Electronic electronic in electronics)
        {
            if (OnState) electronic.PowerOn();
            else electronic.PowerOff();
        }
    }
}