using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX : MonoBehaviour
{
    public ParticleSystem ParticleImpactHit;

    public void FxImpactHit(Vector3 position)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = position,
        };
        ParticleImpactHit.Emit(emitParams, 1);
    }
}
