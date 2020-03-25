using UnityEngine;

public enum EAttackType
{
    Weak,
    Strong
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
        return (c.Attributes.Vigor+c.Inventory.GetTotalAttributes().Vigor) * 19;
    }

    public static int GetMaxMana(CharacterStats c)
    {
        return (c.Attributes.Magic+c.Inventory.GetTotalAttributes().Magic) * 19;
    }

    public static int GetDamage(CharacterStats attacker, CharacterStats defender, Vector3 attackerForward, Vector3 defenderForward, EAttackType attackType)
    {
        var dmgScaling = attacker.Inventory.GetTotalDamageScaling();

        float str = (attacker.GetAttributeTotal(EAttribute.Strength)) * (1f + dmgScaling.Strength);
        float dex = (attacker.GetAttributeTotal(EAttribute.Dexterity)) * dmgScaling.Dexterity;
        float mag = (attacker.GetAttributeTotal(EAttribute.Magic)) * dmgScaling.Magic;
        float crit = Random.value < GetCritChance(attacker) ? GetCritFactor(attacker) : 1f;

        float backstab = 1f + Mathf.Max(0f, Vector3.Dot(attackerForward, defenderForward));

        return Mathf.FloorToInt((str + dex + mag) * crit * backstab) * (attackType == EAttackType.Weak?1:4);
    }


    public static void CalculateAttackStats(GameObject attacker, GameObject defender, ref CharacterAttackData attackData)
    {
        CalculateAttackStats(
            attacker.GetComponent<CharacterData>()?.Stats, 
            defender.GetComponent<CharacterData>().Stats,
            ref attackData
        );
    }

    public static void CalculateAttackStats(CharacterStats attacker, CharacterStats defender, ref CharacterAttackData attackData)
    {
        attackData.AttackerStats = attacker;
        attackData.DefenderStats = defender;

        // calcula dano cru
        int damage = 0;
        if (attacker == null)
        {
            // TODO: remover isso aqui
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

        // TODO: poise bar legítimo
        defender.PoiseBar -= (defender.Poise*0.1f) / defender.Poise;

        // reduz vida
        defender.Health -= damage;

        // vê se derrubou o BONECO
        attackData.Knockdown = Mathf.Approximately(defender.PoiseBar, 0) || defender.Health <= 0;
        attackData.Dead = defender.Health <= 0;

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

        Collider[] colliders = Physics.OverlapBox(
            colliderPos, 
            colliderSize, 
            colliderRot, 
            1 << LayerMask.NameToLayer("Entities")
        );

        if (colliders.Length > 1)
        {
            SoundManager.Instance.PlayHit(colliderPos);
        }

        foreach (var c in colliders)
        {
            var movement = c.gameObject.GetComponent<CharacterMovement>();
            if (movement && movement.IsRolling)
            {
                continue;
            }

            if (c.gameObject == attack.Attacker) continue;
            attack.Defender = c.gameObject;
            CalculateAttackStats(attack.Attacker, c.gameObject, ref attack);

            var combat = c.gameObject.GetComponent<CharacterCombat>();

            attack.CancelAnimation = ((combat && !combat.IsOnHeavyAttack) ||
                attack.Type == EAttackType.Strong ||
                attack.DefenderStats.Health == 0) &&
                ((attack.DefenderStats.Attributes.Vigor < attack.AttackerStats.Attributes.Strength) || attack.DefenderStats.PoiseBar < 0.5f);
            //attack.CancelAnimation |= attack.Type == EAttackType.Strong;

            c.gameObject.GetComponent<CharacterHealth>()?.TakeDamage(attack);
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