using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnparentParticleSystemOnDeath : MonoBehaviour
{
    public ParticleSystem[] Systems;


    // não funciona ):
    /*private void OnDestroy()
    {
        Detach();
    }*/

    public void Detach()
    {
        System.Array.ForEach(Systems, s =>
        {
            s.transform.parent = null;
            var m = s.main;
            m.stopAction = ParticleSystemStopAction.Destroy;
        });
    }
}
