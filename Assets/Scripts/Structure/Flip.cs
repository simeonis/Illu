using System.Collections;
using UnityEngine;

public class Flip : Trigger
{
    [Header("Functionality")]
    [SerializeField] private Transform rotationPivot;
    [SerializeField] private float flipDuration = 0.25f;
    [SerializeField] private bool reverseFlip = false;
    private float rotationAmount = 180f;

    private void Start()
    {
        rotationAmount *= reverseFlip ? -1 : 1;
    }

    public override void Activate(IlluInteractable.Button button)
    {
        StartCoroutine(FlipOverTime(button));
    }

    private IEnumerator FlipOverTime(IlluInteractable.Button button = null)
    {
        rotationAmount *= -1;
        Quaternion currentRotation = rotationPivot.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(rotationAmount, 0f, 0f);

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime / flipDuration;
            rotationPivot.localRotation = Quaternion.Lerp(currentRotation, targetRotation, percent);
            yield return null;
        }

        if (button) button.Reset();
    }
}
