using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToFloor : MonoBehaviour
{
    void Update()
    {
        /*// manter hud no chão
        RaycastHit hitInfo;
        Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hitInfo);
        transform.position = hitInfo.point + Vector3.up * 0.001f; // previnir flickering    */
        if (transform.parent == null) return;
        transform.localRotation = Quaternion.Euler(90f, -transform.parent.rotation.y, 0f);

    }
}
