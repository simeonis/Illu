using UnityEngine;

public class PortalSetup : MonoBehaviour
{
    public void Setup()
    {
        Camera camera = Camera.main;
        Shader shader = Shader.Find("Unlit/PortalCutout");
        Portal[] portals = FindObjectsOfType<Portal>();

        // Texture setup
        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i].camera.targetTexture != null)
            {
                portals[i].camera.targetTexture.Release();
            }
            portals[i].targetPortal.camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 32);
            portals[i].renderer.material = new Material(shader)
            {
                mainTexture = portals[i].targetPortal.camera.targetTexture
            };

            // Player camera setup
            portals[i].render.playerCamera = camera;
        }
    }
}
