#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class StairBuilder : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private Material material;
    [SerializeField] private int numberOfSteps = 1;
    [SerializeField] private Vector2 offset = new Vector2(0.5f, 0.5f);
    [HideInInspector] public bool visualize = true;
    [HideInInspector] public bool fill = false;

    [Header("Visualization")]
    [SerializeField] private Color color = Color.red;

    public void Build()
    {
        GameObject container = new GameObject($"Stairs ({numberOfSteps} Steps)");
        
        // Container Properties
        container.transform.parent = transform.parent;
        container.layer = LayerMask.NameToLayer("Ground");
        
        // Steps
        for (int i=0; i<numberOfSteps; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Cube Properties
            cube.name = "Step #" + (i + 1);
            cube.transform.parent = container.transform;
            cube.layer = LayerMask.NameToLayer("Ground");

            // Position, Rotation and Scale
            cube.transform.position = transform.position + transform.forward * (i * offset.x) + transform.up * (i * offset.y);
            cube.transform.rotation = transform.rotation;
            cube.transform.localScale = transform.localScale;

            // Material
            cube.GetComponent<Renderer>().material = material;
        }
    }

    void OnDrawGizmos()
    {
        if (visualize)
        {
            Gizmos.color = color;
            for (int i=0; i<numberOfSteps; i++)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                if (fill)
                {
                    Gizmos.DrawCube((Vector3.forward * i * offset.x) + (Vector3.up * i * offset.y), Vector3.one);
                }
                else
                {
                    Gizmos.DrawWireCube((Vector3.forward * i * offset.x) + (Vector3.up * i * offset.y), Vector3.one);
                }
            }
        }
    }
}
#endif