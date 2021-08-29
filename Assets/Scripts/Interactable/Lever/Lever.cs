using System.Collections;
using System;
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
        _networkSimpleData = new NetworkSimpleData();
        _networkSimpleData.DataChanged += lever_EventHandler;
    }

    void lever_EventHandler(object sender, DataChangedEventArgs e)
    {
        Debug.Log("From lever key for data" + e.Key + " fired at " + e.EventFired);
        
        if(e.Key == "toggle"){
            onState = (bool)_networkSimpleData.GetData(e.Key); 
            if (enumerator != null) StopCoroutine(enumerator);
            StartCoroutine(enumerator = Switch());
        }
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

        _networkSimpleData.SendData("toggle", onState);
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