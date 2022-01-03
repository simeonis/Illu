using UnityEngine;
using System.Collections;

public class AudioImpact : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ImpactAudioEvent impactEvent;
    [SerializeField] private float threshhold;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float playRate = 0.03f;

    private bool colliding;
    private Vector3 p_Velocity;
    private float collisionMagnitude;
    private float lastPlayed;

    void OnCollisionEnter(Collision collision)
    {
        colliding = true;
        // Debug.Log("VEL COL" + collision.relativeVelocity.magnitude);
        // if (collision.relativeVelocity.magnitude > 2)
        //     impactEvent.Play(audioSource, collision.relativeVelocity.magnitude);
        collisionMagnitude = collision.relativeVelocity.magnitude;
    }
    void OnCollisionExit(Collision collision)
    {
        colliding = false;
    }

    void FixedUpdate()
    {
        Vector3 c_Velocity = rigidBody.velocity;

        if (colliding && Vector3.Distance(p_Velocity, c_Velocity) > threshhold)
        {
            if (Time.time - lastPlayed >= playRate)
            {
                impactEvent.Play(audioSource, collisionMagnitude);
                lastPlayed = Time.time;
            }
            p_Velocity = c_Velocity;
        }
    }
}
