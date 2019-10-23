using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;
    public Transform HandTransform;

    CharacterData _charData;
    CharacterMovement _charMovement;
    CharacterCombat _charCombat;
    CharacterHealth _charHealth;

    // ==== MOVEMENT
    int _movingHash = Animator.StringToHash("Moving");
    int _isOnAirHash = Animator.StringToHash("IsOnAir");
    int _speedYHash = Animator.StringToHash("SpeedY");

    // ===== COMBAT
    int _weakAttackHash = Animator.StringToHash("WeakAttack");
    int _strongAttackHash = Animator.StringToHash("StrongAttack");

    // ====== HEALTH
    int _damagedHash = Animator.StringToHash("Damaged");
    int _knockdownHash = Animator.StringToHash("Knockdown");
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
        if (attack.Poised)
        {
            return;
        }

        animator.SetInteger(_damagedNHits, attack.HitNumber);
        animator.SetTrigger(attack.Knockdown ? _knockdownHash : _damagedHash);

        _timeSpeedReset = Time.time;
        animator.speed = 0f;
    }

    private void OnRequestCharacterAttackCallback(EAttackType type)
    {
        animator.SetTrigger(type == EAttackType.Weak ? _weakAttackHash : _strongAttackHash);
    }

    private void OnCharacterAttackCallback(CharacterAttackData attack)
    {
        if (attack.Defender != null)
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
        if (stats.Inventory[EInventorySlot.Weapon] != null && stats.Inventory[EInventorySlot.Weapon] != null)
        {
            //type = (stats.Inventory[EInventorySlot.Weapon] as Weapon).Type;
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
        animator.SetBool(_isOnAirHash, _charMovement.IsOnAir);
        animator.SetFloat(_speedYHash, Mathf.Clamp(_charMovement.velocity.y, -1f, 1f));

        if (animator.speed < 1f && Time.time > _timeSpeedReset + .35f)
        {
            animator.speed = 1f;
        }
    }

    public void Equip(ItemData item)
    {
        var model = item.transform.Find("ModelRoot").GetChild(0);
        model.transform.parent = HandTransform;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (item.Stats.ItemType == EItemType.Equip)
        {
            if (item.Stats.Slot == EInventorySlot.Weapon)
            {
                animator.runtimeAnimatorController = CharacterManager.Instance.Config.GetRuntimeAnimatorController(item.Stats.WeaponType);
            }
        }
        
    }
}
