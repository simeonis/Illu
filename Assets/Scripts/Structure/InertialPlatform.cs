using UnityEngine;

public abstract class InertialPlatform : MonoBehaviour
{ 
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            collider.transform.SetParent(transform);
            PlayerEnter();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            collider.transform.SetParent(null);
            PlayerExit();
        }
    }

    protected virtual void PlayerEnter() {}
    protected virtual void PlayerExit() {}
}
