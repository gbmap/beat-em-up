using UnityEngine;
using Catacumba.Data;
using Catacumba.Effects;

namespace Catacumba.Entity
{
    public class CharacterCombat : CharacterComponentBase
    {
        public ParticleEffectConfiguration AttackEffect;

        int _nComboHits;
        BaseSkill skillBeingCasted;

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
        public System.Action<CharacterAttackData> OnAttack;

        public System.Action<BaseSkill> OnRequestSkillUse;
        public System.Action<BaseSkill> OnSkillUsed;

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

            LastDamageData = new CharacterAttackData
            {
                Time = float.NegativeInfinity
            };
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
                AttackImmediate(new CharacterAttackData(type, gameObject, 0));
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
        public void AttackImmediate(CharacterAttackData attack)
        {
            CombatManager.Attack(
                ref attack, 
                GetAttackColliderPosition(), 
                GetAttackColliderSize(attack.Type), 
                transform.rotation
            );

            EmitAttackEffect();
            OnAttack?.Invoke(attack);

            LastAttackData = attack;
        }

        private void EmitAttackEffect()
        {
            if (!AttackEffect) return;
            AttackEffect.EmitBurst(this, 1);
        }

        /*
        * Skills
        * */
        public void AnimUseWeaponSkill(int index)
        {
            /*
            ItemStats weapon = data.Stats.Inventory[EInventorySlot.Weapon];
            if (weapon.Skills == null)
            {
                Debug.LogWarning("No weapon skills found. This shouldn't be happening.");
                return;
            }

            SkillData skill = weapon.Skills[index];
            UseSkill(skill);
            */
        }

        public void AnimUseCharacterSkill(int index)
        {
            // TODO 
            //SkillData skill = data.CharacterSkills[index];
            //UseSkill(skill);
        }

        private void UseSkill(SkillData s)
        {
            if (s.gameObject == null) // objeto foi destruído por algum motivo, possivelmente o jogo foi ganho?
            {
                return;
            }

            // hack pra determinar se é um prefab
            if (s.gameObject.scene.rootCount == 0)
            {
                var obj = Instantiate(s.gameObject, transform.position + transform.forward * s.Offset.z, transform.rotation);
                s = obj.GetComponent<SkillData>();
                s.Caster = data;
            }
            s.Cast();
        }
        
        public void RequestSkillUse(BaseSkill skill)
        {
            skillBeingCasted = skill;
            OnRequestSkillUse?.Invoke(skill);
        }

        public void UseSkill(int index)
        {
            //animator.UseSkill(index);
        }

        public void AnimSkillUsed()
        {
            // fazer algo com a skill sendo castada.
            OnSkillUsed?.Invoke(skillBeingCasted);
            skillBeingCasted = null;
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