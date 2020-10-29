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

    public AttackRequest(CharacterData attacker, CharacterData defender, EAttackType attackType)
    {
        AttackerData = attacker;
        DefenderData = defender;
        Type = attackType;
    }
}

public class CharacterAttackData
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

    public CharacterAttackData(AttackRequest request)
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
    private static CharacterAttackData lastAttack;

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

    public static void CalculateAttackStats(ref CharacterAttackData attackData)
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

    public static CharacterAttackData[] Attack(
        CharacterData attacker,
        EAttackType attackType,
        Vector3 colliderPos, 
        Vector3 colliderSize, 
        Quaternion colliderRot)
    {
        Collider[] colliders = CollectDefenders(
            attacker, 
            colliderPos, 
            colliderSize, 
            colliderRot
        );

        if (colliders == null) 
            return null;

        CharacterAttackData[] attackResults = new CharacterAttackData[colliders.Length];

        int hits = 0;
        foreach (var c in colliders)
        {
            if (c.gameObject == attacker.gameObject) continue;

            CharacterData defender = c.GetComponent<CharacterData>();

            AttackRequest request = new AttackRequest(attacker, defender, attackType);
            CharacterAttackData attackData = AttackCharacter(request);
            if (attackData == null) continue;

            lastAttack = attackData;
            attackResults[hits] = attackData;
            hits++;
        }

        /*
        if (hits > 0)
            SoundManager.Instance.PlayHit(colliderPos);
        */

        return attackResults;
    }

    public static Collider[] CollectDefenders(
        CharacterData attacker, 
        Vector3 colliderPos,
        Vector3 colliderSize,
        Quaternion colliderRot)
    {
        string layer = "Entities";

        Collider[] colliders = Physics.OverlapBox(
            colliderPos, 
            colliderSize/2f, 
            colliderRot, 
            1 << LayerMask.NameToLayer(layer)
        );

        if (colliders.Length == 0 ) return null;
        return colliders;
    }

    public static CharacterAttackData AttackCharacter(AttackRequest request)
    {
        CharacterData attacker = request.AttackerData;
        CharacterData defender = request.DefenderData;

        bool hasCharacterData = defender != null;
        if (!hasCharacterData) return null;
        if (!defender.Components.Health) return null;

        Vector3 fwd = attacker.transform.forward;
        Vector3 dir2Collider = (defender.transform.position - attacker.transform.position).normalized;
        float attackAngle = Vector3.Angle(fwd, dir2Collider);
        bool isValidAttackAngle = attackAngle <= 60f; 
        if (!isValidAttackAngle) return null;

        CharacterAttackData attackData = new CharacterAttackData(request);
        CalculateAttackStats(ref attackData);

        defender.Components.Health.TakeDamage(attackData);
        return attackData;
    }

    public static void Heal(CharacterStats healer, CharacterStats healed)
    {
        healed.Health += (int)(healer.Attributes.Magic * 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || lastAttack.Time == 0f) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(lastAttack.ColliderPos, lastAttack.ColliderRot, lastAttack.ColliderSz);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
