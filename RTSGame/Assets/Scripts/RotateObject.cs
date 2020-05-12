using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Tooltip("The transform to rotate")] public Transform RotationTransform = null;
    [Space]
    [Tooltip("Whether to rotate around the X-Axis")] public bool RotateX = false;
    [Tooltip("Whether to rotate around the Y-Axis")] public bool RotateY = false;
    [Tooltip("Whether to rotate around the Z-Axis")] public bool RotateZ = false;
    [Space]
    [Tooltip("How fast to rotate about the X-Axis")] public float XSpeed = 1;
    [Tooltip("How fast to rotate about the Y-Axis")] public float YSpeed = 1;
    [Tooltip("How fast to rotate about the Z-Axis")] public float ZSpeed = 1;
    [Space]
    [Tooltip("Overrider for rotating")] public bool Rotate = true;

    Vector3 currentRotation = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (RotationTransform != null && Rotate)
        {
            // 'Rotate'
            currentRotation = (new Vector3((RotateX ? 1 : 0) * XSpeed, (RotateY ? 1 : 0) * YSpeed, (RotateZ ? 1 : 0) * ZSpeed) * Time.deltaTime);
            RotationTransform.Rotate(currentRotation);
        }
    }
}
