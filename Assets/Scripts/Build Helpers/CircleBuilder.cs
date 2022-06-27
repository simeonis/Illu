#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class CircleBuilder : MonoBehaviour
{
    [Header("GameObject Settings")]
    [SerializeField] GameObject _sourceObject;
    [SerializeField] bool _isStatic = false;

    [Header("Circle Settings")]
    [SerializeField, Range(1f, 1080f)] float _numberOfSteps = 360f;
    [SerializeField] float _radius = 1f;

    [Header("Visualization Settings")]
    [SerializeField] Color _color = Color.red;
    [HideInInspector] public bool Visualize = true;

    public void Build()
    {
        if (_sourceObject == null) return;

        GameObject container = new GameObject($"Circle ({_numberOfSteps} Steps)");
        container.transform.position = transform.position;
        container.transform.rotation = transform.rotation;
        container.isStatic = _isStatic;

        float angle = 0f;
        for (int i=0; i<_numberOfSteps; i++)
        {
            float x = Mathf.Cos(angle) * _radius;
            float y = Mathf.Sin(angle) * _radius;
            angle += 2 * Mathf.PI / _numberOfSteps;
            
            GameObject gameObject = GameObject.Instantiate(_sourceObject, Vector3.zero, Quaternion.identity);

            // Object Properties
            gameObject.name = "Object #" + (i + 1);
            gameObject.transform.SetParent(container.transform, false);
            gameObject.transform.localPosition = new Vector3(x, 0f, y);
            gameObject.isStatic = _isStatic;
        }
    }

    void OnDrawGizmos()
    {
        if (Visualize && _sourceObject != null)
        {
            Gizmos.color = _color;

            float angle = 0f;
            for (int i=0; i<_numberOfSteps; i++)
            {
                float x = Mathf.Cos(angle) * _radius;
                float y = Mathf.Sin(angle) * _radius;
                angle += 2 * Mathf.PI / _numberOfSteps;

                Gizmos.matrix = transform.localToWorldMatrix;
                if (_sourceObject.TryGetComponent<MeshFilter>(out var meshFilter))
                    Gizmos.DrawMesh(meshFilter.sharedMesh, -1, new Vector3(x, 0f, y));
                else
                    Gizmos.DrawSphere(new Vector3(x, 0f, y), 1f);
            }
        }
    }
}
#endif