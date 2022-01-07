using System.Collections;
using UnityEngine;

namespace Illu.Utility {
    [System.Serializable] 
    public struct LayerCullDistances {
        public string name;
        public int layerNumber;
        public float distance;
    }

    public class CameraUtility : MonoBehaviour
    {
        [Header("Overrides")]
        [SerializeField] private LayerCullDistances[] layerCullDistances;

        private static CameraUtility _instance;
        public static CameraUtility singleton { get { return _instance; } }
        private Canvas canvas;
        private new Camera camera;

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

        public void FindSceneCamera()
        {
            Camera camera = FindObjectOfType<Camera>();
            if (camera)
            {
                this.camera = camera;

                // Desired outcome (Post-processing for UI)
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                CullCamera(camera);
            }
            else 
            {
                this.camera = null;

                // Undesired outcome (No post-processing for UI)
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        private void CullCamera(Camera camera)
        {
            float[] distances = new float[32];

            // Set override far clip plane for corresponding layers
            for(int i=0; i<layerCullDistances.Length; i++)
            {
                LayerCullDistances lcd = layerCullDistances[i];
                distances[lcd.layerNumber] = lcd.distance;
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
    }
}
