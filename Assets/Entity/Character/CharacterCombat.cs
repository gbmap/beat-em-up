using UnityEngine;
using Catacumba.Data;
using Catacumba.Effects;

namespace Catacumba.Entity
{
    public class CharacterCombat : CharacterComponentBase
    {
        public ParticleEffectConfiguration AttackEffect;

        int _nComboHits;

        CharacterHealth health;
        CharacterMovement movement;
        CharacterAnimator animator;

        [HideInInspector] public bool IsOnCombo;

        public bool IsOnHeavyAttack { get; set; }
        public bool CanAttack
        {
            get 
            {
                return !health || (!health.IsDead && !health.IsBeingDamaged);
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

        Vector3 GetAttackColliderPosition()
        {
            return transform.position + (transform.forward*1.25f + Vector3.up);
        }

        Vector3 GetAttackColliderSize(EAttackType type)
        {
            float weaponScale = 0f;
            Vector3 attackColliderSize = (Vector3.one * 0.65f + Vector3.right * 0.65f); 
            
            if (data.Stats.Inventory.HasEquip(EInventorySlot.Weapon))
            {
                weaponScale = data.Stats.Inventory[EInventorySlot.Weapon].WeaponColliderScaling;
            }

            return attackColliderSize * (type == EAttackType.Weak ? 1.0f : 1.5f) + Vector3.one * weaponScale;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            OnComboStarted += OnComboStartedCallback;
            OnComboEnded += OnComboEndedCallback;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();

            OnComboStarted -= OnComboStartedCallback;
            OnComboEnded -= OnComboEndedCallback;

            if (health)
            {
                health.OnFall -= OnFallCallback;
                health.OnDamaged -= OnDamagedCallback;
            }

            if (movement)
            {
                movement.OnRoll -= OnRollCallback;
            }
        }

        protected void OnDestroy()
        {
            AttackEffect?.Destroy(this);
        }

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();

            if (!AttackEffect)
                AttackEffect = data.CharacterCfg.View.AttackEffect;
            AttackEffect?.Setup(this);
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

        private void OnRollCallback()
        {
            OnComboEnded?.Invoke();
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

        /*
        *  CHAMADO PELO ANIMATOR!!!1111
        */
        public void AttackImmediate(EAttackType type)
        {
            CharacterAttackData[] results = CombatManager.Attack(
                data,
                type, 
                GetAttackColliderPosition(), 
                GetAttackColliderSize(type), 
                transform.rotation
            );

            EmitAttackEffect();

            if (results == null) return;
            OnAttack?.Invoke(results);
            LastAttackData = results[results.Length-1];
        }

        private void EmitAttackEffect()
        {
            if (!AttackEffect) return;
            AttackEffect.EmitBurst(this, 1);
        }

        public override string GetDebugString()
        {
            return "Is on combo: " + IsOnCombo + "\n" +
                   "Can attack: " + CanAttack;
        }

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            try
            {
                if (Time.time < LastAttackData.Time + 1f)
                {
                    Gizmos.color = Color.red;
                    Gizmos.matrix = Matrix4x4.TRS(GetAttackColliderPosition(), transform.rotation, GetAttackColliderSize(LastAttackData.Type));
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
            }
            catch { }
        }
    #endif

    }
}