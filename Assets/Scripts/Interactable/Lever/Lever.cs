using System.Collections;
using Mirror;
using UnityEngine;


public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;

    private bool onState = false;
    private bool locked = false;
    private IEnumerator enumerator;
    NetworkSimpleData _networkSimpleData;

    protected override void Start()
    {
        base.Start();
        _networkSimpleData = GetComponent<NetworkSimpleData>();
        _networkSimpleData.RegisterData();

        CustomEventHandler<bool> subToEvent = _networkSimpleData.EventHandlersDictionary["yoo"] as CustomEventHandler<bool>;
        subToEvent += lever_EventHandler;
    }

    void lever_EventHandler(object sender, DataChangedEventArgs<bool> e)
    {
        Debug.Log("From lever " + e.data + " changed at " + e.TimeSent);

        onState = e.data;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());
    }

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || locked || !target) return;

        // Request Authority
        base.Interaction(interactor);

        locked = true;
        onState = !onState;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());
    }

    public override void OnStartAuthority()
    {
        Debug.Log("I the lever, the mighty lever, have been granted authority.");
        _networkSimpleData.SendData(onState);
    }

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