using UnityEngine;

public class SpawnParticleSystemOnDestroy : MonoBehaviour
{
    public GameObject Particles;

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        var particle = Instantiate(Particles, transform.position, Quaternion.identity);
    }
}
