using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairBuilder : MonoBehaviour
{
    public int numberOfSteps;
    public Vector2 offset = new Vector2(0.5f, 0.5f);

    void Start()
    {
        for (int i=0; i<numberOfSteps; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Properties
            cube.name = "Step " + (i + 1);
            cube.transform.parent = transform;
            cube.layer = transform.gameObject.layer;

            // Position, Rotation and Scale
            cube.transform.position = transform.position + transform.forward * (i * offset.x) + transform.up * (i * offset.y);
            cube.transform.rotation = transform.rotation;
            cube.transform.localScale = Vector3.one;
        }
    }
}
