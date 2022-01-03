using System.Collections;
using UnityEngine;


public class Lever : AnimatedInteractable
{
    [Header("Target Script")]
    [SerializeField] private Trigger target;

    private bool onState = false;
    private IEnumerator enumerator;

    protected override void Awake() { base.Awake(); }

    public override void Interaction(Interactor interactor)
    {
        if (!enabled || !target) return;

        enabled = false;
        onState = !onState;
        if (enumerator != null) StopCoroutine(enumerator);
        StartCoroutine(enumerator = Switch());

        audioEvent[0].Play(audioSource);
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

        enabled = true;
        target.Activate();
    }
}