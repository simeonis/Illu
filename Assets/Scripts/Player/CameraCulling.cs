using UnityEngine;

public class CameraCulling : MonoBehaviour
{
    [System.Serializable] public struct LayerCullDistances {
        public string name;
        public int layerNumber;
        public float distance;
    }
    
    [Header("Overrides")]
    [SerializeField] private LayerCullDistances[] layerCullDistances;

    void Start()
    {
        Camera camera = GetComponent<Camera>();
        float[] distances = new float[32];

        // Set override far clip plane for corresponding layers
        for(int i=0; i<layerCullDistances.Length; i++)
        {
            LayerCullDistances lcd = layerCullDistances[i];
            distances[lcd.layerNumber] = lcd.distance;
        }

        camera.layerCullDistances = distances;
    }
}
