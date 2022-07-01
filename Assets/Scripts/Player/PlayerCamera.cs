using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [Header("Overrides")]
    [SerializeField] private float[] layerCullDistances = new float[32];

    private new Camera camera;
    
    void Start()
    {
        camera = GetComponent<Camera>();
        // TO-DO: Create custom inspector
        // 1. Fixed array of size 32
        // 2. Each index is named after corresponding layer
        // 3. Each index value is defaulted to 0
        // camera.layerCullDistances = layerCullDistances;
    }

    private GameObject lockedVirtualCamera;
    public void LockCinemachine(bool state)
    {
        if (camera && camera.TryGetComponent<CinemachineBrain>(out var brain))
        {
            if (!lockedVirtualCamera)
                lockedVirtualCamera = brain.ActiveVirtualCamera.VirtualCameraGameObject;
            lockedVirtualCamera.SetActive(!state);
        }
    }

    public bool VisibleFromCamera(MeshRenderer renderer) 
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (cameraShakeCoroutine != null) StopCoroutine(cameraShakeCoroutine);
        StartCoroutine(cameraShakeCoroutine = Shake(duration, magnitude));
    }

    private IEnumerator cameraShakeCoroutine;
    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = camera.transform.localPosition;
        float elapsed = 0.0f;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camera.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        camera.transform.localPosition = originalPos;
    }
}
