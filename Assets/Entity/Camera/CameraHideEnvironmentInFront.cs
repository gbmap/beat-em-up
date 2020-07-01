﻿using UnityEngine;

public class CameraHideEnvironmentInFront : MonoBehaviour
{
    public Transform Target;

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 tp = Target.transform.position;
        FireRay(tp);

        tp = Target.transform.position + transform.right * 5f;
        FireRay(tp);

        tp = Target.transform.position + -transform.right * 5f;
        FireRay(tp);
    }

    private void FireRay(Vector3 tp)
    {
        Ray r = new Ray();
        r.origin = transform.position;
        r.direction = (tp - transform.position).normalized;

        RaycastHit[] hits =
        Physics.RaycastAll(r,
                           Vector3.Distance(tp, transform.position),
                           LayerMask.GetMask("Level"));

        foreach (RaycastHit rh in hits)
        {
            var dissolves = rh.collider.GetComponentsInChildren<EnvironmentDissolveEffect>();
            foreach (var d in dissolves)
            {
                d.Target = 1f;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || Target == null) return;

        Gizmos.color = Color.red;

        var delta = (Target.transform.position - transform.position).normalized;
        float dist = Vector3.Distance(Target.transform.position, transform.position);
        Gizmos.DrawLine(transform.position, transform.position + delta * dist);
    }
}
