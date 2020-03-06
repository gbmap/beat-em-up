using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class DebugParticle : MonoBehaviour
{
    public Vector3 position;
    public Vector3 shapeRotation;

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.TransformPoint(position);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, 0.25f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos, pos - shapeRotation.normalized);

        //Sphere(null, transform.position, DebugDraw);
    }

    public void Sphere(ParticleSystem ps, Vector3 initialPos, System.Action<ParticleSystem, Vector3, Vector3> callback)
    {
        for (int i = 0; i < 10; i++)
        {
            float t = ((float)i) / 10;
            t *= 360f;
            //Vector3 pos = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f) * 2f;
            Vector3 pos = UnityEngine.Random.insideUnitSphere * 2f;
            Vector3 rot = (initialPos - pos).normalized;
            callback(ps, pos, rot);
        }
    }

    private void DebugDraw(ParticleSystem ps, Vector3 pos, Vector3 rot)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.1f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos, pos + rot);
    }

    public void Emit(ParticleSystem ps, Vector3 pos, Vector3 dir)
    {
        EmitParams ep = new EmitParams()
        {
            position = pos,
            applyShapeToPosition = true
        };

        var shape = ps.shape;
        Vector3 rotation = new Vector3(shape.rotation.x, shape.rotation.y, shape.rotation.z);
        shape.rotation = Quaternion.LookRotation(pos + dir, Vector3.up).eulerAngles;
        ps.Emit(ep, 1);
        shape.rotation = rotation;
    }

}
