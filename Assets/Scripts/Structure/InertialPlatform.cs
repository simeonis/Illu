using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertialPlatform : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log(collision.gameObject.name + " got on a rotating bridge.");
            collision.gameObject.GetComponent<PlayerController>().transform.SetParent(transform);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log(collision.gameObject.name + " got off a rotating bridge.");
            collision.gameObject.GetComponent<PlayerController>().transform.SetParent(null);
        }
    }
    
}
