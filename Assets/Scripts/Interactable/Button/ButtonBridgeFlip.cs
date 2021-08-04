using System.Collections;
using UnityEngine;

public class ButtonBridgeFlip : Interactable
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private float rotationSpeed = 0.1f;
    private IEnumerator rotateCoroutine;

    public override void Interaction(Interactor interactor)
    {
        Debug.Log("Pressed!");

        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = Flip();
        StartCoroutine(rotateCoroutine);
    }

    public override void InteractionCancelled(Interactor interactor){}

    private IEnumerator Flip()
    {
        Vector3 currentAngles = targetObject.localRotation.eulerAngles;
        float targetAngle = currentAngles.x == 180f ? 0f : 180f;

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime * rotationSpeed;

            targetObject.localRotation = Quaternion.Euler(Mathf.Lerp(currentAngles.x, targetAngle, percent), currentAngles.y, currentAngles.z);
            yield return null;
        }
    }
}
