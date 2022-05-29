using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; private set; }

    public virtual void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}

