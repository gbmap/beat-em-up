using UnityEngine;
using Catacumba.Data;
using Catacumba.Data.Items;
using System;
using Catacumba.Data.Items.Characteristics;
using System.Linq;
using Catacumba.Data.Character;

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
        CharacterMovementBase movement;
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

        GameObject equippedWeapon;

        ////////////////////////////////////////
        //      ANIMATOR PARAMETER HASHES

        int hashMoving        = Animator.StringToHash("Moving");
        int hashRoll          = Animator.StringToHash("Roll");
        int hashWeakAttack    = Animator.StringToHash("WeakAttack");
        int hashStrongAttack  = Animator.StringToHash("StrongAttack");
        int hashAttackType    = Animator.StringToHash("AttackType");
        int hashAttackTrigger = Animator.StringToHash("Attack");
        int hashDamaged       = Animator.StringToHash("Damaged");
        int hashKnockdown     = Animator.StringToHash("Knockdown");
        int hashNHits         = Animator.StringToHash("DamagedHits");
        int hashRecover       = Animator.StringToHash("Recovered");
        int hashCastSkill     = Animator.StringToHash("Cast");
        int hashUseSkill      = Animator.StringToHash("UseSkill");
        int hashSkillIndex    = Animator.StringToHash("SkillIndex");

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
            data.Stats.Inventory.OnItemEquipped += Cb_OnItemEquipped;
            data.Stats.Inventory.OnItemDropped += Cb_OnItemDropped;
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            data.Stats.Inventory.OnItemEquipped -= Cb_OnItemEquipped;
            data.Stats.Inventory.OnItemDropped -= Cb_OnItemDropped;
        }


        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
            if (component is CharacterMovementBase) 
            {
                movement = component as CharacterMovementBase;
                // movement.OnRoll += OnRollCallback;
            }

            else if (component is CharacterCombat)
            {
                combat = component as CharacterCombat;
                combat.OnRequestAttack += OnRequestCharacterAttackCallback;
            }
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterMovementBase) 
            {
                // movement.OnRoll -= OnRollCallback;
                movement = null; 
            }

            else if (component is CharacterCombat)
            {
                combat.OnRequestAttack -= OnRequestCharacterAttackCallback;
                combat = null;
            }
        }

        void Update()
        {
            if (movement)
            {
                if (movement.NavMeshAgent)
                    animator.SetBool(hashMoving, movement.Direction.sqrMagnitude > 0.0f && movement.CanMove);
                
                /*
                if (combat)
                    UpdateSmokeEmission();
                */
            }

    #if UNITY_EDITOR
            CheckDebugInput();
    #endif
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

        private void EmitHitImpact(AttackResult attack)
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

        private void Cb_OnItemEquipped(InventoryEquipResult result)
        {
            var weapon = result.Params.Item.GetCharacteristics<CharacteristicWeaponizable>().FirstOrDefault();
            if (!weapon)
            {
                Debug.Log("Not weapon");
                return;
            }

            if (!weapon.WeaponType.animatorController)
                return;

            UpdateAnimator(weapon.WeaponType.animatorController);
            AddItemToBone(result.Params.Item, result.Params.Slot);
        }

        private void Cb_OnItemDropped(InventoryDropResult result)
        {
            Item item = result.Item;
            ItemTemplate.Create(item, transform.position);

            RemoveItemFromBone(result.Params.Slot);
        }

        private void OnCharacterDamagedCallback(AttackResult attack)
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

        private void OnCharacterAttackCallback(AttackResult attack)
        {
            if (attack.Defender != null)
                FreezeAnimator();
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

        public void ResetAttackTrigger()
        {
            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);
        }

        public void FreezeAnimator()
        {
            GetComponent<FreezeAnimator>()?.Freeze();
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

        public void UpdateAnimator(RuntimeAnimatorController controller)
        {
            if (!animator)
                return; 

            animator.runtimeAnimatorController = controller;
            animator.SetInteger("BrainType", (int)data.BrainType);
        }

        private void AddItemToBone(Item item, BodyPart slot)
        {
            if (item.Model == null) return;

            var equippables = item.GetCharacteristics<CharacteristicEquippable>();
            if (!equippables.Any(e => e.Slots.Contains(slot)))
                return;

            Transform childBone = GetBone(gameObject, slot);
            if (!childBone)
                return;

            DestroyItemsInBone(childBone);

            GameObject model = Instantiate(item.Model, Vector3.zero, Quaternion.identity);
            model.transform.SetParent(childBone, true);
            model.transform.localPosition = slot.LocalPosition;
            model.transform.localRotation = Quaternion.Euler(slot.LocalRotationEuler);
        }

        private void RemoveItemFromBone(BodyPart slot)
        {
            Transform childBone = GetBone(gameObject, slot);
            if (!childBone)
            {
                Debug.LogErrorFormat("Couldn't find bone: {0}", slot.BoneName);
                return;
            }

            DestroyItemsInBone(childBone);
        }
        
        private static Transform GetBone(GameObject obj, BodyPart slot)
        {
            Transform childBone = obj.transform.GetFirstChildByNameRecursive(slot.BoneName);
            if (!childBone)
            {
                Debug.LogErrorFormat("Couldn't find bone: {0}", slot.BoneName);
                return null;
            }
            return childBone;
        }

        private static void DestroyItemsInBone(Transform childBone)
        {
            // Clean items that might already be equipped.
            for (int i = 0; i < childBone.childCount; i++)
            {
                Transform subObject = childBone.GetChild(i);
                if (subObject.GetComponent<Renderer>())
                    Destroy(subObject.gameObject);
            }
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

        void OnDamaged(AttackResult attack)
        {
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

            combat.AttackImmediate(type);
        }

    }
}