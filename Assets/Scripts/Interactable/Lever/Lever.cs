using System.Collections;
using UnityEngine;


public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;

    private bool onState = false;
    private IEnumerator enumerator;

    protected override void Awake() { base.Awake(); }

    void Start()
    {
        networkSimpleData.RegisterKey("LEVER_PULL");
        networkSimpleData.DataChanged += LeverEventHandler;
    }

    public override void OnStartAuthority()
    {
        networkSimpleData.SendData("LEVER_PULL", onState);
    }

    void LeverEventHandler(object sender, DataChangedEventArgs e)
    {
        if (e.key == "LEVER_PULL")
        {
            bool data = (bool)networkSimpleData.GetData(e.key);
            // Ensures data doesn't match current
            if (onState != data)
            {
                onState = data;
                if (enumerator != null) StopCoroutine(enumerator);
                StartCoroutine(enumerator = Switch());
            }
        }
    }

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || !target) return;

        // Request Authority
        base.Interaction(interactor);

        enabled = false;
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

        enabled = true;
        target.Activate();
    }
}