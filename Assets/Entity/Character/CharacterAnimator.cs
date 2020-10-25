using UnityEngine;
using Catacumba.Data;

namespace Catacumba.Entity
{
    public class CharacterAnimator : CharacterComponentBase
    {
        public Animator animator;

        public bool IgnoreWeaponAnimations = false;

        public Vector3 RealCharacterPosition {
            get
            {
                Vector3 delta = ModelInfo.HipsBone.Bone.position - transform.position;
                delta.y = 0f;
                return transform.position + delta;
            }
        }

        //CharacterData data;
        CharacterMovement movement;
        CharacterCombat combat;
        CharacterModelInfo modelInfo;
        public CharacterModelInfo ModelInfo
        {
            get { return modelInfo ?? (modelInfo = GetComponentInChildren<CharacterModelInfo>()); }
        }

        private float AnimatorDefaultSpeed
        {
            get { return data.BrainType == ECharacterBrainType.Input ? 1.3f : animatorSpeed; }
        }

        private new Renderer renderer;

        public float animatorSpeed = 1f;

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

        [Space]
        [Header("FX Slash")]
        public ParticleSystem ParticlesSlash;
        public ParticleSystem.MinMaxGradient UnarmedGradient;
        public ParticleSystem.MinMaxCurve UnarmedStartSize;
        public float UnarmedDistanceFromCharacter = 0.7f;

        // ==== MOVEMENT
        int hashMoving = Animator.StringToHash("Moving");
        int hashRoll = Animator.StringToHash("Roll");

        // ===== COMBAT
        int hashWeakAttack = Animator.StringToHash("WeakAttack");
        int hashStrongAttack = Animator.StringToHash("StrongAttack");

        int hashAttackType = Animator.StringToHash("AttackType");
        int hashAttackTrigger = Animator.StringToHash("Attack");

        GameObject equippedWeapon;

        // ====== HEALTH
        int hashDamaged = Animator.StringToHash("Damaged");
        int hashKnockdown = Animator.StringToHash("Knockdown");
        int hashNHits = Animator.StringToHash("DamagedHits");
        int hashRecover = Animator.StringToHash("Recovered");
        int hashCastSkill = Animator.StringToHash("Cast");

        // ======= SKILL
        int hashUseSkill = Animator.StringToHash("UseSkill");
        int hashSkillIndex = Animator.StringToHash("SkillIndex");

        // ============ EVENTS
        public System.Action<Animator> OnRefreshAnimator;

        public System.Action<CharacterAnimator> OnStartUsingSkill;
        public System.Action<CharacterAnimator> OnEndUsingSkill;

        #region MONOBEHAVIOUR

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();

            modelInfo = GetComponent<CharacterModelInfo>();
            renderer = GetComponentInChildren<Renderer>();

            //SetupSlashParticles(null);
        }

        protected override void Start()
        {
            base.Start();

            animator = GetComponentInChildren<Animator>();
            animator.speed = AnimatorDefaultSpeed;
            animator.SetInteger("BrainType", (int)data.BrainType);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
            if (component is CharacterMovement) 
            {
                movement = component as CharacterMovement;
                movement.OnRoll += OnRollCallback;
            }

            else if (component is CharacterCombat)
            {
                combat = component as CharacterCombat;
                combat.OnRequestAttack += OnRequestCharacterAttackCallback;
                combat.OnRequestSkillUse += OnRequestSkillUseCallback;
                combat.OnAttack += OnCharacterAttackCallback;
            }
        }

        protected override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterMovement) 
            {
                movement.OnRoll -= OnRollCallback;
                movement = null; 
            }

            else if (component is CharacterCombat)
            {
                combat.OnRequestAttack -= OnRequestCharacterAttackCallback;
                combat.OnRequestSkillUse -= OnRequestSkillUseCallback;
                combat.OnAttack -= OnCharacterAttackCallback;
                combat = null;
            }
        }

        void Update()
        {
            if (movement)
            {
                if (movement.NavMeshAgent)
                    animator.SetBool(hashMoving, movement.Direction.sqrMagnitude > 0.0f && movement.CanMove);
                
                if (combat)
                    UpdateSmokeEmission();
            }

    #if UNITY_EDITOR
            CheckDebugInput();
    #endif
        }

        private void UpdateSmokeEmission()
        {
            if (!ParticlesSmoke) return;

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
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.75f);
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

            float timeFactor = Mathf.Max(0f, ((Time.time - timeOfDeath)));
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
        }

        private void OnRequestCharacterAttackCallback(EAttackType type)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);

            if (clips.Length > 0)
            {
                AnimationClip clip = clips[0].clip;
                if (clip.name.Contains("Heavy")) // VAI DA MERDA VAI
                {
                    if (info.normalizedTime < 0.33f) return;
                }
            }

            animator.SetInteger(hashAttackType, (int)type);
            animator.SetTrigger(hashAttackTrigger);
            // animator.SetTrigger(type == EAttackType.Weak ? hashWeakAttack : hashStrongAttack);
        }

        private void OnRequestSkillUseCallback(BaseSkill obj)
        {
            animator.SetTrigger(hashCastSkill);
        }

        private void OnCharacterAttackCallback(CharacterAttackData attack)
        {
            if (attack.Defender != null)
            {
                FreezeAnimator();
            }

            if (!ParticlesSlash) return;
            ParticlesSlash.Emit(new ParticleSystem.EmitParams()
            {
                velocity = transform.forward,
            }, 1);
        }

        private void OnRecoverCallback()
        {
        }

        private void OnRollCallback()
        {
            
            animator.speed = AnimatorDefaultSpeed;
            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);
            animator.SetTrigger(hashRoll);
        }
        
        #endregion

        public void Equip(ItemData item)
        {
            Transform modelRoot = item.transform.Find("ModelRoot");
            if (modelRoot.childCount > 0)
            {
                var model = modelRoot.GetChild(0);
                Equip(model.gameObject, item.itemConfig);
            }
        }

        public void Equip(ItemConfig cfg)
        {
            Equip(Instantiate(cfg.Prefab), cfg);
        }

        public void Equip(GameObject model, ItemConfig itemCfg)
        {
            var item = itemCfg.Stats;

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
                else if (item.WeaponType == EWeaponType.Bow)
                {
                    handBone = ModelInfo.LeftHandBone.Bone.Find("WeaponHolder");
                    rotation = Quaternion.Euler(204f, 87f, 122f);
                    position = new Vector3(-0.067f, -0.067f, -0.018f);
                }
                else
                {
                    handBone = ModelInfo.LeftHandBone.Bone.Find("WeaponHolder");
                    rotation = Quaternion.Euler(90f, 0f, 0f);
                }

                if (itemCfg.OverrideHand)
                {
                    CharacterModelInfo.TransformBone transformBone = null;
                    if (itemCfg.Hand == EWeaponHand.Left)
                    {
                        transformBone = ModelInfo.LeftHandBone;
                        rotation = Quaternion.Euler(-131f, -81f, -111f);
                        position = new Vector3(-0.055f, -0.025f, 0.346f);
                    }
                    else
                    {
                        transformBone = ModelInfo.RightHandBone;
                        rotation = Quaternion.Euler(-26.666f, 85.7f, -117f);
                        position = new Vector3(-0.017f, 0.12f, 0.072f);
                    }

                    handBone = transformBone.Bone.Find("WeaponHolder");
                }
            }

            model.transform.SetParent(handBone, true);
            //model.transform.parent = handBone;
            model.transform.localRotation = rotation;
            model.transform.localPosition = position;
            //model.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            if (model.transform.localScale.sqrMagnitude <= new Vector3(1f, 1f, 1f).sqrMagnitude)
            {
                model.transform.localScale = Vector3.one;
            }

            if (item.ItemType == EItemType.Equip)
            {
                if (item.Slot == EInventorySlot.Weapon)
                {
                    if (!IgnoreWeaponAnimations)
                    {
                        RefreshAnimator(false);
                    }

                    SetupSlashParticles(itemCfg);
                }
            }
        }

        private void SetupSlashParticles(ItemConfig itemCfg)
        {
            Gradient targetSlashColors = UnarmedGradient.gradient;
            ParticleSystem.MinMaxCurve targetSize = UnarmedStartSize;
            float distance = UnarmedDistanceFromCharacter;

            if (itemCfg != null && itemCfg.CustomSlashColors)
            {
                // atualizar gradiente 
                targetSize = itemCfg.StartSize;
                distance = itemCfg.DistanceFromCharacter;
                targetSlashColors = itemCfg.SlashColors;
            }

            var main = ParticlesSlash.main;
            main.startSize = targetSize;

            var col = ParticlesSlash.colorOverLifetime;
            col.color = new ParticleSystem.MinMaxGradient(targetSlashColors);

            ParticlesSlash.transform.localPosition = new Vector3(0f, ParticlesSlash.transform.localPosition.y, distance);
        }

        public void UnEquip(EInventorySlot slot)
        {
            if (slot != EInventorySlot.Weapon)
            {
                return;
            }

            if (equippedWeapon != null)
            {
                if (!IgnoreWeaponAnimations)
                {
                    RefreshAnimator(false);
                }

                Destroy(equippedWeapon);
            }
        }

        public void ResetAttackTrigger()
        {
            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);
        }

        public void FreezeAnimator()
        {
            GetComponent<FreezeAnimator>().Freeze();
        }

        // gambiarra 
        public void RefreshAnimator(bool destroyCurrentAnimator = true)
        {
            Avatar avatar = animator.avatar;
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            bool rootMotion = animator.applyRootMotion;
            RefreshAnimator(avatar, controller, rootMotion, destroyCurrentAnimator);
        }

        public void RefreshAnimator(Avatar avatar, RuntimeAnimatorController controller, bool rootMotion, bool destroyCurrentAnimator = true)
        {
            if (avatar == null)
            {
                avatar = animator.avatar;
            }

            if (destroyCurrentAnimator)
            {
                DestroyImmediate(animator);
                animator = gameObject.AddComponent<Animator>();
            }
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.SetInteger("BrainType", (int)data.BrainType);
            modelInfo = GetComponentInChildren<CharacterModelInfo>();
            modelInfo?.UpdateBones();
            renderer = GetComponentInChildren<SkinnedMeshRenderer>();

            OnRefreshAnimator?.Invoke(animator);
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
        
        public void UseSkill(int index)
        {
            animator.SetInteger(hashSkillIndex, index);
            animator.SetTrigger(hashUseSkill);
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

        void OnDamaged(CharacterAttackData attack)
        {
            if (attack.Poised && !attack.Dead)
            {
                return;
            }

            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);

            if (attack.CancelAnimation || attack.Dead)
            {
                animator.SetInteger(hashNHits, attack.HitNumber);
                animator.SetTrigger( (attack.Knockdown || attack.Dead) ? hashKnockdown : hashDamaged);
            }

            EmitHitImpact(attack);
            FX.Instance.DamageLabel(transform.position + Vector3.up, attack.Damage);
        }

        void OnRecover()
        {
            animator.SetTrigger(hashRecover);
        }

        void OnDodge()
        {
            animator.SetTrigger(hashRoll);
        }

#endif

        public void AnimPlayWoosh()
        {
            if (movement)
            {
                Vector3 dir = movement.transform.forward;
                if (movement) movement.ApplySpeedBump(dir, movement.SpeedBumpForce);
            }

            SoundManager.Instance.PlayWoosh(transform.position);
        }

        public void Attack(EAttackType type)
        {
            if (!combat) return;
            if (!combat.CanAttack) return;

            combat.AttackImmediate(new CharacterAttackData(type, gameObject, 0));
        }

    }
}