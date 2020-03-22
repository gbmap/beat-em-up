using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public Vector3 Rotation;
    public float Speed = 5f;
    public float SpeedFactor = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Rotation * Speed * SpeedFactor * Time.deltaTime, Space.Self);
    }
}
