using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Illu.Utility;

public class Gyroscope : MonoBehaviour
{
    [Header("Rings")]
    [SerializeField] private Transform outerRing;
    [SerializeField] private Transform centerRing;
    [SerializeField] private Transform innerRing;

    [Header("Ring Periods (in seconds)")]
    [SerializeField, Tooltip("(Max, Min)")] private Vector2 outerPeriodRange;
    [SerializeField, Tooltip("(Max, Min)")] private Vector2 centerPeriodRange;
    [SerializeField, Tooltip("(Max, Min)")] private Vector2 innerPeriodRange;
    private float outerPeriod, centerPeriod, innerPeriod;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffect orb;
    [SerializeField] private VisualEffect laser;
    [SerializeField] private new Light light;
    [SerializeField] private float laserDuration;
    [SerializeField] private float attractionDuration;
    [SerializeField] private float consumptionDuration;
    [SerializeField, Range(1, 10)] private int consumptionCapacity;

    private enum GyroMode { Off, PhaseOne, PhaseTwo, PhaseThree, Charging, Fire }
    private enum PowerLevel { Empty, Charging, Full };
    
    private GyroMode gyroMode = GyroMode.Off;
    private PowerLevel powerLevel = PowerLevel.Empty;
    
    private int energyLevel = 0;
    private float percent = 0f, elapsed = 0f;

    private bool outerInLockRange, outerRingLocked;
    private bool centerInLockRange, centerRingLocked;
    private bool innerInLockRange, innerRingLocked;

    private IEnumerator consumeCoroutine;
    private Transform currentEnergyOrb;

    void Start()
    {
        // Default ring speeds
        outerPeriod = outerPeriodRange.x;
        centerPeriod = centerPeriodRange.x;
        innerPeriod = innerPeriodRange.x;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Energy")
        {
            // Prevent double-trigger
            collider.tag = "Untagged";

            // Disable rigidbody
            collider.GetComponent<Rigidbody>().isKinematic = true;

            // Pull the energy orb to the gyro's center
            StartCoroutine(Attract(collider.transform, attractionDuration));
        }
    }

    IEnumerator Attract(Transform targetOrb, float duration)
    {
        Vector3 currentPosition = targetOrb.position;
        Vector3 targetPosition = transform.position;

        float percent = 0.0f;
        while(percent < 1.0f)
        {
            percent += Time.deltaTime / duration;
            targetOrb.position = Vector3.Lerp(currentPosition, targetPosition, percent);
            yield return null;
        }

        if (consumeCoroutine != null)
        {
            StopCoroutine(consumeCoroutine);
            Destroy(currentEnergyOrb.gameObject, 0.1f);
        }
        consumeCoroutine = Consume(targetOrb, 5f);
        StartCoroutine(consumeCoroutine);
    }

    IEnumerator Consume(Transform targetOrb, float duration)
    {
        // Increase energy level
        energyLevel++;
        currentEnergyOrb = targetOrb;

        // Energy orb
        Vector3 currentTargetScale = targetOrb.localScale;
        
        // Gyro's orb
        Vector3 currentScale = orb.transform.localScale;
        // Mininum size = 1, Maximum size = 5
        // Scale between 1-5 based on currentEnergyLevel / consumptionCapacity
        float scale = Mathf.Lerp(1f, 5f, energyLevel / consumptionCapacity);
        powerLevel = (scale == 5f) ? PowerLevel.Full : PowerLevel.Charging;
        Vector3 targetScale = new Vector3(scale, scale, scale);
        
        // Make sure gyro orb is enabled
        orb.Play();

        float percent = 0.0f;
        while(percent < 1.0f)
        {
            percent += Time.deltaTime / duration;

            // Energy Orb (vanishes)
            targetOrb.localScale = Vector3.Lerp(currentTargetScale, new Vector3(), percent);
            // Gyro's orb (grows)
            orb.transform.localScale = Vector3.Lerp(currentScale, targetScale, percent);
            // Gyro's orb's light (brighter)
            light.range = Mathf.Lerp(6f, 15f, orb.transform.localScale.x / 5f);
            yield return null;
        }

        // Energy Orb successfully consumed
        Destroy(currentEnergyOrb.gameObject, 0.1f);
    }

    private void PowerOff()
    {
        // Reset energy levels
        powerLevel = PowerLevel.Empty;
        gyroMode = GyroMode.Off;
        energyLevel = 0;

        // Reset ring variables
        outerPeriod = outerPeriodRange.x;
        centerPeriod = centerPeriodRange.x;
        innerPeriod = innerPeriodRange.x;

        outerRingLocked = false;
        centerRingLocked = false;
        innerRingLocked = false;

        percent = elapsed = 0f;

        // Disable laser
        laser.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        // Battery fully charged and not currently firing
        if (powerLevel == PowerLevel.Full && gyroMode != GyroMode.Fire)
        {
            elapsed += Time.deltaTime / 5f;

            // Increase GyroMode every 5 seconds
            if (elapsed >= 1.0f)
            {
                gyroMode++;
                percent = elapsed = 0f;
            }
        }

        switch (gyroMode)
        {
            case GyroMode.PhaseOne:
                // Accelerate rotation speed from standstill
                if (percent < 1.0f) 
                {
                    percent += Time.deltaTime / 5f;
                    outerPeriod = Mathf.Lerp(outerPeriodRange.x * 4f, outerPeriodRange.x, percent);
                }

                outerRing.Rotate(Vector3.up * 360f * Time.deltaTime / outerPeriod, Space.World);
                break;
            case GyroMode.PhaseTwo:
                // Accelerate rotation speed from standstill
                if (percent < 1.0f) 
                {
                    percent += Time.deltaTime / 5f;
                    outerPeriod = Mathf.Lerp(outerPeriodRange.x, outerPeriodRange.y, percent);
                    centerPeriod = Mathf.Lerp(centerPeriodRange.x * 4f, centerPeriodRange.x, percent);
                }

                outerRing.Rotate(Vector3.up * 360f * Time.deltaTime / outerPeriod, Space.World);
                centerRing.Rotate((Vector3.forward + Vector3.left) * 360f * Time.deltaTime / centerPeriod, Space.World);
                break;
            case GyroMode.PhaseThree:
                // Accelerate rotation speed from standstill
                if (percent < 1.0f) 
                {
                    percent += Time.deltaTime / 5f;
                    centerPeriod = Mathf.Lerp(centerPeriodRange.x, centerPeriodRange.y, percent);
                    innerPeriod = Mathf.Lerp(innerPeriodRange.x * 4f, innerPeriodRange.x, percent);
                }

                outerRing.Rotate(Vector3.up * 360f * Time.deltaTime / outerPeriod, Space.World);
                centerRing.Rotate((Vector3.forward + Vector3.left) * 360f * Time.deltaTime / centerPeriod, Space.World);
                innerRing.Rotate((Vector3.up + Vector3.right) * 360f * Time.deltaTime / innerPeriod, Space.World);
                break;
            case GyroMode.Charging:
                // Accelerate rotation speed
                if (percent < 1.0f) 
                {
                    percent += Time.deltaTime / 5f;
                    innerPeriod = Mathf.Lerp(innerPeriodRange.x, innerPeriodRange.y, percent);
                }

                outerRing.Rotate(Vector3.up * 360f * Time.deltaTime / outerPeriod, Space.World);
                centerRing.Rotate((Vector3.forward + Vector3.left) * 360f * Time.deltaTime / centerPeriod, Space.World);
                innerRing.Rotate((Vector3.up + Vector3.right) * 360f * Time.deltaTime / innerPeriod, Space.World);
                break;
            case GyroMode.Fire:
                if (percent < 1.0f) percent += Time.deltaTime / 0.25f;

                // Check if ring can be rotated
                if (!outerRingLocked)
                {
                    // Rotate ring
                    outerRing.Rotate(Vector3.up * 360f * Time.deltaTime / outerPeriod, Space.World);
                    // Check if ring is within locking range
                    outerInLockRange = Vector3.Angle(outerRing.forward, transform.forward) <= 4f / outerPeriod;
                    // Lock ring
                    if (outerInLockRange)
                    {
                        outerRing.rotation = default(Quaternion);
                        outerRingLocked = true;
                        percent = 0f;
                    }
                }

                // Check if ring can be rotated
                if (!centerRingLocked)
                {
                    // Rotate ring
                    centerRing.Rotate((Vector3.forward + Vector3.left) * 360f * Time.deltaTime / centerPeriod, Space.World);
                    // Check if ring is within locking range
                    centerInLockRange = Vector3.Angle(centerRing.forward, transform.forward) <= 4f / centerPeriod;
                    // Lock ring
                    if (outerRingLocked && centerInLockRange && percent >= 1f)
                    {
                        centerRing.rotation = default(Quaternion);
                        centerRingLocked = true;
                        percent = 0f;
                    }
                }

                // Check if ring can be rotated
                if (!innerRingLocked)
                {
                    // Rotate ring
                    innerRing.Rotate((Vector3.up + Vector3.right) * 360f * Time.deltaTime / innerPeriod, Space.World);
                    // Check if ring is within locking range
                    innerInLockRange = Vector3.Angle(innerRing.forward, transform.forward) <= 4f / innerPeriod;
                    // Lock ring
                    if (outerRingLocked && centerRingLocked && innerInLockRange && percent >= 1f)
                    {
                        innerRing.rotation = default(Quaternion);
                        innerRingLocked = true;
                        percent = 0f;

                        laser.Play();
                        CameraUtility.singleton.ShakeCamera(laserDuration, 0.25f);
                        Invoke("PowerOff", laserDuration);
                    }
                }
                break;
            default:
                break;
        }
    }
}