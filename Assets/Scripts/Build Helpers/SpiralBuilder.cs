#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class SpiralBuilder : MonoBehaviour
{
    [Header("GameObject Settings")]
    [SerializeField] GameObject _sourceObject;
    [SerializeField] bool _isStatic = false;

    [Header("Spiral Settings")]
    [SerializeField] int _numberOfSteps = 1;
    [SerializeField] float _angle = 45f;
    [SerializeField] float _gap = 1.5f;
    [SerializeField] float _verticalOffset = 0f;

    [Header("Visualization Settings")]
    [SerializeField] Color _color = Color.red;
    [HideInInspector] public bool Visualize = true;

    public void Build()
    {
        if (_sourceObject == null) return;

        GameObject container = new GameObject($"Spiral ({_numberOfSteps} Objects)");
        container.transform.position = transform.position;
        container.transform.rotation = transform.rotation;
        container.isStatic = _isStatic;

        for (int i=0; i<_numberOfSteps; i++)
        {
            float angle = i * 2 * Mathf.PI / _angle;
            float x = Mathf.Cos(angle) * _gap;
            float y = Mathf.Sin(angle) * _gap;
            
            GameObject gameObject = GameObject.Instantiate(_sourceObject, Vector3.zero, Quaternion.identity);

            // Object Properties
            gameObject.name = "Object #" + (i + 1);
            gameObject.transform.SetParent(container.transform, false);
            gameObject.transform.localPosition = new Vector3(i * x, i * _verticalOffset, i * y);
            gameObject.isStatic = _isStatic;
        }
    }

    void OnDrawGizmos()
    {
        if (Visualize && _sourceObject != null)
        {
            Gizmos.color = _color;
            for (int i=0; i<_numberOfSteps; i++)
            {
                float angle = i * 2 * Mathf.PI / _angle;
                float x = Mathf.Cos(angle) * _gap;
                float y = Mathf.Sin(angle) * _gap;

                Gizmos.matrix = transform.localToWorldMatrix;
                if (_sourceObject.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    Gizmos.DrawMesh(meshFilter.sharedMesh, -1, new Vector3(i * x, i * _verticalOffset, i * y));
                }
                else
                {
                    Gizmos.DrawSphere(new Vector3(i * x, i * _verticalOffset, i * y), 1f);
                }
            }
        }
    }
}
#endif