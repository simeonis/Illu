using System.Collections;
using UnityEngine;
using Cinemachine;

namespace Illu.Utility {
    [System.Serializable] 
    public struct LayerCullDistances {
        public LayerMask layer;
        public float distance;
    }

    public class CameraUtility : MonoBehaviour
    {
        [Header("Overrides")]
        [SerializeField] private LayerCullDistances[] layerCullDistances;

        private static CameraUtility _instance;
        public static CameraUtility singleton { get { return _instance; } }
        private Canvas canvas;
        public new Camera camera;

        void Awake()
        {
            // Singleton
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else {
                _instance = this;
                canvas = FindObjectOfType<Canvas>();
            }
        }

        public static bool VisibleFromCamera(MeshRenderer renderer, Camera camera) 
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }

        public void UpdateCanvasCamera()
        {
            FindSceneCamera();
            if (camera)
            {
                canvas.worldCamera = camera;
                CullCamera();
            }
        }

        public void FindSceneCamera()
        {
            camera = GameObject.FindObjectOfType<Camera>();
        }

        private void CullCamera()
        {
            float[] distances = new float[32];

            // Set override far clip plane for corresponding layers
            for(int i=0; i<layerCullDistances.Length; i++)
            {
                LayerCullDistances lcd = layerCullDistances[i];
                int index = (int) Mathf.Log(lcd.layer.value, 2);
                distances[index] = lcd.distance;
            }

            camera.layerCullDistances = distances;
        }

        public void ShakeCamera(float duration, float magnitude)
        {
            if (camera)
            {
                StartCoroutine(Shake(duration, magnitude));
            }
        }

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

        private GameObject lockedVirtualCamera;
        public void LockCinemachine(bool state)
        {
            if (camera.TryGetComponent<CinemachineBrain>(out var brain))
            {
                if (!lockedVirtualCamera)
                    lockedVirtualCamera = brain.ActiveVirtualCamera.VirtualCameraGameObject;
                lockedVirtualCamera.SetActive(!state);
            }

            // Camera[] cameras = Camera.allCameras;
            // if (cameras.Length > 0)
            // {
            //     foreach (Camera camera in cameras)
            //     {
            //         if (camera.TryGetComponent<CinemachineBrain>(out var brain))
            //         {
            //             if (!lockedVirtualCamera)
            //                 lockedVirtualCamera = brain.ActiveVirtualCamera.VirtualCameraGameObject;
            //             lockedVirtualCamera.SetActive(!state);
            //             break;
            //         }
            //     }
            // }
        }
    }
}
