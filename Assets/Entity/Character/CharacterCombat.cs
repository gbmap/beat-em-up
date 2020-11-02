using UnityEngine;
using Catacumba.Data;
using Catacumba.Effects;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Data.Items;
using System;
using System.Linq;

namespace Catacumba.Entity
{
    public class CharacterCombat : CharacterComponentBase
    {
        private int _nComboHits;

        private CharacterHealth health;
        private CharacterMovement movement;
        private CharacterAnimator animator;

        [HideInInspector] public bool IsOnCombo;

        public ParticleEffectConfiguration AttackEffect { get; private set; }
        public ParticleEffectConfiguration HitEffect { get; private set; }
        public CharacteristicWeaponizable Weapon { get; private set; }

        public bool IsOnHeavyAttack { get; set; }
        public bool CanAttack
        {
            get 
            {
                return Weapon && (!health || (!health.IsDead && !health.IsBeingDamaged));
            }
        }

        public CharacterAttackData LastAttackData { get; private set; }
        public CharacterAttackData LastDamageData { get; private set; }

        ////////////////////////
        //  Callbacks

        public System.Action<EAttackType> OnRequestAttack;
        public System.Action<CharacterAttackData[]> OnAttack;

        public System.Action OnComboStarted;
        public System.Action OnComboEnded;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            OnComboStarted += OnComboStartedCallback;
            OnComboEnded += OnComboEndedCallback;

            data.Stats.Inventory.OnItemEquipped += Cb_OnItemEquipped;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            OnComboStarted -= OnComboStartedCallback;
            OnComboEnded -= OnComboEndedCallback;

            data.Stats.Inventory.OnItemEquipped -= Cb_OnItemEquipped;
        }

        private void Cb_OnItemEquipped(InventoryEquipResult result)
        {
            var weapon = result.Params.Item.GetCharacteristics<CharacteristicWeaponizable>().FirstOrDefault();
            if (!weapon)
            {
                Debug.Log("Not weapon");
                return;
            }

            Weapon = weapon;
            if (AttackEffect)
                AttackEffect.Destroy(this);

            if (weapon.AttackEffect)
            {
                AttackEffect = weapon.AttackEffect;
                AttackEffect.Setup(this);
            }

            if (HitEffect)
                HitEffect.Destroy(this);
            
            if (weapon.HitEffect)
            {
                HitEffect = weapon.HitEffect;
                HitEffect.Setup(this);
            }
        }

        protected void OnDestroy()
        {
            AttackEffect?.Destroy(this);
        }

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();
        }

        protected override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
            if (component is CharacterHealth)
            {
                health = component as CharacterHealth;
                health.OnFall += OnFallCallback;
                health.OnDamaged += OnDamagedCallback;
            }

            else if (component is CharacterMovement)
            {
                movement = component as CharacterMovement;
                movement.OnRoll += OnRollCallback;
            }

            else if (component is CharacterAnimator)
            {
                animator = component as CharacterAnimator;
            }
        }

        protected override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterHealth)
            {
                health.OnFall -= OnFallCallback;
                health.OnDamaged -= OnDamagedCallback;
                health = null;
            }

            else if (component is CharacterMovement)
            {
                movement.OnRoll -= OnRollCallback;
                movement = null;
            }

            else if (component is CharacterAnimator)
            {
                animator = null;
            }
        }


        public void RequestAttack(EAttackType type)
        {
            if (!CanAttack) return;
            if (!animator)
            {
                AttackImmediate(type);
                return;
            }

            OnRequestAttack?.Invoke(type);
        }

        public void AttackImmediate(EAttackType type)
        {
            if (!Weapon) return;

            CharacterAttackData[] results = Weapon.Attack(data, type);
            EmitAttackEffect();

            if (results == null) return;
            EmitHitEffects(results);
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
            _nComboHits = 0;
        }

        private void OnFallCallback()
        {
            OnComboEnded?.Invoke();
        }

        private void OnDamagedCallback(CharacterAttackData msg)
        {
            if (msg.CancelAnimation)
            {
                OnComboEnded?.Invoke();
            }

            LastDamageData = msg;
        }

        private void OnRollCallback()
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

        private void EmitHitEffects(CharacterAttackData[] attacks)
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
                if (Time.time < LastAttackData.Time + 1f)
                {
                    if (Weapon && data)
                        Weapon.DebugDraw(data, LastAttackData.Type);
                }
            }
            catch { }
        }
    #endif

    }
}