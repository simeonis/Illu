using System.Collections;
using UnityEngine;

public class LeverBridgeRotate : Lever
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private float rotationSpeed = 0.1f;
    private IEnumerator rotateCoroutine;

    protected override void Left()
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = RotateTowards(0f);
        StartCoroutine(rotateCoroutine);
    }

    protected override void Up()
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = RotateTowards(90f);
        StartCoroutine(rotateCoroutine);
    }

    protected override void Right()
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = RotateTowards(180f);
        StartCoroutine(rotateCoroutine);
    }

    protected override void Down()
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = RotateTowards(270f);
        StartCoroutine(rotateCoroutine);
    }

    private IEnumerator RotateTowards(float targetAngle)
    {
        Vector3 currentAngles = targetObject.localRotation.eulerAngles;
        
        // Shortest Rotation Path
        if (Mathf.Abs(currentAngles.z - targetAngle) > 180f)
        {
            targetAngle += targetAngle > currentAngles.z ? -360f : 360f;
        }

        float percent = 0.0f;
        while (percent < 1.0f)
        {
            percent += Time.deltaTime * rotationSpeed;

            targetObject.localRotation = Quaternion.Euler(currentAngles.x, currentAngles.y, Mathf.Lerp(currentAngles.z, targetAngle, percent));
            yield return null;
        }
    }
}
