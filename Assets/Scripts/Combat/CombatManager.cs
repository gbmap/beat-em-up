using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data;

public enum EAttackType
{
    Weak = 0,
    Strong,
    Skill
}

public class AttackRequest
{
    public CharacterData AttackerData { get; private set; }
    public CharacterStats AttackerStats { get { return AttackerData.Stats; } }
    public GameObject Attacker { get { return AttackerData.gameObject; }}

    public CharacterData DefenderData { get; private set; }
    public CharacterStats DefenderStats { get { return DefenderData.Stats; } }
    public GameObject Defender { get { return DefenderData.gameObject; } }

    public EAttackType Type;
    public bool IgnoreFacingDirection;

    public AttackRequest(
        CharacterData attacker, 
        CharacterData defender, 
        EAttackType attackType,
        bool ignoreFacingDirection = false
    ) {
        AttackerData = attacker;
        DefenderData = defender;
        Type = attackType;
        IgnoreFacingDirection = ignoreFacingDirection;
    }
}

public class AttackResult
{
    public AttackRequest Request { get; private set; }

    public CharacterData AttackerData { get { return Request.AttackerData; } }
    public CharacterStats AttackerStats { get { return AttackerData.Stats; } }
    public GameObject Attacker { get { return AttackerData.gameObject; } }

    public CharacterData DefenderData { get { return Request.DefenderData; } }
    public CharacterStats DefenderStats { get { return DefenderData.Stats; } }
    public GameObject Defender { get { return DefenderData.gameObject; } }

    public EAttackType Type { get { return Request.Type; } }

    public float Time;

    public int Damage;
    public int HitNumber;
    public bool Dead;

    public bool Knockdown;
    public bool CancelAnimation;

    public Vector3 ColliderPos;
    public Vector3 ColliderSz;
    public Quaternion ColliderRot;

    public AttackResult(AttackRequest request)
    {
        Request         = request;
        Time            = UnityEngine.Time.time;
        Damage          = 0;
        HitNumber       = 0;
        Knockdown       = false;
        CancelAnimation = false;
        Dead            = false;

        ColliderPos = Vector3.zero;
        ColliderSz  = Vector3.zero;
        ColliderRot = Quaternion.identity;
    }
}

public class CombatManager : SimpleSingleton<CombatManager>
{
    public static float GetCritFactor(CharacterStats c)
    {
        return 2f;
    }

    public static float GetCritChance(CharacterStats c)
    {
        float d = c.Attributes.Dexterity;

        // esse 256 é arbitrário e significa o valor de destreza que equivale a 50% de chance de crit.
        return 1f - (1f / (Mathf.Pow(d/256, 2f)+1f));
        // return Mathf.Pow(d/256, 1f/1.75f);
    }

    public static float GetPoiseChance(CharacterStats c)
    {
        float d = c.Attributes.Dexterity;
        return 1f - (1f / (Mathf.Pow(d / 256, 2f) + 1f));
    }

    public static int GetMaxHealth(CharacterStats c)
    {
        return (c.Attributes.Vigor) * 19;
    }

    public static int GetMaxMana(CharacterStats c)
    {
        return (c.Attributes.Magic) * 19;
    }

    public static int GetDamage(CharacterStats attacker, CharacterStats defender, Vector3 attackerForward, Vector3 defenderForward, EAttackType attackType)
    {
        float str = (attacker.Attributes.Strength) * (attackType == EAttackType.Weak ? 1f : 4f);
        float crit = Random.value < GetCritChance(attacker) ? GetCritFactor(attacker) : 1f;
        float backstab = Mathf.Max(0f, Vector3.Dot(attackerForward, defenderForward));

        return Mathf.RoundToInt((str+(str*backstab)) * crit);
    }

    public static void CalculateAttackStats(ref AttackResult attackData)
    {
        CharacterStats attacker = attackData.AttackerStats;
        CharacterStats defender = attackData.DefenderStats;

        int damage = 0;
        if (attacker == null || attackData.Type == EAttackType.Skill)
        {
            damage = attackData.Damage;
        }
        else
            damage = GetDamage(attacker, defender, attackData.Attacker.transform.forward, attackData.Defender.transform.forward, attackData.Type);

        defender.Health -= damage;
        defender.CurrentStamina -= attackData.Type == EAttackType.Weak ? 1 : 3;
        attackData.Dead = defender.Health <= 0;
        attackData.Knockdown = defender.CanBeKnockedOut && (Mathf.Approximately(defender.StaminaBar, 0) || attackData.Dead);
        attackData.Damage = damage;

        CharacterCombat defenderCombat = attackData.DefenderData.Components.Combat; 
        bool canBeKnockedOut = defender.CanBeKnockedOut;
        bool IsOnHeavyAttack = defenderCombat && defenderCombat.IsOnHeavyAttack;
        bool IsStrongAttack = attackData.Type == EAttackType.Strong;
        bool IsLowOnStamina = defender.StaminaBar < 0.25f;
        attackData.CancelAnimation = (canBeKnockedOut && 
                                     (!IsOnHeavyAttack 
                                     || IsStrongAttack 
                                     || attackData.Dead 
                                     || IsLowOnStamina));
    }

    public static AttackResult AttackCharacter(AttackRequest request)
    {
        CharacterData attacker = request.AttackerData;
        CharacterData defender = request.DefenderData;

        bool hasCharacterData = defender != null;
        if (!hasCharacterData) return null;
        if (!defender.Components.Health) return null;

        if (!request.IgnoreFacingDirection)
        {
            Vector3 fwd = attacker.transform.forward;
            Vector3 dir2Collider = (defender.transform.position - attacker.transform.position).normalized;
            float attackAngle = Vector3.Angle(fwd, dir2Collider);
            bool isValidAttackAngle = attackAngle <= 60f; 
            if (!isValidAttackAngle) return null;
        }

        AttackResult attackData = new AttackResult(request);
        CalculateAttackStats(ref attackData);

        defender.Components.Health.TakeDamage(attackData);
        return attackData;
    }
}
