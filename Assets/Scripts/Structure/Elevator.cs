using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private float height = 3.0f;
    [SerializeField] private float speed = 1.0f;
    private bool isRising = false;
    private Vector3 initialPos;
    private Vector3 targetPos;

    void Start()
    {
        initialPos = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log(collision.gameObject.name + " got on a rotating bridge.");
            collision.transform.SetParent(transform);
            isRising = true;
        }
    }

    void FixedUpdate()
    {
        targetPos = isRising ? new Vector3(initialPos.x, initialPos.y + height, initialPos.z) : initialPos;
        if (transform.position != targetPos)
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log(collision.gameObject.name + " got off a rotating bridge.");
            collision.transform.SetParent(null);
            isRising = false;
        }
    }
}
