using System;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using UnityEngine;
using Catacumba.Data;
using Catacumba.Effects;

namespace Catacumba.Entity
{
    public class MsgOnPlayerDied { public CharacterData player; }

    public class CharacterHealth : CharacterComponentBase
    {
        public ParticleEffectConfiguration HitEffect;
        public HealthEffectsConfiguration HealthEffects;
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

        private new Collider collider;

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
                HitEffect = data.CharacterCfg.View.DamageEffect;

            HitEffect?.Setup(this);
            shaderHitEffect = new HitEffect(this);

            if (!HealthEffects)
                HealthEffects = data.CharacterCfg.View.HealthQuad;
            
            if (HealthEffects)
            {
                HealthEffects.Setup(this);
                HealthEffects.SetHealth(this, data.Stats.HealthNormalized);
                HealthEffects.SetStamina(this, data.Stats.StaminaBar);
            }
        }

        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
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
            collider = GetComponent<Collider>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            OnFall += OnFallCallback;
            OnGetUp += OnGetUpAnimationEnd;

            data.Stats.OnStatsChanged += OnStatsChangedCallback;
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
                        if (data.BrainType == ECharacterBrainType.Input)
                            ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new MsgOnPlayerDied { player = data });

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

            data.Stats.OnStatsChanged -= OnStatsChangedCallback;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyEffects();
        }

        private void DestroyEffects()
        {
            HitEffect?.Destroy(this);
            HealthEffects?.Destroy(this);
        }

        ////////////////////////////////////////
        //      CALLBACKS

        private void OnStatsChangedCallback(CharacterStats stats)
        {
            // UpdateHealthQuad(stats.HealthNormalized, stats.StaminaBar);
            if (HealthEffects)
            {
                HealthEffects.SetHealth(this, stats.HealthNormalized);
                HealthEffects.SetStamina(this, stats.StaminaBar);
            }
        }

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

    }

}