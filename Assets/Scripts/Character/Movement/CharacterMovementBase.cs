using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Catacumba.Effects;

namespace Catacumba.Entity
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class CharacterMovementBase : CharacterComponentBase
    {
        public ParticleEffectConfiguration MovementEffect;

        public Vector3 Direction;
        public float SpeedBumpForce = 0.9f;

        public Vector3 Velocity { get { return brainType == ECharacterBrainType.AI ? NavMeshAgent.velocity : Direction; } }

        public NavMeshAgent NavMeshAgent { get; private set; }
        public bool NavAgentValid { get { return NavMeshAgent && NavMeshAgent.enabled; } }

        public bool CanMove { get { return true; } }
        public bool IsBeingMoved { get { return speedBumpT > 0f; } }
        public Vector3 SpeedBumpDir { get; private set; }

        private bool isAgentStopped;
        public bool IsAgentStopped
        {
            get { return !CanMove || isAgentStopped; }
            set { isAgentStopped = value; }
        }

        // references
        private ECharacterBrainType brainType { get { return data.BrainType; } }

        private CharacterHealth health;

        protected Vector3 forward; // where the character is facing

        // speed bump
        private float speedBumpT;
        private const float speedBumpScale = 7f;

        protected virtual void UpdateEffect()
        {
            if (!MovementEffect) return;

            MovementEffect.SetEmission(this, IsBeingMoved);
            bool emission = MovementEffect.IsEmitting(this);
            if (emission)
                MovementEffect.PointSystemTowards(this, -SpeedBumpDir);

            /*
             DONT DELETE THIS WILL BE USED EVENTUALLY 

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
            */
        }

        protected override void Awake()
        {
            base.Awake();
            NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Update()
        {
            NavMeshAgent.enabled = GetNavMeshEnabled(health, speedBumpT);
            NavMeshAgent.isStopped = IsAgentStopped;

            Vector3 velocity = MoveNavAgent();
            UpdateFacingDirection(velocity);
            UpdateEffect();
        }

        private void UpdateFacingDirection(Vector3 velocity)
        {
            forward = GetForwardVector(velocity.normalized);
            transform.LookAt(transform.position + forward);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MovementEffect?.Destroy(this);
        }

        private bool GetNavMeshEnabled(CharacterHealth health, float speedBump)
        {
            bool ret = true;
            if (health)
            {
                if (health.IsOnGround)
                    return speedBump > 0f;
            }

            return ret;
        }

        private Vector3 MoveNavAgent()
        {
            UpdateSpeedBump();

            if (!NavAgentValid)
                return Vector3.zero;

            Vector3 velocity = Vector3.zero;
            Vector3 direction = Direction.normalized;

            bool updatedVelocity = false;
            velocity = UpdateVelocity(velocity, ref updatedVelocity);

            bool isPlayer = brainType == ECharacterBrainType.Input;
            if (isPlayer || IsBeingMoved)
                NavMeshAgent.Move(velocity * Time.deltaTime);

            return velocity;
        }

        protected virtual Vector3 UpdateVelocity(Vector3 velocity, ref bool updatedValue)
        {
            updatedValue = IsBeingMoved;
            if (updatedValue)
                velocity = UpdateVelocityWithBump(velocity);
            return velocity;
        }

        private Vector3 UpdateVelocityWithBump(Vector3 vel)
        {
            float t = 1f - speedBumpT;
            return speedBumpScale * SpeedBumpDir * Mathf.Pow(-t + 1f, 3f);
        }

        protected virtual Vector3 GetForwardVector(Vector3 dir)
        {
            return Vector3.Slerp(forward, dir, 0.5f * Time.deltaTime * 30f).normalized;
        }

        private void UpdateSpeedBump()
        {
            speedBumpT = Mathf.Max(0, speedBumpT - Time.deltaTime * 5f);
        }

        public override void OnConfigurationEnded()
        {
            base.OnConfigurationEnded();
            SetupEffect();
        }

        private void SetupEffect()
        {
            if (!MovementEffect)
                MovementEffect = data.CharacterCfg.View.MovementEffect;

            MovementEffect?.Setup(this);
        }

        public override void OnComponentAdded(CharacterComponentBase component)
        {
            base.OnComponentAdded(component);
            if (component is CharacterHealth)
            {
                health = component as CharacterHealth;
                health.OnDamaged += OnDamagedCallback;
                health.OnFall += OnFallCallback;
                health.OnGetUp += OnGetUpCallback;
            }
        }

        public override void OnComponentRemoved(CharacterComponentBase component)
        {
            base.OnComponentRemoved(component);
            if (component is CharacterHealth)
            {
                health.OnDamaged -= OnDamagedCallback;
                health.OnFall -= OnFallCallback;
                health.OnGetUp -= OnGetUpCallback;
                health = null;
            }
        }

        //////////////////////////////////
        //      CALLBACKS

        private void OnDamagedCallback(AttackResult attack)
        {
            if (attack.CancelAnimation)
            {
                var lookAt = attack.Attacker.transform.position;
                lookAt.y = transform.position.y;
                transform.LookAt(lookAt);
            }

            // if (attack.Knockdown && attack.AttackerStats != data.Stats)
            ApplySpeedBump(attack.Attacker.transform.forward, GetSpeedBumpForce(attack));
        }

        private void OnGetUpCallback()
        {
            NavMeshAgent.enabled = true;
        }

        private void OnFallCallback()
        {
            NavMeshAgent.enabled = false;
        }

        ////////////////////////////////
        //      UTILITY

        public void ApplySpeedBump(Vector3 direction, float force)
        {
            //transform.LookAt(transform.position + direction);
            speedBumpT = 1f;
            SpeedBumpDir = direction.normalized * force;
        }

        public float GetSpeedBumpForce(AttackResult attack, bool ignoreSpeedBump = false)
        {
            if (ignoreSpeedBump) return 0f;

            float modifier = (attack.Type == EAttackType.Weak ? 1f : 2f);
            modifier = attack.Knockdown ? 3f : modifier;

            return SpeedBumpForce * modifier;
        }

        public override string GetDebugString()
        {
            return "NavMeshAgentValid: " + NavAgentValid +
                    //"\nIsOnCombo: " + combat.IsOnCombo +
                    "\nIsBeingMoved:" + IsBeingMoved + 
                    "\nspeedBumpT: " + speedBumpT +
                    "\nmoveDir: " + Direction +
                    "\ncanMove: " + CanMove;
        }
    }
}