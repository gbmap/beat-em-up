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

        CharacterMovementBase movement;
        CharacterCombat combat;
        CharacterHealth health;

        private float AnimatorDefaultSpeed
        {
            get { return data.BrainType == ECharacterBrainType.Input ? 1.3f : animatorSpeed; }
        }

        private new Renderer renderer;

        public float animatorSpeed = 1f;

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
        }

        protected override void Start()
        {
            base.Start();

            renderer = GetComponentInChildren<Renderer>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            data.Stats.Inventory.OnItemEquipped -= Cb_OnItemEquipped;
            data.Stats.Inventory.OnItemDropped -= Cb_OnItemDropped;
        }

        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
            if (component is CharacterMovementBase) 
            {
                movement = component as CharacterMovementBase;
                if (movement is CharacterMovementWalkDodge)
                    (movement as CharacterMovementWalkDodge).OnDodge += OnRollCallback;
            }

            else if (component is CharacterCombat)
            {
                combat = component as CharacterCombat;
                combat.OnRequestAttack += OnRequestCharacterAttackCallback;
            }

            else if (component is CharacterHealth)
            {
                health = component as CharacterHealth;
                health.OnDamaged += Cb_OnDamaged;
            }
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterMovementBase) 
            {
                if (component is CharacterMovementWalkDodge)
                    (component as CharacterMovementWalkDodge).OnDodge -= OnRollCallback;

                movement = null; 
            }

            else if (component is CharacterCombat)
            {
                combat.OnRequestAttack -= OnRequestCharacterAttackCallback;
                combat = null;
            }

            else if (component is CharacterHealth)
            {
                health.OnDamaged -= Cb_OnDamaged;
                health = null;
            }
        }

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();

            animator = GetComponentInChildren<Animator>();
            animator.speed = AnimatorDefaultSpeed;
            animator.SetInteger("BrainType", (int)data.BrainType);

            data.Stats.Inventory.OnItemEquipped += Cb_OnItemEquipped;
            data.Stats.Inventory.OnItemDropped += Cb_OnItemDropped;
        }

        void Update()
        {
            if (movement)
            {
                if (movement.NavMeshAgent)
                    animator.SetBool(hashMoving, movement.Velocity.sqrMagnitude > 0.0f && movement.CanMove);
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

        public void EmitSmokeRadius()
        {
            movement?.EmitSmokeRadius();
        }

        #endregion

        #region CALLBACKS 

        private void Cb_OnItemEquipped(InventoryEquipResult result)
        {
            AddItemToBone(result.Params.Item, result.Params.Slot);

            var weapon = result.Params.Item.GetCharacteristics<CharacteristicWeaponizable>().FirstOrDefault();
            if (!weapon)
                return;

            if (!weapon.WeaponType.animatorController)
                return;

            UpdateAnimator(weapon.WeaponType.animatorController);
        }

        private void Cb_OnItemDropped(InventoryDropResult result)
        {
            /*
            Item item = result.Item;
            ItemTemplate.Create(item, transform.position);
            */

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



        private void OnRecoverCallback()
        {
        }

        private void OnRollCallback()
        {
            //animator.speed = AnimatorDefaultSpeed;
            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);
            animator.SetTrigger(hashRoll);
        }

        private void Cb_OnDamaged(AttackResult attack)
        {
            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);

            if (attack.CancelAnimation || attack.Dead)
            {
                animator.SetInteger(hashNHits, attack.HitNumber);
                animator.SetTrigger( (attack.Knockdown || attack.Dead) ? hashKnockdown : hashDamaged);
            }

            // EmitHitImpact(attack);
            // FX.Instance.DamageLabel(transform.position + Vector3.up, attack.Damage);
        }
        
        void OnRecover()
        {
            animator.SetTrigger(hashRecover);
        }

        void OnDodge()
        {
            animator.SetTrigger(hashRoll);
        }

        #endregion

        public void ResetAttackTrigger()
        {
            animator.ResetTrigger(hashAttackTrigger);
            animator.ResetTrigger(hashAttackTrigger);
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
            var equippable = equippables.FirstOrDefault(e => e.Slots.Any(s => s.BodyPart == slot));
            if (!equippable)
                return;

            if (string.IsNullOrEmpty(slot.BoneName))
                return;

            Transform childBone = GetBone(gameObject, slot);
            if (!childBone)
                return;

            DestroyItemsInBone(childBone);

            CharacteristicEquippable.SlotData slotData = equippable.GetSlot(slot);

            GameObject model = Instantiate(item.Model, Vector3.zero, Quaternion.identity);
            model.transform.SetParent(childBone, true);

            Vector3 position = Vector3.zero;
            Vector3 rotation = Vector3.zero;
            if (slotData.ModelOffset)
            {
                position = slotData.ModelOffset.Position;
                rotation = slotData.ModelOffset.Rotation;
            }

            // This is used to fix difference in scales from different model packs
            // in polygon's models.
            position.x *= model.transform.localScale.x;
            position.y *= model.transform.localScale.y;
            position.z *= model.transform.localScale.z;

            model.transform.localPosition = position;
            model.transform.localRotation = Quaternion.Euler(rotation);

            CharacterViewConfiguration.SetLayerRecursive(model, data.gameObject.layer);
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

        public void AnimPlayWoosh() 
        {
            if (movement)
            {
                Vector3 dir = movement.transform.forward;
                if (movement) movement.ApplySpeedBump(dir, movement.SpeedBumpForce);
            }

            // SoundManager.Instance.PlayWoosh(transform.position);
        }

        public void Attack(EAttackType type)
        {
            if (!combat) return;
            if (!combat.CanAttack) return;

            combat.AttackImmediate(type);
        }

    }
}