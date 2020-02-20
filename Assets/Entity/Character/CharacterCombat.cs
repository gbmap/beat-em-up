using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [HideInInspector] public bool IsOnCombo;

    private int _nComboHits;

    public System.Action<EAttackType> OnRequestCharacterAttack;
    public System.Action<CharacterAttackData> OnCharacterAttack;

    private BaseSkill skillBeingCasted;
    public System.Action<BaseSkill> OnRequestSkillUse;
    public System.Action<BaseSkill> OnSkillUsed;

    public System.Action OnComboStarted;
    public System.Action OnComboEnded;

    private CharacterData data;
    private CharacterHealth health;
    private CharacterMovement movement;
    private CharacterAnimator animator;

    public bool IsOnHeavyAttack;

    private Vector3 attackColliderBasePosition
    {
        get { return animator.RealCharacterPosition + transform.forward*0.75f + Vector3.up; }
    }

    private Vector3 attackColliderSize
    {
        get { return (Vector3.one * 0.5f + Vector3.right * 0.5f) * (1f+((float)data.Stats.GetAttributeTotal(EAttribute.Strength)) / CharacterStats.MaxAttributeLevel); }
    }

    private Vector3 GetAttackColliderSize(EAttackType type)
    {
        return attackColliderSize * (type == EAttackType.Weak ? 1.5f : 2.5f);
    }

    public CharacterAttackData LastAttackData
    {
        get; private set;
    }

    public CharacterAttackData LastDamageData
    {
        get; private set;
    }

    private void Awake()
    {
        LastDamageData = new CharacterAttackData
        {
            Time = float.NegativeInfinity
        };

        health = GetComponent<CharacterHealth>();
        data = GetComponent<CharacterData>();
        movement = GetComponent<CharacterMovement>();
        animator = GetComponent<CharacterAnimator>();
    }

    private void OnEnable()
    {
        OnComboStarted += OnComboStartedCallback;
        OnComboEnded += OnComboEndedCallback;

        health.OnFall += OnFallCallback;
        health.OnDamaged += OnDamagedCallback;

        movement.OnRoll += OnRollCallback;
    }
    
    private void OnDisable()
    {
        OnComboStarted -= OnComboStartedCallback;
        OnComboEnded -= OnComboEndedCallback;

        health.OnFall -= OnFallCallback;
        health.OnDamaged -= OnDamagedCallback;

        movement.OnRoll -= OnRollCallback;
    }

    private void OnRollCallback()
    {
        OnComboEnded?.Invoke();
    }

    public void RequestAttack(EAttackType type)
    {
        OnRequestCharacterAttack?.Invoke(type);
    }

    /*
    * CALLBACKS
    */
    private void OnComboStartedCallback()
    {
        Debug.Log("Combo ended.");
        IsOnCombo = true;
    }

    private void OnComboEndedCallback()
    {
        Debug.Log("Combo ended.");
        IsOnCombo = false;
        _nComboHits = 0;
    }

    private void OnFallCallback()
    {
        OnComboEnded?.Invoke();
    }

    private void OnDamagedCallback(CharacterAttackData msg)
    {
        if (msg.CancelAnimation)
        {
            OnComboEnded?.Invoke();
        }

        LastDamageData = msg;
    }

    /*
    *  CHAMADO PELO ANIMATOR!!!1111
    */
    public void Attack(EAttackType type)
    {
        Attack(new CharacterAttackData(type, gameObject, ++_nComboHits));
    }

    private void Attack(CharacterAttackData attack)
    {
        /*
        Collider[] colliders = Physics.OverlapBox(
            attackColliderBasePosition,
            GetAttackColliderSize(attack.Type),
            transform.rotation,
            1 << LayerMask.NameToLayer("Entities")
        );

        if (colliders.Length > 1)
        {
            SoundManager.Instance.PlayHit(transform.position);
        }

        foreach (var c in colliders)
        {
            if (c.gameObject.GetComponent<CharacterMovement>().IsRolling)
            {
                continue;
            }

            if (c.gameObject == gameObject) continue;
            attack.Defender = c.gameObject;
            CombatManager.Attack(gameObject, c.gameObject, ref attack);

            attack.CancelAnimation = !c.gameObject.GetComponent<CharacterCombat>().IsOnHeavyAttack;
            attack.CancelAnimation |= attack.Type == EAttackType.Strong;

            c.gameObject.GetComponent<CharacterHealth>()?.TakeDamage(attack);
        }*/
        CombatManager.Attack(ref attack, attackColliderBasePosition, GetAttackColliderSize(attack.Type), transform.rotation);

        OnCharacterAttack?.Invoke(attack);

        LastAttackData = attack;
    }

    /*
     * Skills
     * */
    public void AnimUseWeaponSkill(int index)
    {
        ItemStats weapon = data.Stats.Inventory[EInventorySlot.Weapon];
        BaseSkill skill = weapon.Skills[index];
        Instantiate(skill.Prefab, transform.position + transform.forward * 1.5f, transform.rotation);
    }

    public void RequestSkillUse(BaseSkill skill)
    {
        skillBeingCasted = skill;
        OnRequestSkillUse?.Invoke(skill);
    }

    public void AnimSkillUsed()
    {
        // fazer algo com a skill sendo castada.
        OnSkillUsed?.Invoke(skillBeingCasted);
        skillBeingCasted = null;
    }

    public void AnimPlayWoosh()
    {
        Vector3 dir = movement.Direction;

        movement.ApplySpeedBump(dir, movement.SpeedBumpForce*0.5f);
        SoundManager.Instance.PlayWoosh(transform.position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (Time.time < LastAttackData.Time + 1f)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(attackColliderBasePosition, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(Vector3.zero, GetAttackColliderSize(LastAttackData.Type));
        }
    }
#endif

}
