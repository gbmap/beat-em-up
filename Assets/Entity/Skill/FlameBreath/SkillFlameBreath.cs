using UnityEngine;

public class SkillFlameBreath : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public ParticleSystemRenderer ParticleSparksRenderer;
    public float SparksBoundsFactor = 1.5f;

    public BoxCollider Collider;

    private CharacterData data;

    private float lastAttack;
    private float attackCooldown = 0.1f;

    private void Start()
    {
        Transform root = transform;
        while (root.parent != null)
        {
            root = root.parent;
        }

        data = root.GetComponent<CharacterData>();
    }

    private void Update()
    {
        if (Time.time < lastAttack + attackCooldown) return;
        if (ParticleSystem.particleCount == 0 && !Collider.enabled) return;
         
        CharacterAttackData ad = new CharacterAttackData
        {
            Damage = 3,
            Attacker = data.gameObject,
            AttackerStats = data.Stats
        };
        CombatManager.Attack(ref ad, ParticleSparksRenderer.bounds.center, ParticleSparksRenderer.bounds.extents * SparksBoundsFactor, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(ParticleSparksRenderer.bounds.center, ParticleSparksRenderer.bounds.extents * SparksBoundsFactor);
    }
}
