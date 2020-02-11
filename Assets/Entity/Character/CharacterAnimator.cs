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

    private Material[] Material
    {
        get; set;
    }

    private float hitEffectFactor;
    private float HitEffectFactor
    {
        get { return hitEffectFactor; }
        set
        {
            hitEffectFactor = value;
            Array.ForEach(Material, m => m.SetFloat("_HitFactor", value));
        }
    }

    [Header("Freeze")] 
    public float AnimationFreezeFrameTime = 0.15f;

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
    int damagedHash = Animator.StringToHash("Damaged");
    int knockdownHash = Animator.StringToHash("Knockdown");
    int damagedNHitsHash = Animator.StringToHash("DamagedHits");
    int recoverHash = Animator.StringToHash("Recovered");
    int castSkillHash = Animator.StringToHash("Cast");

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
        combat.OnRequestSkillUse += OnRequestSkillUseCallback;
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
        animator.SetBool(_movingHash, movement.Velocity.sqrMagnitude > 0.0f);
        animator.SetFloat(_speedYHash, Mathf.Clamp(movement.Velocity.y, -1f, 1f));

        if (animator.speed < 1f && Time.time > _timeSpeedReset + AnimationFreezeFrameTime)
        {
            animator.speed = AnimatorDefaultSpeed;
        }

        if (!Mathf.Approximately(HitEffectFactor, 0f))
        {
            HitEffectFactor = Mathf.Max(0f, HitEffectFactor - Time.deltaTime * 2f);
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

        if (attack.CancelAnimation)
        {
            animator.SetInteger(damagedNHitsHash, attack.HitNumber);
            animator.SetTrigger(attack.Knockdown ? knockdownHash : damagedHash);
        }

        HitEffectFactor = 1f;
        _timeSpeedReset = Time.time;
        animator.speed = 0f;
    }

    private void OnRequestCharacterAttackCallback(EAttackType type)
    {
        animator.SetTrigger(type == EAttackType.Weak ? _weakAttackHash : _strongAttackHash);
    }

    private void OnRequestSkillUseCallback(BaseSkill obj)
    {
        animator.SetTrigger(castSkillHash);
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
        animator.SetTrigger(recoverHash);
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
        Equip(model.gameObject, item.Stats);
    }

    public void Equip(ItemConfig cfg)
    {
        Equip(Instantiate(cfg.Prefab), cfg.Stats);
    }

    public void Equip(GameObject model, ItemStats item)
    {
       
        equippedWeapon = model.gameObject;

        Transform handBone = null;
        Quaternion rotation = Quaternion.identity;
        Vector3 position = Vector3.zero;

        if (item.ItemType == EItemType.Equip && item.Slot == EInventorySlot.Weapon)
        {
            if (item.WeaponType == EWeaponType.Scepter)
            {
                handBone = ModelInfo.RightHandBone.Bone.Find("WeaponHolder");
                rotation = Quaternion.Euler(-90f, 0f, 0f);
                position = new Vector3(0.03f, 0.03f, -0.62f);
            }
            else if (item.WeaponType == EWeaponType.TwoHandedSword)
            {
                handBone = ModelInfo.LeftHandBone.Bone.Find("WeaponHolder");
                rotation = Quaternion.Euler(-131f, -81f, -111f);
                position = new Vector3(-0.055f, -0.025f, 0.346f);
            }
            else if (item.WeaponType == EWeaponType.Sword)
            {
                handBone = ModelInfo.RightHandBone.Bone.Find("WeaponHolder");
                rotation = Quaternion.Euler(-26.666f, 85.7f, -117f);
                position = new Vector3(-0.017f, 0.12f, 0.072f);
            }

            else
            {
                handBone = ModelInfo.LeftHandBone.Bone.Find("WeaponHolder");
                rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        model.transform.SetParent(handBone, true);
        //model.transform.parent = handBone;
        model.transform.localRotation = rotation;
        model.transform.localPosition = position;
        //model.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (item.ItemType == EItemType.Equip)
        {
            if (item.Slot == EInventorySlot.Weapon)
            {
                animator.runtimeAnimatorController = CharacterManager.Instance.Config.GetRuntimeAnimatorController(item);
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
            animator.runtimeAnimatorController = CharacterManager.Instance.Config.GetRuntimeAnimatorController(EWeaponType.Fists);
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

        List<Material> materials = new List<Material>();
        foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            materials.Add(r.material);
        }

        Material = materials.ToArray();
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
        //Rect r = UIManager.WorldSpaceGUI(transform.position + Vector3.down, Vector2.one * 100f);
        //GUI.Label(r, animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
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
