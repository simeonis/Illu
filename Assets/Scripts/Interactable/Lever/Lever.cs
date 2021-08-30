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
        _networkSimpleData = GetComponent<NetworkSimpleData>();
        _networkSimpleData.RegisterData("toggle", onState);
        _networkSimpleData.DataChanged += LeverEventHandler;
    }

    public override void OnStartAuthority()
    {
        _networkSimpleData.SendData("toggle", onState);
    }

    void LeverEventHandler(object sender, DataChangedEventArgs e)
    {
        Debug.Log("From lever key for data" + e.Key + " fired at " + e.EventFired);
        
        if (e.Key == "toggle")
        {
            object data = _networkSimpleData.GetData(e.Key);
            onState = (bool)data;
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