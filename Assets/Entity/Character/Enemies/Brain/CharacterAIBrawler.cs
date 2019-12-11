using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Character.AI
{
    public enum EBrawlerAIStates
    {
        Wander,
        Attack,
        Orbit,
        OrbitAttack,
    }

    public class CharacterAIBrawler : MonoBehaviour
    {
        private GameObject target;
        private float lastDistance;

        [Header("Wander State")]
        public WanderStateConfig WanderStateConfig;

        [Header("Attack State")]
        public AttackStateConfig AttackStateConfig;

        [Header("Orbit State")]
        public OrbitStateConfig OrbitStateConfig;

        private NavMeshAgent navMeshAgent;

        private CharacterAnimator characterAnimator;
        private CharacterHealth characterHealth;
        private CharacterCombat characterCombat;

        private EBrawlerAIStates movementStatus;
        private EBrawlerAIStates MovementStatus
        {
            get { return movementStatus; }
            set
            {
                if (value == movementStatus) return;

                characterAnimator.ResetAttackTrigger();

                if (movementStatus == EBrawlerAIStates.Attack && value != movementStatus)
                {
                    if (target != null)
                    {
                        attackState.OnExit(target.transform);
                    }
                }

                movementStatus = value;
                switch (movementStatus)
                {
                    case EBrawlerAIStates.Orbit:
                        orbitState.OnEnter();
                        break;
                    case EBrawlerAIStates.OrbitAttack:
                    case EBrawlerAIStates.Attack:
                        attackState.OnEnter(target.transform);
                        return;
                }
            }
        }

        private AttackState attackState;
        private WanderState wanderState;
        private OrbitState orbitState;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            characterAnimator = GetComponent<CharacterAnimator>();
            characterHealth = GetComponent<CharacterHealth>();
            characterCombat = GetComponent<CharacterCombat>();

            attackState = new AttackState(gameObject, AttackStateConfig);
            wanderState = new WanderState(gameObject, WanderStateConfig);
            orbitState = new OrbitState(gameObject, OrbitStateConfig);

            MovementStatus = EBrawlerAIStates.Orbit;

            UpdateTarget();
        }

        private void Update()
        {
            if (target == null)
            {
                MovementStatus = EBrawlerAIStates.Wander;
            }

            float distanceToTarget = target != null ? Vector3.Distance(transform.position, target.transform.position) : float.MaxValue;
            switch (MovementStatus)
            {
                case EBrawlerAIStates.Attack:
                    AttackStateUpdate(target.transform, distanceToTarget);
                    break;
                case EBrawlerAIStates.Wander:
                    WanderStateUpdate(distanceToTarget);
                    break;
                case EBrawlerAIStates.Orbit:
                    OrbitStateUpdate(distanceToTarget);
                    break;
                case EBrawlerAIStates.OrbitAttack:
                    OrbitAttackState(target.transform, distanceToTarget);
                    break;
            }

            lastDistance = distanceToTarget;
        }

        private void OnEnable()
        {
            characterHealth.OnDamaged += OnDamagedCallback;
        }

        private void OnDisable()
        {
            characterHealth.OnDamaged -= OnDamagedCallback;
            if (MovementStatus == EBrawlerAIStates.Attack)
            {
                AIManager.Instance?.DecreaseAttackers(target);
            }
        }

        private void OnDamagedCallback(CharacterAttackData obj)
        {
            if (MovementStatus == EBrawlerAIStates.Orbit)
            {
                MovementStatus = EBrawlerAIStates.OrbitAttack;
            }
        }

        void UpdateTarget()
        {
            if (target != null)
            {
                AIManager.Instance.ClearTarget(target);
            }
            target = AIManager.Instance.GetTarget(gameObject);
        }

        void AttackStateUpdate(Transform target, float distanceToTarget, bool orbitReaction = false)
        {
            int result = attackState.Update(target, orbitReaction);
            if (result == AttackState.RES_TOO_MANY_ATTACKERS ||
                result == AttackState.RES_ORBIT_REACTION_COMBO_END)
            {
                MovementStatus = EBrawlerAIStates.Orbit;
            }
            else if (result == AttackState.RES_OUT_OF_SIGHT)
            {
                MovementStatus = EBrawlerAIStates.Wander;
            }
            /*if (AIManager.Instance.GetNumberOfAttackers(target.gameObject) > AIManager.Instance.GetMaxAttackers(target.gameObject) && !orbitReaction)
            {
                MovementStatus = EBrawlerAIStates.Orbit;
                return;
            }

            if (distanceToTarget <= DistanceToAttack)
            {
                navMeshAgent.isStopped = true;
                if (Time.time > lastAttack + AttackCooldown && Time.time > lastCombo + ComboCooldown)
                {
                    if (currentAttackIndex > comboLength - 1)
                    {
                        if (orbitReaction)
                        {
                            MovementStatus = EBrawlerAIStates.Orbit;
                            return;
                        }
                        else
                        {
                            currentAttackIndex = 0;
                            lastCombo = Time.time;
                        }
                    }

                    var attackType = combo[(currentAttackIndex++) % comboLength];
                    characterCombat.RequestAttack(attackType);
                    lastAttack = Time.time;
                }
            }
            else if (distanceToTarget >= SightRange)
            {
                MovementStatus = EBrawlerAIStates.Wander;
            }
            else
            {
                navMeshAgent.isStopped = characterHealth.IsOnGround;
                navMeshAgent.SetDestination(target.position);
            }*/
        }

        void WanderStateUpdate(float distanceToTarget)
        {
            int result = wanderState.Update(target.transform);
            if (result == WanderState.RES_ENEMY_IN_SIGHT)
            {
                MovementStatus = EBrawlerAIStates.Orbit;
            }
            /*
            if (distanceToTarget < SightRange)
            {
                MovementStatus = EBrawlerAIStates.Orbit;
                return;
            }

            navMeshAgent.isStopped = false;
            if (!navMeshAgent.hasPath || navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                if (Time.time > lastPathChange + SleepTime)
                {
                    navMeshAgent.SetDestination(transform.position + UnityEngine.Random.insideUnitSphere * WanderRadius);
                    lastPathChange = Time.time;
                }
            }*/
        }

        void OrbitStateUpdate(float distanceToTarget)
        {
            /*if (AIManager.Instance.GetNumberOfAttackers(target.gameObject) < AIManager.Instance.GetMaxAttackers(target.gameObject))
            {
                MovementStatus = EBrawlerAIStates.Attack;
                return;
            }

            if (Mathf.Abs(distanceToTarget - lastDistance) > 0.025f)
            {
                {
                    navMeshAgent.isStopped = characterHealth.IsOnGround;
                    float angle = gameObject.GetInstanceID() % 360f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    navMeshAgent.SetDestination(target.transform.position + offset * OrbitRadius);
                    lastPathChange = Time.time;
                }
            }

            if (Time.time > lastAttackRoll + DiceRollCooldown)
            {
                if (UnityEngine.Random.value > 0.75)
                {
                    MovementStatus = EBrawlerAIStates.OrbitAttack;
                    return;
                }

                lastAttackRoll = Time.time;
            }*/

            int result = orbitState.Update(target.transform);
            if (result == OrbitState.RES_NOT_ENOUGH_ATTACKERS)
            {
                MovementStatus = EBrawlerAIStates.Attack;
            }
            else if (result == OrbitState.RES_ORBIT_ATTACK)
            {
                MovementStatus = EBrawlerAIStates.OrbitAttack;
            }
        }

        void OrbitAttackState(Transform target, float distanceToTarget)
        {
            AttackStateUpdate(target, distanceToTarget, true);
        }

        private void OnDrawGizmos()
        {
            if (navMeshAgent == null || !navMeshAgent.hasPath) return;

            var path = navMeshAgent.path;
            for (int i = 1; i < path.corners.Length; i++)
            {
                var a = path.corners[i - 1];
                var b = path.corners[i];
                Gizmos.DrawLine(a, b);
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!Application.isEditor) return;

            Rect r = UIManager.WorldSpaceGUI(transform.position, Vector2.one * 100f);
            GUI.Label(r, "State: " + MovementStatus);
        }
#endif

    }
}