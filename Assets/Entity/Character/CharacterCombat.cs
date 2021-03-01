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

    public SoundManager Sounds;

    public bool IsOnHeavyAttack;

    private Vector3 attackColliderBasePosition
    {
        get { return animator.RealCharacterPosition + transform.forward*1.25f + Vector3.up; }
    }

    private Vector3 attackColliderSize
    {
        get { return (Vector3.one * 0.65f + Vector3.right * 0.65f); }
    }

    private Vector3 GetAttackColliderSize(EAttackType type)
    {
        float weaponScale = 0f;
        
        if (data.Stats.Inventory.HasEquip(EInventorySlot.Weapon))
        {
            weaponScale = data.Stats.Inventory[EInventorySlot.Weapon].WeaponColliderScaling;
        }

        return attackColliderSize * (type == EAttackType.Weak ? 1.0f : 1.5f) + Vector3.one * weaponScale;
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
        Sounds.PlayRoll(transform.position);
        OnComboEnded?.Invoke();
    }

    public void RequestAttack(EAttackType type)
    {
        if (health.IsDead || (health.IsBeingDamaged && health.CanBeKnockedOut) ) return;
        OnRequestCharacterAttack?.Invoke(type);
    }

    /*
    * CALLBACKS
    */
    private void OnComboStartedCallback()
    {
        Debug.Log("Combo started.");
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
        if (health.IsDead || health.IsOnGround) return;
        Attack(new CharacterAttackData(type, gameObject, ++_nComboHits));
    }

    private void Attack(CharacterAttackData attack)
    {
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
        if (weapon == null || weapon.Skills == null)
        {
            Debug.LogWarning("No weapon skills found. This shouldn't be happening.");
            return;
        }

        SkillData skill = weapon.Skills[index];
        UseSkill(skill);
    }

    public void AnimUseCharacterSkill(int index)
    {
        SkillData skill = data.CharacterSkills[index];
        UseSkill(skill);
    }

    private void UseSkill(SkillData s)
    {
        if (s.gameObject == null) // objeto foi destruído por algum motivo, possivelmente o jogo foi ganho?
        {
            return;
        }

        // hack pra determinar se é um prefab
        if (s.gameObject.scene.rootCount == 0)
        {
            var obj = Instantiate(s.gameObject, transform.position + transform.forward * s.Offset.z, transform.rotation);
            s = obj.GetComponent<SkillData>();
            s.Caster = data;
        }
        s.Cast();
    }
    
    public void RequestSkillUse(BaseSkill skill)
    {
        skillBeingCasted = skill;
        OnRequestSkillUse?.Invoke(skill);
    }

    public void UseSkill(int index)
    {
        animator.UseSkill(index);
    }

    public void AnimSkillUsed()
    {
        // fazer algo com a skill sendo castada.
        OnSkillUsed?.Invoke(skillBeingCasted);
        skillBeingCasted = null;
    }

    public void AnimPlayWoosh()
    {
        Vector3 dir = movement.transform.forward;

        movement.ApplySpeedBump(dir, movement.SpeedBumpForce);
        Sounds.PlayWoosh(transform.position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        try
        {
            if (Time.time < LastAttackData.Time + 1f)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(attackColliderBasePosition, transform.rotation, GetAttackColliderSize(LastAttackData.Type));
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
        catch { }
    }
#endif

}
