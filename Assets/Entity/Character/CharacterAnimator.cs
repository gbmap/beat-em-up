using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;

    public Vector3 RealCharacterPosition {
        get
        {
            Vector3 delta = ModelInfo.HipsBone.Bone.position - transform.position;
            delta.y = 0f;
            return transform.position + delta;
        }
    }

    CharacterData _charData;
    CharacterMovement movement;
    CharacterCombat combat;
    CharacterHealth health;
    CharacterModelInfo modelInfo;
    CharacterModelInfo ModelInfo
    {
        get { return modelInfo ?? (modelInfo = GetComponentInChildren<CharacterModelInfo>()); }
    }
    

    private float AnimatorDefaultSpeed
    {
        get { return _charData.BrainType == ECharacterBrainType.Input ? 1.3f : 1f; }
    }

    // ==== MOVEMENT
    int _movingHash = Animator.StringToHash("Moving");
    int _isOnAirHash = Animator.StringToHash("IsOnAir");
    int _speedYHash = Animator.StringToHash("SpeedY");
    int _jumpTriggerHash = Animator.StringToHash("Jump");
    int _rollTriggerHash = Animator.StringToHash("Roll");

    // ===== COMBAT
    int _weakAttackHash = Animator.StringToHash("WeakAttack");
    int _strongAttackHash = Animator.StringToHash("StrongAttack");

    GameObject equippedWeapon;

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
        movement = GetComponent<CharacterMovement>();
        health = GetComponent<CharacterHealth>();
        combat = GetComponent<CharacterCombat>();
        modelInfo = GetComponent<CharacterModelInfo>();

        animator.speed = AnimatorDefaultSpeed;
    }

    private void OnEnable()
    {
        combat.OnRequestCharacterAttack += OnRequestCharacterAttackCallback;
        combat.OnCharacterAttack += OnCharacterAttackCallback;

        health.OnDamaged += OnCharacterDamagedCallback;
        health.OnRecover += OnRecoverCallback;

        movement.OnRoll += OnRollCallback;

        // Isso aqui tá bugando pq a Unity não garante que o OnEnable vai ser chamado antes do Awake pra componentes diferentes.
        // Eventualmente a gente vai precisar disso aqui, até lá tem que pensar num trabalho a redondo.
        //_charData.Stats.OnStatsChanged += OnStatsChangedCallback;
    }

    private void OnDisable()
    {
        combat.OnRequestCharacterAttack -= OnRequestCharacterAttackCallback;
        combat.OnCharacterAttack -= OnCharacterAttackCallback;

        health.OnDamaged -= OnCharacterDamagedCallback;
        health.OnRecover -= OnRecoverCallback;

        movement.OnRoll -= OnRollCallback;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool(_movingHash, movement.Velocity.sqrMagnitude > 0.05f);
        animator.SetFloat(_speedYHash, Mathf.Clamp(movement.Velocity.y, -1f, 1f));

        if (animator.speed < 1f && Time.time > _timeSpeedReset + .35f)
        {
            animator.speed = AnimatorDefaultSpeed;
        }

#if UNITY_EDITOR
        CheckDebugInput();
#endif

    }

    private void OnCharacterDamagedCallback(CharacterAttackData attack)
    {
        if (attack.Poised)
        {
            return;
        }

        animator.ResetTrigger(_weakAttackHash);
        animator.ResetTrigger(_strongAttackHash);

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

    private void OnRecoverCallback()
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

    private void OnJumpCallback()
    {
        animator.SetTrigger(_jumpTriggerHash);
    }

    private void OnRollCallback()
    {
        animator.speed = AnimatorDefaultSpeed;
        animator.ResetTrigger(_weakAttackHash);
        animator.ResetTrigger(_strongAttackHash);
        animator.SetTrigger(_rollTriggerHash);
    }

    public void Equip(ItemData item)
    {
        var model = item.transform.Find("ModelRoot").GetChild(0);
        equippedWeapon = model.gameObject;
        model.transform.parent = ModelInfo.HandBone.Bone;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (item.Stats.ItemType == EItemType.Equip)
        {
            if (item.Stats.Slot == EInventorySlot.Weapon)
            {
                animator.runtimeAnimatorController = CharacterManager.Instance.Config.GetRuntimeAnimatorController(item.Stats);
            }
        }
    }

    public void UnEquip(EInventorySlot slot)
    {
        if (slot != EInventorySlot.Weapon)
        {
            return;
        }

        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
        }
    }

    public void ResetAttackTrigger()
    {
        animator.ResetTrigger("WeakAttack");
        animator.ResetTrigger("StrongAttack");
    }

    // gambiarra 
    public void RefreshAnimator()
    {
        Avatar avatar = animator.avatar;
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        RefreshAnimator(avatar, controller);
    }

    public void RefreshAnimator(Avatar avatar, RuntimeAnimatorController controller)
    {
        if (avatar == null)
        {
            avatar = animator.avatar;
        }

        DestroyImmediate(animator);
        animator = gameObject.AddComponent<Animator>();
        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;
        modelInfo = GetComponentInChildren<CharacterModelInfo>();
    }

    public void SetRootMotion(bool v)
    {
        animator.applyRootMotion = v;
    }

    public void UpdatePosition()
    {
        Vector3 hipsPosition = ModelInfo.HipsBone.Bone.position - transform.position;
        hipsPosition.y = 0f;
        transform.position += hipsPosition;
    }

#if UNITY_EDITOR

    private void OnGUI()
    {
        Rect r = UIManager.WorldSpaceGUI(transform.position + Vector3.down, Vector2.one * 100f);
        GUI.Label(r, animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }

    private bool showDeltaHips = false;

    private void OnDrawGizmosSelected()
    {
        if (showDeltaHips)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, ModelInfo.HipsBone.Bone.position);

            Vector3 hipsDelta = ModelInfo.HipsBone.Bone.position - transform.position;
            hipsDelta.y = 0f;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + hipsDelta);
        }
    }

    private void CheckDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            showDeltaHips = !showDeltaHips;
        }
    }

#endif

}
