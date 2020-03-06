using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[CustomEditor(typeof(DebugParticle))]
public class DebugParticleEditor : Editor
{
    

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Emit"))
        {
            DebugParticle dp = (target as DebugParticle);
            var ps = dp.gameObject.GetComponent<ParticleSystem>();

            //dp.Emit(ps, dp.position, dp.shapeRotation);
            EmitSmokeRadius(ps);

            //dp.Sphere(ps, ps.transform.position, dp.Emit);
        }
    }

    private void EmitSmokeRadius(ParticleSystem ps)
    {
        int range = UnityEngine.Random.Range(15, 20);
        for (int i = 0; i < range; i++)
        {
            Vector3 vel = UnityEngine.Random.insideUnitSphere;
            vel.y = 0f;
            vel.Normalize();
            vel *= 7f;
            ps.Emit(new ParticleSystem.EmitParams
            {
                startSize = UnityEngine.Random.Range(2, 4),
                velocity = vel
            }, 1);
        }
    }

}
