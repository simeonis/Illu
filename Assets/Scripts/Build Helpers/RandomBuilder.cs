#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RandomBuilder : MonoBehaviour
{
    // Generation
    public string[] Options { get; set; } = new string[] {"Cube", "Sphere", "Cylinder", "Custom"};
    public int Index { get; set; } = 0;
    public bool IsStatic { get; set; } = false;
    public GameObject CustomObject { get; set; }
    public Material CustomMaterial { get; set; }
    public int Amount { get; set; } = 5;
    public Vector3 BoundSize { get; set; } = new Vector3(5f, 5f, 5f);
    public bool CenterPivot { get; set; } = true;
    public bool UniformScaling { get; set; }  = true;
    [HideInInspector] public float MinWidth = 1f;
    [HideInInspector] public float MaxWidth = 1f;
    [HideInInspector] public float MinHeight = 1f;
    [HideInInspector] public float MaxHeight = 1f;

    // Visualization
    public Color BoundColor { get; set; } = new Color(179f/255f, 63f/255f, 64f/255f, 1f);
    public Color PointColor  { get; set; } = new Color(101f/255f, 119f/255f, 179f/255f, 1f);
    public bool Visualize { get; set; } = true;

    List<Vector3> _points = new List<Vector3>();
    int _counter = 0;

    public void Randomize()
    {
        _points.Clear();
        for (int i=0; i<Amount; i++)
            _points.Add(RandomPointInBox(transform.position, BoundSize));
    }

    public void Build()
    {
        if (_points.Count == 0 || _points.Count != Amount)
            Randomize();

        switch(Index)
        {
            // Cube
            case 0:
                BuildCube();
                break;
            // Sphere
            case 1:
                BuildSphere();
                break;
            // Cylinder
            case 2:
                BuildCylinder();
                break;
            // Custom
            default:
                if (CustomObject != null)
                    BuildMesh();
                else
                    Debug.LogError("Missing custom GameObject!");
                break;
        }
    }

    void BuildCube()
    {
        Transform parent = BuildParent(Options[0]).transform;
        for (int i=0; i<_points.Count; i++)
            BuildPrimitive(PrimitiveType.Cube, _points[i], parent);
        _counter = 0;
    }

    void BuildSphere()
    {
        Transform parent = BuildParent(Options[1]).transform;
        for (int i=0; i<_points.Count; i++)
            BuildPrimitive(PrimitiveType.Sphere, _points[i], parent);
        _counter = 0;
    }

    void BuildCylinder()
    {
        Transform parent = BuildParent(Options[2]).transform;
        for (int i=0; i<_points.Count; i++)
            BuildPrimitive(PrimitiveType.Cylinder, _points[i], parent);
        _counter = 0;
    }

    void BuildMesh()
    {
        Transform parent = BuildParent(Options[3]).transform;
        for (int i=0; i<_points.Count; i++)
        {
            GameObject custom = GameObject.Instantiate(CustomObject, _points[i] - transform.position, Quaternion.identity);

            custom.name = $"Object #{i}";
            custom.transform.SetParent(parent.transform, false);
            custom.isStatic = IsStatic;

            // Scale
            float randomWidth = Random.Range(MinWidth, MaxWidth);
            float randomHeight = UniformScaling ? randomWidth : Random.Range(MinHeight, MaxHeight);
            custom.transform.localScale = new Vector3(randomWidth, randomHeight, randomWidth);
        }
    }

    GameObject BuildParent(string type)
    {
        GameObject parent = new GameObject($"Randomized {_points.Count} {type}{(_points.Count > 1 ? "s" : "")}");
        parent.transform.position = transform.position;
        parent.transform.rotation = transform.rotation;
        parent.isStatic = IsStatic;
        return parent;
    }

    void BuildPrimitive(PrimitiveType primitiveType, Vector3 position, Transform parent = null)
    {
        _counter++;
        bool isCylinder = (primitiveType == PrimitiveType.Cylinder);

        GameObject primitive = GameObject.CreatePrimitive(primitiveType);
        
        primitive.name = $"{primitiveType.ToString()} #{_counter}";
        primitive.transform.SetParent(parent.transform, false);
        primitive.isStatic = IsStatic;
            
        // Material
        if (CustomMaterial)
            primitive.GetComponent<Renderer>().material = CustomMaterial;

        // Collider
        if (isCylinder)
        {
            Collider collider = primitive.GetComponent<Collider>(); 
            DestroyImmediate(collider);
            MeshCollider meshCollider = primitive.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = primitive.GetComponent<MeshFilter>().sharedMesh;
        }

        // Scale
        float randomWidth = Random.Range(MinWidth, MaxWidth);
        float randomHeight = UniformScaling ? randomWidth : Random.Range(MinHeight, MaxHeight);
        primitive.transform.localScale = new Vector3(randomWidth, randomHeight, randomWidth);

        // Position
        if (CenterPivot)
            primitive.transform.position = position;
        else
        {
            Vector3 offset = Vector3.up * (primitive.transform.localScale.y * (isCylinder ? 1f : 0.5f));
            primitive.transform.position = position + offset;
        }
    }

    Vector3 RandomPointInBox(Vector3 center, Vector3 size)
    {
        return center + new Vector3(
            (Random.value - 0.5f) * size.x,
            (Random.value - 0.5f) * size.y,
            (Random.value - 0.5f) * size.z
        );
    }

    void OnDrawGizmos()
    {
        if (Visualize)
        {
            Gizmos.color = BoundColor;
            Gizmos.DrawCube(transform.position, BoundSize);
            Gizmos.DrawWireCube(transform.position, BoundSize);

            Gizmos.color = PointColor;
            for (int i=0; i<_points.Count; i++)
                Gizmos.DrawSphere(_points[i], 0.1f);
        }
    }
}
#endif