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
        return attackColliderSize * (type == EAttackType.Weak ? 1f : 1.1f);
    }

    private CharacterAttackData lastAttackData;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
        data = GetComponent<CharacterData>();
        movement = GetComponent<CharacterMovement>();
        animator = GetComponent<CharacterAnimator>();

        lastAttackData.Time = float.NegativeInfinity;
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

    private void OnDamagedCallback(CharacterAttackData obj)
    {
        OnComboEnded?.Invoke();
    }

    /*
    *  CHAMADO PELO ANIMATOR!!!1111
    */
    public void Attack(EAttackType type)
    {
        Attack(new CharacterAttackData { Type = type, Attacker = gameObject, HitNumber = ++_nComboHits, Time = Time.time });
    }

    private void Attack(CharacterAttackData attack)
    {
        Collider[] colliders = Physics.OverlapBox(
            attackColliderBasePosition,
            GetAttackColliderSize(attack.Type), 
            transform.rotation, 
            1 << LayerMask.NameToLayer("Entities")
        );

        if (colliders.Length > 0)
        {
            SoundManager.Instance.PlayHit(transform.position);
        }

        foreach (var c in colliders)
        {
            if (c.gameObject == gameObject) continue;
            attack.Defender = c.gameObject;
            CombatManager.Attack(gameObject, c.gameObject, ref attack);
            c.gameObject.GetComponent<CharacterHealth>()?.TakeDamage(attack);
        }

        OnCharacterAttack?.Invoke(attack);

        lastAttackData = attack;
    }

    /*
     * Skills
     * */
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
        SoundManager.Instance.PlayWoosh(transform.position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (Time.time < lastAttackData.Time + 1f)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(attackColliderBasePosition, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(Vector3.zero, GetAttackColliderSize(lastAttackData.Type));
        }
    }
#endif

}
