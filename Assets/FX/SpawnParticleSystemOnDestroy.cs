using UnityEngine;

public class SpawnParticleSystemOnDestroy : MonoBehaviour
{
    public GameObject Particles;

    private void Start()
    {
        if (!Particles)
            Destroy(this);
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying || !Particles) return;
        var particle = Instantiate(Particles, transform.position, Quaternion.identity);
    }
}
