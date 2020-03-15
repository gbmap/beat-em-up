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
    public CharacterModelInfo ModelInfo
    {
        get { return modelInfo ?? (modelInfo = GetComponentInChildren<CharacterModelInfo>()); }
    }

    private float AnimatorDefaultSpeed
    {
        get { return _charData.BrainType == ECharacterBrainType.Input ? 1.3f : 1f; }
    }

    private Material[] Materials
    {
        get; set;
    }

    private new Renderer renderer;

    private float hitEffectFactor;
    private float HitEffectFactor
    {
        get { return hitEffectFactor; }
        set
        {
            hitEffectFactor = value;
            if (Materials == null) return;
            for (int i = 0; i < Materials.Length; i++)
            {
                Material m = Materials[i];
                if (m == null) continue;
                m.SetFloat("_HitFactor", value);
            }
        }
    }

    [Space]
    [Header("FX Impact")]
    public ParticleSystem ParticlesHit;
    public ParticleSystem.MinMaxCurve WeakHitStartSize;
    public ParticleSystem.MinMaxGradient WeakHitStartColor;
    public ParticleSystem.MinMaxCurve StrongHitStartSize;
    public ParticleSystem.MinMaxGradient StrongHitStartColor;

    [Space]
    [Header("FX Smoke")]
    public ParticleSystem ParticlesSmoke;

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

    // ============ EVENTS
    public System.Action<Animator> OnRefreshAnimator;


    #region MONOBEHAVIOUR

    // Start is called before the first frame update
    void Awake()
    {
        _charData = GetComponent<CharacterData>();
        movement = GetComponent<CharacterMovement>();
        health = GetComponent<CharacterHealth>();
        combat = GetComponent<CharacterCombat>();
        modelInfo = GetComponent<CharacterModelInfo>();
        renderer = GetComponentInChildren<Renderer>();

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
        animator.SetBool(_movingHash, movement.Velocity.sqrMagnitude > 0.0f && movement.CanMove);

        UpdateHitFactor();
        UpdateSmokeEmission();

        /*if (true) // pra prevenir o LastDamageData de ser nulo.
        {
            UpdateDeathBlinkAnimation(true, 0f);
        }*/

        
        if (health.IsDead) // pra prevenir o LastDamageData de ser nulo.
        {
            UpdateDeathBlinkAnimation(health.IsDead, combat.LastDamageData.Time);
        }

#if UNITY_EDITOR
        CheckDebugInput();
#endif
    }

    private void UpdateHitFactor()
    {
        if (!Mathf.Approximately(HitEffectFactor, 0f))
        {
            HitEffectFactor = Mathf.Max(0f, HitEffectFactor - Time.deltaTime * 2f);
        }
    }

    private void UpdateSmokeEmission()
    {
        var emission = ParticlesSmoke.emission;
        emission.enabled = movement.IsRolling || combat.IsOnCombo || movement.IsBeingMoved;

        if (!emission.enabled) return;

        if (movement.IsRolling || combat.IsOnCombo)
        {
            ParticlesSmoke.transform.rotation = Quaternion.LookRotation(-transform.forward);
        }
        else if (movement.IsBeingMoved)
        {
            ParticlesSmoke.transform.rotation = Quaternion.LookRotation(-movement.SpeedBumpDir);
        }

        var main = ParticlesSmoke.main;
        ParticleSystem.MinMaxCurve sz = ParticlesSmoke.main.startSize;
        if (movement.IsRolling)
        {
            main.startSize = new ParticleSystem.MinMaxCurve(2, 4);
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2f);
            emission.rateOverDistanceMultiplier = 2f;
            
        }
        else
        {
            main.startSize = new ParticleSystem.MinMaxCurve(1, 2);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.75f);
            emission.rateOverDistanceMultiplier = 5f;
        }
        
    }

    private void UpdateDeathBlinkAnimation(bool isDead, float timeOfDeath)
    {
        if (!isDead || renderer == null) return;

        float timeFactor = Mathf.Max(0f, ((Time.time - timeOfDeath) * 0.75f));
        timeFactor *= timeFactor;
        float y = Mathf.Cos(Time.time * timeFactor);
        bool enabled = y > 0.0f;
        renderer.enabled = enabled;
    }

    private void EmitSmokeRadius()
    {
        if (!ParticlesSmoke) return;

        int range = UnityEngine.Random.Range(15, 20);
        for (int i = 0; i < range; i++)
        {
            Vector3 vel = UnityEngine.Random.insideUnitSphere;
            vel.y = 0f;
            vel.Normalize();
            vel *= 13f;
            ParticlesSmoke.Emit(new ParticleSystem.EmitParams
            {
                startSize = UnityEngine.Random.Range(2, 4),
                velocity = vel
            }, 1);
        }
    }

    private void EmitHitImpact(CharacterAttackData attack)
    {
        var main = ParticlesHit.main;

        if (attack.Type == EAttackType.Weak)
        {
            main.startSize = WeakHitStartSize;
            main.startColor = WeakHitStartColor;
            //main.startColor = new ParticleSystem.MinMaxGradient()
        }
        else
        {
            main.startSize = StrongHitStartSize;
            main.startColor = StrongHitStartColor;
        }
    
        ParticlesHit.Emit(1);
    }

    public void ToggleWeaponTrailVisibility()
    {
        var pss = equippedWeapon.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in pss)
        {
            var emission = ps.emission;
            emission.enabled = !emission.enabled;
        }
    }

    #endregion

    #region CALLBACKS

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
        EmitHitImpact(attack);
        FX.Instance.DamageLabel(transform.position + Vector3.up, attack.Damage);
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
            FreezeAnimator();
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
    
    #endregion

    public void Equip(ItemData item)
    {
        Transform modelRoot = item.transform.Find("ModelRoot");
        if (modelRoot.childCount > 0)
        {
            var model = modelRoot.GetChild(0);
            Equip(model.gameObject, item.Stats);
        }
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

    public void FreezeAnimator()
    {
        GetComponent<FreezeAnimator>().Freeze();
    }

    // gambiarra 
    public void RefreshAnimator()
    {
        Avatar avatar = animator.avatar;
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        bool rootMotion = animator.applyRootMotion;
        RefreshAnimator(avatar, controller, rootMotion);
    }

    public void RefreshAnimator(Avatar avatar, RuntimeAnimatorController controller, bool rootMotion)
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
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();

        RefreshMaterials();

        OnRefreshAnimator?.Invoke(animator);
    }

    void RefreshMaterials()
    {
        List<Material> materials = new List<Material>();
        foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            materials.Add(r.material);
        }

        Materials = materials.ToArray();
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
