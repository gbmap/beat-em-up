﻿using UnityEngine;
using Catacumba.Data;
using Catacumba.Effects;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Data.Items;
using System;
using System.Linq;
using Frictionless;

namespace Catacumba.Entity
{

    public class CharacterCombat : CharacterComponentBase
    {
        // Used only for debugging.
        private struct AttackAttempt
        {
            public float Time;
            public EAttackType Type;
        }

        public LayerMask TargetLayer; 

        [HideInInspector] public bool IsOnCombo;

        public ParticleEffectConfiguration AttackEffect { get; private set; }
        public ParticleEffectConfiguration HitEffect { get; private set; }
        public CharacteristicWeaponizable Weapon { get; private set; }

        public bool IsOnHeavyAttack { get; set; }
        public bool CanAttack
        {
            get 
            {
                return Weapon && (!Health || (!Health.IsDead && !Health.IsBeingDamaged));
            }
        }

        public EAttackType LastAttackRequest { get; private set; }
        public AttackResult LastAttackData { get; private set; }
        public AttackResult LastDamageData { get; private set; }

        private AttackAttempt _lastAttack;

        ////////////////////////
        //  Callbacks

        public System.Action<EAttackType> OnRequestAttack;
        public System.Action<AttackResult[]> OnAttack;

        public System.Action OnComboStarted;
        public System.Action OnComboEnded;

        private CharacterHealth Health { get { return data.Components.Health; } }
        private CharacterMovementBase Movement { get { return data.Components.Movement; } }
        private CharacterAnimator Animator { get { return data.Components.Animator; } }

        protected override void Awake()
        {
            base.Awake();
            TargetLayer = 1 << LayerMask.NameToLayer("Player") 
                        | 1 << LayerMask.NameToLayer("Entities")
                        | 1 << LayerMask.NameToLayer("Projectiles");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            OnComboStarted += OnComboStartedCallback;
            OnComboEnded += OnComboEndedCallback;
            OnAttack += Cb_OnAttack;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            OnComboStarted -= OnComboStartedCallback;
            OnComboEnded -= OnComboEndedCallback;
            OnAttack -= Cb_OnAttack;

        }

        private void Cb_OnItemEquipped(InventoryEquipResult result)
        {
            SetupWeaponEquip(result.Params.Item);
        }

        private void SetupWeaponEquip(Item item)
        {
            var weapon = item.GetCharacteristics<CharacteristicWeaponizable>().FirstOrDefault();
            if (!weapon)
                return;

            Weapon = weapon;
            if (AttackEffect)
            {
                AttackEffect.Destroy(this);
                AttackEffect = null;
            }

            if (weapon.AttackEffect)
            {
                AttackEffect = weapon.AttackEffect;
                AttackEffect.Setup(this);
            }

            if (HitEffect)
            {
                HitEffect.Destroy(this);
                HitEffect = null;
            }
            
            if (weapon.HitEffect)
            {
                HitEffect = weapon.HitEffect;
                HitEffect.Setup(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AttackEffect?.Destroy(this);

            data.Stats.Inventory.OnItemEquipped -= Cb_OnItemEquipped;
        }

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();
            data.Stats.Inventory.OnItemEquipped += Cb_OnItemEquipped;
            if (data.IsConfigured)
            // If this is not the object's startup
            {
                Item weapon = data.Stats.Inventory.GetWeapon();
                SetupWeaponEquip(weapon);
                /*
                foreach (var slot in data.Stats.Inventory.Items)
                {
                    if (slot.Item == null) continue;

                    var weapons = slot.Item.GetCharacteristics<CharacteristicWeaponizable>();
                    if (weapons == null || weapons.Length == 0) continue;

                    SetupWeaponEquip(slot.Item);
                    break;
                }
                */
            }
        }

        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
            if (component is CharacterHealth)
            {
                var health = component as CharacterHealth;
                health.OnFall += OnFallCallback;
                health.OnDamaged += OnDamagedCallback;
            }

            else if (component is CharacterMovementWalkDodge)
            {
                (component as CharacterMovementWalkDodge).OnDodge += Cb_OnDodge;
            }
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterHealth)
            {
                var health = component as CharacterHealth;
                health.OnFall -= OnFallCallback;
                health.OnDamaged -= OnDamagedCallback;
            }

            else if (component is CharacterMovementWalkDodge)
            {
                (component as CharacterMovementWalkDodge).OnDodge -= Cb_OnDodge;
            }
        }

        public void RequestAttack(EAttackType type)
        {
            if (!CanAttack) return;

            LastAttackRequest = type;

            if (!Animator)
            {
                AttackImmediate(type);
                return;
            }

            OnRequestAttack?.Invoke(type);
        }

        public void AttackImmediate(EAttackType type)
        {
            if (!Weapon) return;

            _lastAttack.Time = Time.time;
            _lastAttack.Type = type;

            AttackResult[] results = Weapon.Attack(data, transform, type);
            EmitAttackEffect();

            if (results == null) return;
            OnAttack?.Invoke(results);

            LastAttackData = results[results.Length-1];
        }

        ////////////////////////////////////////
        //    CALLBACKS HANDLERS

        private void OnComboStartedCallback()
        {
            IsOnCombo = true;
        }

        private void OnComboEndedCallback()
        {
            IsOnCombo = false;
        }

        private void OnFallCallback()
        {
            OnComboEnded?.Invoke();
        }

        private void Cb_OnAttack(AttackResult[] results)
        {
            EmitHitEffects(results);
            if (data.BrainType == ECharacterBrainType.Input)
            {
                ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new Catacumba.Events.OnPlayerHit
                {
                    Attack = results[0]
                });
            }
        }


        private void OnDamagedCallback(AttackResult msg)
        {
            if (msg.CancelAnimation)
            {
                OnComboEnded?.Invoke();
            }

            if (data.BrainType == ECharacterBrainType.Input)
            {
                ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new Catacumba.Events.OnPlayerDamaged
                {
                    Attack = msg
                });
            }

            LastDamageData = msg;
        }

        private void OnRollCallback()
        {
            OnComboEnded?.Invoke();
        }

        private void Cb_OnDodge()
        {
            OnComboEnded?.Invoke();
        }

        /////////////////////////////////
        //      EFFECTS HANDLER

        private void EmitAttackEffect()
        {
            if (!AttackEffect) return;
            AttackEffect.EmitBurst(this, 1);
        }

        private void EmitHitEffects(AttackResult[] attacks)
        {
            if (!HitEffect) return;
            if (attacks == null) return;
            foreach (var result in attacks)
            {
                if (result == null) continue;

                Vector3 effectLocalPosition = HitEffect.CalculatePosition(result.Defender, HitEffect.LocalPosition);
                Vector3 position = result.Defender.transform.position + effectLocalPosition;
                HitEffect.EmitBurst(
                    this, 
                    1, 
                    position
                );
            }
        }

        public override string GetDebugString()
        {
            return "Is on combo: " + IsOnCombo + "\n" +
                   "Can attack: " + CanAttack;
        }

        /////////////////////////////////
        //      DEBUG

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            try
            {
                if (Time.time < _lastAttack.Time + 1f)
                {
                    if (Weapon && data)
                        Weapon.DebugDraw(data, _lastAttack.Type);
                }
                else
                {
                    if (Weapon && data)
                        Weapon.DebugDraw(data, EAttackType.Weak);
                }
            }
            catch { }
        }
    #endif

    }
}