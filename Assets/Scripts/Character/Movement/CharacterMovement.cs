using System;
using UnityEngine;
using UnityEngine.AI;
using Catacumba.Effects;
using Catacumba.Configuration;

namespace Catacumba.Entity 
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterMovement : CharacterComponentBase
    {
        public ParticleEffectConfiguration MovementEffect;

        // ==== MOVEMENT
        [HideInInspector]
        public Vector3 Direction;
        public Vector3 Velocity { get { return brainType == ECharacterBrainType.AI ? NavMeshAgent.velocity : Direction; } }
        public float MoveSpeed { get { return data.Stats.MoveSpeed; } }
        public Vector3 SpeedBumpDir { get; private set; }

        public bool IsBeingMoved { get { return speedBumpT > 0f; } }

        //public bool IsRolling { get; private set; }
        public bool IsRolling { get { return rollSpeedT > 0f; } }

        public bool CanMove
        {
            get
            {
                return true;
                /*
                return (!combat || !combat.IsOnCombo) &&
                    (!health || (!health.IsOnGround && !health.IsBeingDamaged)) &&
                    !IsBeingMoved &&
                    (data.BrainType == ECharacterBrainType.AI ? Time.time > (combat.LastDamageData.Time + (combat.LastDamageData.Type == EAttackType.Strong ? 0.25f : 0.75f)) : true);
                */
            }
        }

        public bool IgnoreSpeedBump = false;

        [Header("Dash when attacks")]
        public float SpeedBumpForce = 0.9f;

        #region INTERFACE WITH NAVMESH

        public NavMeshAgent NavMeshAgent
        {
            get; private set;
        }

        public bool NavAgentValid
        {
            get { return NavMeshAgent && NavMeshAgent.enabled; }
        }

        private bool isAgentStopped;
        public bool IsAgentStopped
        {
            get { return !CanMove || isAgentStopped; }
            set
            {
                isAgentStopped = value;
            }
        }

        public NavMeshPathStatus PathStatus
        {
            get { return NavMeshAgent.pathStatus; }
        }

        private Vector3 targetDestination;
        public Vector3 Destination
        {
            get { return NavMeshAgent.destination; }
        }

        public bool HasPath { get { return NavMeshAgent.hasPath; } }

        public bool SetDestination(Vector3 pos)
        {
            targetDestination = pos;
            if (NavMeshAgent.enabled)
            {
                return NavMeshAgent.SetDestination(pos);
            }
            return false;
        }

        #endregion

        public System.Action OnRoll;
        public System.Action OnRollEnded;

        // === REFS
        private CharacterHealth health;
        private CharacterCombat combat;

        private Vector3 forward;

        private float speedBumpT;

        private float rollSpeedT;
        private float lastRoll;
        private Vector3 rollDirection;

        // private Bitmask canMoveMask;
        
        private const float speedBumpScale = 7f;

        private ECharacterBrainType brainType
        {
            get
            {
                return data.BrainType;
            }
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

        protected override void Awake()
        {
            base.Awake();


            //data = GetComponent<CharacterData>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            combat = GetComponent<CharacterCombat>();
        }

        protected override void Start()
        {
            base.Start();

            if (NavMeshAgent)
                NavMeshAgent.speed = MoveSpeed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (health)
            {
                health.OnDamaged -= OnDamagedCallback;
                health.OnFall -= OnFallCallback;
                health.OnGetUp -= OnGetUpCallback;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (health && health.IsOnGround)
                NavMeshAgent.enabled = speedBumpT > 0f;
            
            Move();

    #if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.F4))
            {
                showDebug = !showDebug;
            }
    #endif
        }

        void Move()
        {
            Vector3 velocity = Vector3.zero;

            if (IsBeingMoved)
            {
                // applies dash on attack
                float t = 1f - speedBumpT;
                var dir = SpeedBumpDir;
                //dir.y = velocity.y;
                velocity = dir;

                speedBumpT = Mathf.Max(0, speedBumpT - Time.deltaTime);
            }

            else if (CanMove)
            {
                float rollSpeed = 1f + Mathf.Clamp01(rollSpeedT * (1f - rollSpeedT) * 8f);

                var dir = Direction.normalized;
                if (IsRolling)
                {
                    float a = 1f - rollSpeedT;
                    a = a * a;
                    dir = Vector3.Slerp(rollDirection, dir, a);
                }

                // escalar direção c a velocidade
                var dirNorm = dir * MoveSpeed;
                forward = Vector3.Slerp(forward, dirNorm, 0.5f * Time.deltaTime * 30f).normalized;

                // aplicar roll speed
                velocity = dirNorm * rollSpeed;

                if (Direction.sqrMagnitude > 0f && true /*!combat.IsOnCombo*/)
                {
                    dirNorm.y = 0f;
                    transform.LookAt(transform.position + forward);
                }
            }

            if ( NavAgentValid && (brainType == ECharacterBrainType.Input || speedBumpT > 0f) )
            {
                NavMeshAgent.Move(velocity * Time.deltaTime);
            }

            rollSpeedT = Mathf.Clamp01(rollSpeedT - Time.deltaTime);
            if (Mathf.Approximately(rollSpeedT, 0f))
            {
                OnRollEnded?.Invoke();
            }

            if (NavAgentValid)
            {
                NavMeshAgent.isStopped = IsAgentStopped;
            }
        }

        public void ApplySpeedBump(Vector3 direction, float force)
        {
            //transform.LookAt(transform.position + direction);
            speedBumpT = CharacterVariables.DashDuration;
            SpeedBumpDir = direction.normalized * force;
        }

        private void OnDamagedCallback(AttackResult attack)
        {
            if (attack.CancelAnimation)
            {
                var lookAt = attack.Attacker.transform.position;
                lookAt.y = transform.position.y;
                transform.LookAt(lookAt);
            }

            if (attack.AttackerStats != data.Stats)
            {
                ApplySpeedBump(attack.Attacker.transform.forward, GetSpeedBumpForce(attack));
            }
        }

        void OnRequestAttack(EAttackType type)
        {
            forward = Direction.normalized;
        }

        void OnAttack(AttackResult attack)
        {
            ApplySpeedBump(transform.forward, GetSpeedBumpForce(attack) * 0.75f);
        }

        void OnComboStarted()
        {
            //canMoveMask.Set(0, true);
        }

        void OnComboEnded()
        {
            //canMoveMask.Set(0, false);
        }

        public float GetSpeedBumpForce(AttackResult attack)
        {
            if (IgnoreSpeedBump) return 0f;
            return (attack.Type == EAttackType.Weak 
                                ? CharacterVariables.AttackDashForceWeak 
                                : CharacterVariables.AttackDashForceStrong);

            /*
            float modifier = (attack.Type == EAttackType.Weak ? 1f : 2f);
            modifier = attack.Knockdown ? 1f : modifier;

            return Mathf.Min(7f, ((float)attack.Damage / 25) * modifier);
            */
        }

        public void Roll(Vector3 direction)
        {
            if (Time.time < lastRoll + 0.75f ||
                (health && health.IsOnGround ) /* ||
                combat.IsOnHeavyAttack*/)
            {
                return;
            }

            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            //combat.IsOnCombo = false;
            OnRoll?.Invoke();

            rollSpeedT    = 1f;
            speedBumpT    = 0f;
            rollDirection = direction;
            forward       = rollDirection;
            lastRoll      = Time.time;

            transform.LookAt(transform.position + direction);
        }

        public void BeginRoll()
        {
            //IsRolling = true;
        }

        public void EndRoll()
        {
            //IsRolling = false;
            //rollSpeedT = 0f;
        }

        private void OnGetUpCallback()
        {
            NavMeshAgent.enabled = true;
        }

        private void OnFallCallback()
        {
            //NavMeshAgent.enabled = false;
        }

        public static Vector3 DistanceIndependentRotation2D(Vector3 targetPosition, Vector3 currentPosition, Vector3 currentForward)
        {
            float maxRotationAngle = 60f;

            // rotação independente de distância do alvo
            Vector3 dir3d = (targetPosition - currentPosition);
            dir3d.y = 0f;

            Vector3 fwd3d = currentForward;
            dir3d.y = 0f;

            Vector2 d = new Vector2(dir3d.x, dir3d.z).normalized;
            Vector2 f = new Vector2(fwd3d.x, fwd3d.z).normalized;

            float t = Vector2.SignedAngle(f, d);
            float a = Mathf.Min(Mathf.Abs(t), maxRotationAngle);
            a *= Mathf.Sign(t);
            a *= Time.deltaTime;

            // talvez tenha uma forma suportada de rotacionar um v2d
            Vector2 y = new Vector2();

            float sin = Mathf.Sin(a * Mathf.Deg2Rad);
            float cos = Mathf.Cos(a * Mathf.Deg2Rad);

            float tx = f.x;
            float ty = f.y;
            y.x = (cos * tx) - (sin * ty);
            y.y = (sin * tx) + (cos * ty);

            return new Vector3(y.x, currentForward.y, y.y);
        }

    #if UNITY_EDITOR

        private bool showDebug = false;

        private void OnGUI()
        {
            if (!showDebug) return;

            /*if (data.BrainType != ECharacterBrainType.Input)
                return;*/

            Rect r = CharacterData.WorldSpaceGUI(transform.position, Vector2.one * 200f);
            GUI.Label(r, "NavMeshAgent: " + NavAgentValid +
                        //"\nIsOnCombo: " + combat.IsOnCombo +
                        "\nspeedBumpT: " + speedBumpT +
                        "\nrollSpeedT: " + rollSpeedT +
                        "\nrollDir: " + rollDirection +
                        "\nmoveDir: " + Direction +
                        "\ncanMove: " + CanMove);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + SpeedBumpDir);
        }

    #endif
        }
}