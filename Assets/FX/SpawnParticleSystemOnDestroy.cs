using UnityEngine;

public class SpawnParticleSystemOnDestroy : MonoBehaviour
{
    public GameObject Particles;

    private bool isQuitting;

    private void Start()
    {
        if (!Particles)
            Destroy(this);

        Application.quitting += ApplicationQuitting;
    }

    private void ApplicationQuitting()
    {
        isQuitting = true;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying || !Particles || isQuitting) return;
        var particle = Instantiate(Particles, transform.position, Quaternion.identity);
    }
}
