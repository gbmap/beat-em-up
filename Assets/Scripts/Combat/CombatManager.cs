using UnityEngine;

public enum EAttackType
{
    Weak = 0,
    Strong,
    Skill
}

public struct CharacterAttackData
{
    public CharacterAttackData(EAttackType type, GameObject attacker, int hitNumber = 0)
    {
        Type = type;
        Attacker = attacker;

        Time = UnityEngine.Time.time;
        AttackerStats = null;
        Defender = null;
        DefenderStats = null;
        Damage = 0;
        HitNumber = hitNumber;
        Poised = false;
        Knockdown = false;
        CancelAnimation = false;
        Dead = false;

        ColliderPos = Vector3.zero;
        ColliderSz = Vector3.zero;
        ColliderRot = Quaternion.identity;
        
    }

    public float Time;
    public EAttackType Type;
    public GameObject Attacker;
    public CharacterStats AttackerStats;
    public GameObject Defender;
    public CharacterStats DefenderStats;
    public int Damage;
    public int HitNumber;
    public bool Dead;

    public bool Poised;
    public bool Knockdown;
    public bool CancelAnimation;

    public Vector3 ColliderPos;
    public Vector3 ColliderSz;
    public Quaternion ColliderRot;
}


public class CombatManager : ConfigurableSingleton<CombatManager, CombatManagerConfig>
{
    protected override string Path => "Data/CombatManagerConfig";

    private static CharacterAttackData lastAttack;

    public static float GetCritFactor(CharacterStats c)
    {
        return 2f;
    }

    public static float GetCritChance(CharacterStats c)
    {
        var attr = c.Inventory.GetTotalAttributes();
        float d = c.Attributes.Dexterity + attr.Dexterity;

        // esse 256 é arbitrário e significa o valor de destreza que equivale a 50% de chance de crit.
        return 1f - (1f / (Mathf.Pow(d/256, 2f)+1f));
        //return Mathf.Pow(d/256, 1f/1.75f);
    }

    public static float GetPoiseChance(CharacterStats c)
    {
        var attr = c.Inventory.GetTotalAttributes();
        float d = c.Attributes.Dexterity + attr.Dexterity;
        return 1f - (1f / (Mathf.Pow(d / 256, 2f) + 1f));
    }

    public static int GetMaxHealth(CharacterStats c)
    {
        return (c.Attributes.Stamina+c.Inventory.GetTotalAttributes().Stamina) * 19;
    }

    public static int GetMaxMana(CharacterStats c)
    {
        return (c.Attributes.Magic+c.Inventory.GetTotalAttributes().Magic) * 19;
    }

    public static int GetDamage(CharacterStats attacker, CharacterStats defender, Vector3 attackerForward, Vector3 defenderForward, EAttackType attackType)
    {
        var dmgScaling = attacker.Inventory.GetTotalDamageScaling();

        float str = (attacker.GetAttributeTotal(EAttribute.Strength)) * (1f + dmgScaling.Strength) * (attackType == EAttackType.Weak ? 1f : 4f);
        float crit = Random.value < GetCritChance(attacker) ? GetCritFactor(attacker) : 1f;
        float backstab = Mathf.Max(0f, Vector3.Dot(attackerForward, defenderForward));

        return Mathf.RoundToInt((str+(str*backstab)) * crit);
    }


    public static void CalculateAttackStats(GameObject attacker, GameObject defender, ref CharacterAttackData attackData)
    {
        CharacterStats attackerStats = attacker.GetComponent<CharacterData>()?.Stats;
        CharacterStats defenderStats = defender.GetComponent<CharacterData>().Stats;

        CalculateAttackStats(
            attackerStats,
            defenderStats,
            ref attackData
        );
    }

    public static void CalculateAttackStats(CharacterStats attacker, CharacterStats defender, ref CharacterAttackData attackData)
    {
        attackData.AttackerStats = attacker;
        attackData.DefenderStats = defender;

        // calcula dano cru
        int damage = 0;
        if (attacker == null || attackData.Type == EAttackType.Skill)
        {
            damage = attackData.Damage;
        }
        else
        {
            damage = GetDamage(attacker, defender, attackData.Attacker.transform.forward, attackData.Defender.transform.forward, attackData.Type);
        }

        // rola o dado pra poise
        attackData.Poised = Random.value < defender.PoiseChance;
        if (attackData.Poised)
        {
            damage = (int)(damage * 0.9f);
        }

        defender.CurrentStamina -= attackData.Type == EAttackType.Weak ? 1 : 3;

        // reduz vida
        defender.Health -= damage;

        // vê se derrubou o BONECO
        attackData.Dead = defender.Health <= 0;
        attackData.Knockdown = defender.CanBeKnockedOut && (Mathf.Approximately(defender.StaminaBar, 0) || attackData.Dead);

        // atualiza o pod pra conter o dano que foi gerado
        attackData.Damage = damage;
    }

    public static void Attack(ref CharacterAttackData attack,
        Vector3 colliderPos, 
        Vector3 colliderSize, 
        Quaternion colliderRot)
    {
        attack.ColliderPos = colliderPos;
        attack.ColliderSz = colliderSize;
        attack.ColliderRot = colliderRot;

        string layer = "Entities";
        if (attack.Attacker)
        {
            layer = attack.Attacker.layer == LayerMask.NameToLayer("Entities") ? "Player" : "Entities";
        }
        else
        {
            Debug.LogWarning("SkillData with no caster. Defaulting attack to entities layer. This shouldn't be happening.");
        }

        Collider[] colliders = Physics.OverlapBox(
            colliderPos, 
            colliderSize, 
            colliderRot, 
            1 << LayerMask.NameToLayer(layer)
        );

        int hits = 0;

        foreach (var c in colliders)
        {
            Vector3 fwd = attack.Attacker.transform.forward;
            Vector3 dir2Collider = (c.transform.position - attack.Attacker.transform.position).normalized;

            var movement = c.gameObject.GetComponent<CharacterMovement>();
            var health = c.gameObject.GetComponent<CharacterHealth>();
            if (movement && movement.IsRolling || !health || health && health.IgnoreDamage ||
                Vector3.Angle(fwd, dir2Collider) >= 60f)
            {
                continue;
            }

            if (c.gameObject == attack.Attacker) continue;
            attack.Defender = c.gameObject;
            CalculateAttackStats(attack.Attacker, c.gameObject, ref attack);

            var combat = c.gameObject.GetComponent<CharacterCombat>();

            attack.CancelAnimation = (attack.DefenderStats.CanBeKnockedOut && ((combat && !combat.IsOnHeavyAttack) ||
                                                                                attack.Type == EAttackType.Strong ||
                                                                                attack.DefenderStats.Health == 0))
                                      || attack.DefenderStats.StaminaBar < 0.25f;  
            //attack.CancelAnimation |= attack.Type == EAttackType.Strong;

            c.gameObject.GetComponent<CharacterHealth>()?.TakeDamage(attack);

            hits++;
        }

        if (hits > 0)
        {
            SoundManager.Instance.PlayHit(colliderPos);
        }

        lastAttack = attack;
    }

    public static void Heal(CharacterStats healer, CharacterStats healed)
    {
        healed.Health += (int)(healer.GetAttributeTotal(EAttribute.Magic) * 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || lastAttack.Time == 0f) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(lastAttack.ColliderPos, lastAttack.ColliderRot, lastAttack.ColliderSz);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}