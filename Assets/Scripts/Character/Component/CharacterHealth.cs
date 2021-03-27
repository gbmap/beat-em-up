using UnityEngine;
using Catacumba.Effects;

namespace Catacumba.Entity
{
    public class MsgOnPlayerDied { public CharacterData player; }

    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterHealth : CharacterComponentBase
    {
        public ParticleEffectConfiguration HitEffect;
        private HitEffect shaderHitEffect; 

        //////////////////////////// 
        //     EVENTS 

        public System.Action<AttackResult> OnDamaged;
        public System.Action OnFall;
        public System.Action OnRecover;
        public System.Action OnGetUp;
        public System.Action<CharacterHealth> OnDeath;

        //////////////////////////// 
        //      CALLBACKS

        public int Health { get { return data.Stats.Health; } }
        public float HealthNormalized { get { return data.Stats.HealthNormalized; } }
        public bool IsDead { get { return Health <= 0; } }
        public bool IsOnGround { get; private set; }
        [HideInInspector] public bool IsBeingDamaged; 

        public float LastHit { get; private set; }
        public AttackResult LastHitData { get; private set; }

        private new CapsuleCollider collider;

        private float recoverTimer;
        private float recoverCooldown = 2f;

        ///////////////////////////////////
        //          OVERRIDES

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();
            SetupEffects();
        }

        private void SetupEffects()
        {
            if (!HitEffect)
                HitEffect = data.ConfigurationView.DamageEffect;

            HitEffect?.Setup(this);
            shaderHitEffect = new HitEffect(this);
        }

        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);

            if (component is CharacterMovementWalkDodge)
            {
                CharacterMovementWalkDodge movement = (component as CharacterMovementWalkDodge);
                movement.OnDodge += Cb_OnDodge;
                movement.OnDodgeEnded += Cb_OnDodgeEnded;
            }
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterMovementWalkDodge)
            {
                CharacterMovementWalkDodge movement = (component as CharacterMovementWalkDodge);
                movement.OnDodge -= Cb_OnDodge;
                movement.OnDodgeEnded -= Cb_OnDodgeEnded;
            }
        }

        ////////////////////////////////////////
        //          INTERFACE

        public void TakeDamage(AttackResult attack)
        {
            if (IsOnGround /* && characterMovement.IsOnAir */)
                return;

            LastHit = Time.time;
            LastHitData = attack;

            if (HitEffect)
                HitEffect.EmitBurst(this, 20);
            shaderHitEffect.OnHit();

            OnDamaged?.Invoke(attack);

            if (IsDead)
            {
                collider.enabled = false;
                OnDeath?.Invoke(this);

                if (!data.Components.Animator)
                    Destroy(gameObject);
            }
        }

        ////////////////////////////////////////
        //        LIFECYCLE

        protected override void Awake()
        {
            base.Awake();
            collider = GetComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = Vector3.up;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            OnFall += OnFallCallback;
            OnGetUp += OnGetUpAnimationEnd;

        }

        private void Update()
        {
            if (Time.time > LastHit + 2f) // TODO: especificar o tempo pra reiniciar o poise
            {
                data.Stats.CurrentStamina = data.Stats.Stamina;
                // UpdatePoise(1f);
            }

            // timer pra se recuperar
            if (IsOnGround && recoverTimer > 0f)
            {
                recoverTimer -= Time.deltaTime;
                if (recoverTimer < 0f)
                {
                    if (IsDead)
                    {
                        /*
                        if (data.BrainType == ECharacterBrainType.Input)
                            ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new MsgOnPlayerDied { player = data });
                        */
                        Destroy(gameObject);
                    }
                    else
                        OnRecover?.Invoke();
                }
            }

            shaderHitEffect?.Update();
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            OnFall -= OnFallCallback;
            OnGetUp -= OnGetUpAnimationEnd;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyEffects();
        }

        private void DestroyEffects()
        {
            HitEffect?.Destroy(this);
        }

        ////////////////////////////////////////
        //      CALLBACKS

        public void OnGetUpAnimationEnd()
        {
            IsOnGround = false;
            collider.enabled = true;
        }

        private void OnFallCallback()
        {
            collider.enabled = false;
            IsOnGround = true;
            recoverTimer = recoverCooldown;
            if (IsDead)
            {
                recoverTimer *= 2f;
            }
        }

        private void Cb_OnDodge()
        {
            collider.enabled = false;
        }

        private void Cb_OnDodgeEnded()
        {
            collider.enabled = true;
        }

    }

}