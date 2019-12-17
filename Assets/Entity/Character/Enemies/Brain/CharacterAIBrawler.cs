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
                characterAnimator.ResetAttackTrigger();

                if (currentState != null)
                {
                    currentState.OnExit();
                }

                movementStatus = value;
                switch (movementStatus)
                {
                    case EBrawlerAIStates.Wander:
                        currentState = new WanderState(gameObject, WanderStateConfig);
                        break;
                    case EBrawlerAIStates.Orbit:
                        currentState = new OrbitState(gameObject, OrbitStateConfig, target.transform);
                        break;
                    case EBrawlerAIStates.OrbitAttack:
                    case EBrawlerAIStates.Attack:
                        currentState = new AttackState(gameObject, AttackStateConfig, target.transform, movementStatus == EBrawlerAIStates.OrbitAttack);
                        break;
                }

                currentState.OnEnter();
            }
        }

        private BaseState currentState;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            characterAnimator = GetComponent<CharacterAnimator>();
            characterHealth = GetComponent<CharacterHealth>();
            characterCombat = GetComponent<CharacterCombat>();

            MovementStatus = EBrawlerAIStates.Wander;
        }

        private void Update()
        {
            if (target == null && MovementStatus != EBrawlerAIStates.Wander)
            {
                MovementStatus = EBrawlerAIStates.Wander;
            }

            if (currentState != null)
            {
                StateResult result = currentState.Update();
                HandleStateResult(MovementStatus, result);
            }
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

        public void HandleStateResult(EBrawlerAIStates state, StateResult result)
        {
            switch (state)
            {
                case EBrawlerAIStates.OrbitAttack:
                case EBrawlerAIStates.Attack:
                    if (result.code == AttackState.RES_TOO_MANY_ATTACKERS ||
                        result.code == AttackState.RES_ORBIT_REACTION_COMBO_END)
                    {
                        MovementStatus = EBrawlerAIStates.Orbit;
                    }
                    else if (result.code == AttackState.RES_OUT_OF_SIGHT)
                    {
                        MovementStatus = EBrawlerAIStates.Wander;
                    }
                    break;
                case EBrawlerAIStates.Orbit:
                    if (result.code == OrbitState.RES_NOT_ENOUGH_ATTACKERS)
                    {
                        MovementStatus = EBrawlerAIStates.Attack;
                    }
                    else if (result.code == OrbitState.RES_ORBIT_ATTACK)
                    {
                        MovementStatus = EBrawlerAIStates.OrbitAttack;
                    }
                    break;
                
                case EBrawlerAIStates.Wander:
                    if (result.code == WanderState.RES_ENEMY_IN_SIGHT)
                    {
                        target = (result.data[0] as Transform).gameObject;
                        MovementStatus = EBrawlerAIStates.Orbit;
                    }
                    break;
            }
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