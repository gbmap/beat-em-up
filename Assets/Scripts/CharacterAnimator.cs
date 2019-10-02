﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(CharacterMovement))]
public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;

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
        _charMovement = GetComponent<CharacterMovement>();
        _charHealth = GetComponent<CharacterHealth>();
        _charCombat = GetComponent<CharacterCombat>();
    }

    private void OnEnable()
    {
        var attackSM = animator.GetBehaviour<AttackStateMachineBehaviour>();
        attackSM.OnComboStarted += delegate { _charCombat.OnComboStarted?.Invoke(); };
        attackSM.OnComboEnded += delegate { _charCombat.OnComboEnded?.Invoke(); };

        var healthSM = animator.GetBehaviour<HurtStateMachineBehaviour>();
        healthSM.OnCharacterFall += _charHealth.OnFall;
        healthSM.OnCharacterGetUp += _charHealth.OnGetUp;

        _charCombat.OnRequestCharacterAttack += OnRequestCharacterAttackCallback;
        _charCombat.OnCharacterAttack += OnCharacterAttackCallback;

        _charHealth.OnDamaged += OnCharacterDamagedCallback;
        _charHealth.OnGetUp += OnGetUpCallback;
    } 

    private void OnDisable()
    {
        var attackSM = animator.GetBehaviour<AttackStateMachineBehaviour>();
        if (attackSM != null)
        {
            attackSM.OnComboStarted = null;
            attackSM.OnComboEnded = null;
        }

        var healthSM = animator.GetBehaviour<HurtStateMachineBehaviour>();
        if (healthSM != null)
        {
            healthSM.OnCharacterFall += _charHealth.OnFall;
            healthSM.OnCharacterGetUp += _charHealth.OnGetUp;
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
        if (attack.hits != null && attack.hits.Length > 0)
        {
            _timeSpeedReset = Time.time;
            animator.speed = 0f;
        }
    }

    private void OnGetUpCallback()
    {
        animator.SetTrigger(_recoverHash);
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
