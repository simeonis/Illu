using UnityEngine;

public class Portal : MonoBehaviour
{
    // Target Portal
    public Portal targetPortal;

    [HideInInspector]
    public bool canTeleport = true;

    // Portal
    [HideInInspector]
    public new Camera camera;
    [HideInInspector]
    public new Transform collider;
    [HideInInspector]
    public PortalRender render;
    [HideInInspector]
    public new Renderer renderer;

    void Awake()
    {
        camera = GetComponentInChildren<Camera>();

        // Teleporting components
        collider = transform.Find("Collider");

        // Rendering components
        Transform renderObject = transform.Find("Render");
        render = renderObject.gameObject.GetComponent<PortalRender>();
        renderer = renderObject.gameObject.GetComponent<Renderer>();
    }
}
