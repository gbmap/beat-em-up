using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticleSystemOnDestroy : MonoBehaviour
{
    public GameObject Particles;

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        Instantiate(Particles, transform.position, Quaternion.identity);
    }
}
