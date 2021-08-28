using System.Collections;
using UnityEngine;


public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;

    private bool onState = false;
    private bool locked = false;
    private IEnumerator enumerator;
    //NetworkSimpleData _networkSimpeData;

    protected override void Start()
    {
        base.Start();
        _networkSimpeData = new NetworkSimpleData();
        _networkSimpeData.DataChanged += l_EventHandler;
    }
    void l_EventHandler(object sender, DataChangedEventArgs e)
    {
        Debug.Log("From lever " + e.data + " changed at " + e.TimeSent);

        //onState = e.data;
        //StartCoroutine(enumerator = Switch());
    }

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || locked || !target) return;

        locked = true;
        onState = !onState;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());

        Debug.Log("Interact: Lever");

        _networkSimpeData.SendData(onState);
    }

    public override void InteractionCancelled(Interactor interactor) { }

    private IEnumerator Switch()
    {
        float currentState = onState ? 0 : 1;
        float targetState = onState ? 1 : 0;

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime * animationSpeed;

            animator.SetFloat("Pulled", Mathf.Lerp(currentState, targetState, percent));

            yield return null;
        }

        locked = false;
        target.Activate();
    }
}
