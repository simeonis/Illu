using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    [SerializeField] private Transform respawnLocation;
    private float offset;

    void Start()
    {
        offset = GetComponent<BoxCollider>().size.x;
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            Debug.Log(collider.gameObject.name + " fell out of the world.");

            Vector3 position = collider.transform.position;
            collider.transform.position = position + collider.transform.up * (offset);
            
            // // Reset position
            // collider.transform.position = respawnLocation.position;

            // // Reset rotation
            // collider.gameObject.GetComponent<PlayerController>().ResetLookDirection();
            // collider.transform.rotation = respawnLocation.rotation;

            // // Reset velocity
            // collider.gameObject.GetComponent<Rigidbody>().velocity = new Vector3();
        }
        else if (collider.tag == "Equipment")
        {
            Debug.Log(collider.gameObject.name + " was destroyed.");
            Destroy(collider.gameObject);
        }
    }
}
