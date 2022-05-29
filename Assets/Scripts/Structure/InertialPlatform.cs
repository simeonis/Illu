using UnityEngine;

public abstract class InertialPlatform : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && isOntop(-collision.GetContact(0).normal))
        {
            collision.transform.SetParent(transform);
            PlayerEnter();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(null);
            PlayerExit();
        }
    }

    protected virtual void PlayerEnter() {}
    protected virtual void PlayerExit() {}

    bool isOntop(Vector3 collisionNormal)
    {
        return Vector3.Dot(collisionNormal, transform.up) > 0f;
    }
}
