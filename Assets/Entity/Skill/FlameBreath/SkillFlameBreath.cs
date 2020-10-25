using UnityEngine;
using Catacumba.Entity;

public class SkillFlameBreath : SkillData
{
    public ParticleSystem ParticleSystem;
    public ParticleSystemRenderer ParticleSparksRenderer;
    public float SparksBoundsFactor = 1.5f;

    public BoxCollider Collider;

    private float lastAttack;
    private float attackCooldown = 0.05f;

    bool lastColliderEnable;
    float colliderEnableTime;

    private void Start()
    {
        if (!Caster) Caster = GetComponentInParent<CharacterData>();
    }

    private void Update()
    {
        if (Collider.enabled)
        {
            if (!lastColliderEnable)
            {
                colliderEnableTime = Time.time;
            }
        }
        lastColliderEnable = Collider.enabled;

        if (Time.time < lastAttack + attackCooldown) return;
        if (ParticleSystem.particleCount == 0 || !Collider.enabled) return;

        CharacterAttackData ad = new CharacterAttackData
        {
            Attacker = Caster.gameObject,
            AttackerStats = Caster.Stats,
            Type = EAttackType.Skill,
            Damage = 5,
        };
        CombatManager.Attack(ref ad, ParticleSparksRenderer.bounds.center, ParticleSparksRenderer.bounds.extents * SparksBoundsFactor, Quaternion.identity);
        lastAttack = Time.time;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(ParticleSparksRenderer.bounds.center, 2f * ParticleSparksRenderer.bounds.extents * SparksBoundsFactor);
    }
}
