using System.Collections;
using UnityEngine;

public class Flip : Trigger
{
    [Header("Functionality")]
    [SerializeField] private Transform rotationPivot;
    [SerializeField] private float flipDuration = 0.25f;

    public override void Activate(Illu_Interactable.Button button)
    {
        StartCoroutine(FlipOverTime(button));
    }

    private IEnumerator FlipOverTime(Illu_Interactable.Button button)
    {
        Quaternion currentRotation = rotationPivot.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(180f, 0f, 0f);

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / flipDuration;
            rotationPivot.localRotation = Quaternion.Lerp(currentRotation, targetRotation, percent);
            yield return null;
        }

        button.Reset();
    }
}
