using UnityEngine;

public class EmitParticlesOnDamage : MonoBehaviour
{
    public CharacterHealth Health;
    public ParticleSystem[] Particles;

    private void OnEnable()
    {
        Health.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        Health.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(CharacterAttackData obj)
    {
        foreach (var particles in Particles)
        {
            var burst = particles.emission.GetBurst(0);
            particles.Emit(Mathf.RoundToInt(burst.count.Evaluate(Random.value)));
        }
    }
}
