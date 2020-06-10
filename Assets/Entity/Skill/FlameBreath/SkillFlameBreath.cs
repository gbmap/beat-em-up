using UnityEngine;

public class SkillFlameBreath : SkillData
{
    public ParticleSystem ParticleSystem;
    public ParticleSystemRenderer ParticleSparksRenderer;
    public float SparksBoundsFactor = 1.5f;

    public BoxCollider Collider;

    private float lastAttack;
    private float attackCooldown = 0.1f;

    private void Start()
    {
        Caster = GetComponentInParent<CharacterData>();
    }

    private void Update()
    {
        if (Time.time < lastAttack + attackCooldown) return;
        if (ParticleSystem.particleCount == 0 && !Collider.enabled) return;
         
        CharacterAttackData ad = new CharacterAttackData
        {
            Damage = 3,
            Attacker = Caster.gameObject,
            AttackerStats = Caster.Stats
        };
        CombatManager.Attack(ref ad, ParticleSparksRenderer.bounds.center, ParticleSparksRenderer.bounds.extents * SparksBoundsFactor, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(ParticleSparksRenderer.bounds.center, ParticleSparksRenderer.bounds.extents * SparksBoundsFactor);
    }
}
