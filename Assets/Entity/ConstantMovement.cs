using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMovement : MonoBehaviour
{
    public float Speed = 5f;
    public float SpeedFactor = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * Speed * SpeedFactor * Time.deltaTime;
    }
}
