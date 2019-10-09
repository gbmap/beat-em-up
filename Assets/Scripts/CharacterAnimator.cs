using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;

    CharacterData _charData;
    CharacterMovement _charMovement;
    CharacterCombat _charCombat;
    CharacterHealth _charHealth;

    // ==== MOVEMENT
    int _movingHash = Animator.StringToHash("Moving");

    // ===== COMBAT
    int _weakAttackHash = Animator.StringToHash("WeakAttack");
    int _strongAttackHash = Animator.StringToHash("StrongAttack");

    // ====== HEALTH
    int _damagedHash = Animator.StringToHash("Damaged");
    int _damagedNHits = Animator.StringToHash("DamagedHits");
    int _recoverHash = Animator.StringToHash("Recovered");

    // Animator speed reset timer
    float _timeSpeedReset;

    // Start is called before the first frame update
    void Awake()
    {
        _charData = GetComponent<CharacterData>();
        _charMovement = GetComponent<CharacterMovement>();
        _charHealth = GetComponent<CharacterHealth>();
        _charCombat = GetComponent<CharacterCombat>();
    }

    private void OnEnable()
    {
        var attackSM = animator.GetBehaviour<AttackStateMachineBehaviour>();
        attackSM.OnComboStarted += delegate { _charCombat.OnComboStarted?.Invoke(); };
        attackSM.OnComboEnded += delegate { _charCombat.OnComboEnded?.Invoke(); };

        _charCombat.OnRequestCharacterAttack += OnRequestCharacterAttackCallback;
        _charCombat.OnCharacterAttack += OnCharacterAttackCallback;

        _charHealth.OnDamaged += OnCharacterDamagedCallback;
        _charHealth.OnGetUp += OnGetUpCallback;

        // Isso aqui tá bugando pq a Unity não garante que o OnEnable vai ser chamado antes do Awake pra componentes diferentes.
        // Eventualmente a gente vai precisar disso aqui, até lá tem que pensar num trabalho a redondo.
        //_charData.Stats.OnStatsChanged += OnStatsChangedCallback;
    }

    private void OnDisable()
    {
        var attackSM = animator.GetBehaviour<AttackStateMachineBehaviour>();
        if (attackSM != null)
        {
            attackSM.OnComboStarted = null;
            attackSM.OnComboEnded = null;
        }

        _charCombat.OnRequestCharacterAttack -= OnRequestCharacterAttackCallback;
        _charCombat.OnCharacterAttack -= OnCharacterAttackCallback;

        _charHealth.OnDamaged -= OnCharacterDamagedCallback;
    }

    private void OnCharacterDamagedCallback(CharacterAttackData attack)
    {
        animator.SetInteger(_damagedNHits, attack.hitNumber);
        animator.SetTrigger(_damagedHash);

        //_timeSpeedReset = Time.time;
        //animator.speed = 0f;
    }

    private void OnRequestCharacterAttackCallback(EAttackType type)
    {
        animator.SetTrigger(type == EAttackType.Weak ? _weakAttackHash : _strongAttackHash);
    }

    private void OnCharacterAttackCallback(CharacterAttackData attack)
    {
        if (attack.defender != null)
        {
            _timeSpeedReset = Time.time;
            animator.speed = 0f;
        }
    }

    private void OnGetUpCallback()
    {
        animator.SetTrigger(_recoverHash);
    }

    private void OnStatsChangedCallback(CharacterStats stats)
    {
        EWeaponType type = EWeaponType.Fists;
        if (stats.Inventory.ContainsKey(EInventorySlot.Weapon) && stats.Inventory[EInventorySlot.Weapon] != null)
        {
            type = (stats.Inventory[EInventorySlot.Weapon] as Weapon).Type;
        }

        var controller = CombatManager.Instance.Config.WeaponTypeToController(type);
        if (controller != animator.runtimeAnimatorController)
        {
            animator.runtimeAnimatorController = controller;
        }
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool(_movingHash, _charMovement.direction.sqrMagnitude > 0.15f);

        if (animator.speed < 1f && Time.time > _timeSpeedReset + 0.2f)
        {
            animator.speed = 1f;
        }
    }
}
