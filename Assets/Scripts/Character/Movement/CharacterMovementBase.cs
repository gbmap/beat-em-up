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
        public bool IsTimerStopped { get { return stopTimer < stopTime; } }
        public bool IsBeingMoved { get { return speedBumpT > 0f; } }
        public Vector3 SpeedBumpDir { get; private set; }

        // references
        private ECharacterBrainType brainType { get { return data.BrainType; } }
        private CharacterHealth health;
        private CharacterCombat combat;

        protected Vector3 forward; // where the character is facing

        private float speedBumpT;
        private const float speedBumpScale = 7f;

        private float stopTimer = 0f;
        private float stopTime = 0f;

        private bool LastHitWasRecent 
        { 
            get 
            { 
                if (!health || health.LastHitData == null) return false;
                return Time.time < health.LastHit + 1f; 
            } 
        }

		//////////////////////////////
		///		INTERFACE

		public void SetDestination(Vector3 position)
		{
            if (brainType != ECharacterBrainType.AI)
                return;

            NavMeshAgent.SetDestination(position);
		}

        public void StopForTime(float time)
        {
            stopTime = time;
            stopTimer = 0f;
        }

        protected virtual void UpdateEffect()
        {
            if (!MovementEffect) return;

            MovementEffect.SetEmission(this, IsBeingMoved);
            bool emission = MovementEffect.IsEmitting(this);
            if (emission)
                MovementEffect.PointSystemTowards(this, -SpeedBumpDir);
        }

        protected override void Awake()
        {
            base.Awake();
            NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Update()
        {
            // Used for when characters are on the ground, Unity 
            // shouldn't do avoidance when characters are laying on the floor.
            NavMeshAgent.enabled = GetNavMeshEnabled(health, speedBumpT);

            // Prohibits characters from walking when taking damage or attacking.
            UpdateNavMeshStopped(speedBumpT);

            // Performs actual movement for the character.
            // Used for everything except AI navigation.
            Vector3 velocity = MoveNavAgent();

            UpdateStopTimer();
            UpdateFacingDirection(velocity);
            UpdateEffect();
        }

        private void UpdateNavMeshStopped(float speedBumpT)
        {
            bool stopped = GetNavMeshStopped(speedBumpT);
            if (NavMeshAgent.isStopped == stopped) return; 

            NavMeshAgent.isStopped = GetNavMeshStopped(speedBumpT);
            if (NavMeshAgent.isStopped)
                NavMeshAgent.velocity = Vector3.zero;
        }

        private void UpdateFacingDirection(Vector3 velocity)
        {
            forward = GetForwardVector(velocity.normalized);
            transform.LookAt(transform.position + forward);
        }

        private void UpdateStopTimer()
        {
            stopTimer += Time.deltaTime;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MovementEffect?.Destroy(this);
        }

        private bool GetNavMeshEnabled(CharacterHealth health, float speedBump)
        {
            bool ret = true;
            if (health && health.IsOnGround)
                return speedBump > 0f;

            return ret;
        }

        private bool GetNavMeshStopped(float speedBumpT)
        {
            bool speedBumpPause = speedBumpT > 0f;
            bool comboPause = combat ? combat.IsOnCombo : false;
            return speedBumpPause || LastHitWasRecent || comboPause || IsTimerStopped; 
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
            //return transform.forward;
            if (brainType == ECharacterBrainType.AI)
            {
                if (combat && combat.IsOnCombo)
                    return transform.forward;

                if (LastHitWasRecent)
                    return (health.LastHitData.Attacker.transform.position - transform.position).normalized;

                if (NavMeshAgent.hasPath)
                    return (NavMeshAgent.pathEndPosition - transform.position).normalized;
            }
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

            if (component is CharacterCombat)
            {
                combat = component as CharacterCombat;
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

            if (component is CharacterCombat)
            {
                combat = null;
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
