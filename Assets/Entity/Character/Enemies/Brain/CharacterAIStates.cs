using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.Character.AI
{
    // ================= STATES

    public class BaseState
    {
        protected GameObject gameObject;
        protected CharacterHealth health;
        protected CharacterCombat combat;
        protected NavMeshAgent navMeshAgent;

        public BaseState(GameObject gameObject)
        {
            this.gameObject = gameObject;
            health = gameObject.GetComponent<CharacterHealth>();
            combat = gameObject.GetComponent<CharacterCombat>();
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        }
    }

    #region WANDER_STATE

    [System.Serializable]
    public class WanderStateConfig
    {
        public float SightRange = 10f;
        public float SleepTime = 5f;
        public float WanderRadius = 5f;
    }

    public class WanderState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_ENEMY_IN_SIGHT = 1;

        public WanderStateConfig Cfg;

        private float lastPathChange;

        public WanderState(GameObject gameObject, WanderStateConfig config)
            : base(gameObject)
        {
            Cfg = config;
            health = gameObject.GetComponent<CharacterHealth>();
            combat = gameObject.GetComponent<CharacterCombat>();
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        }

        public void OnEnter(Transform target)
        {

        }

        public void OnExit(Transform target)
        {

        }
        
        public int Update(Transform target)
        {
            float distanceToTarget = Vector3.Distance(gameObject.transform.position, target.position);

            if (distanceToTarget < Cfg.SightRange)
            {
                return RES_ENEMY_IN_SIGHT;
            }

            navMeshAgent.isStopped = false;
            if (!navMeshAgent.hasPath || navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                if (Time.time > lastPathChange + Cfg.SleepTime)
                {
                    navMeshAgent.SetDestination(gameObject.transform.position + UnityEngine.Random.insideUnitSphere * Cfg.WanderRadius);
                    lastPathChange = Time.time;
                }
            }

            return RES_CONTINUE;
        }
    }

    #endregion

    #region ATTACK_STATE

    [System.Serializable]
    public class AttackStateConfig
    {
        public float DistanceToAttack = 1.5f;
        public float AttackCooldown = 0.3f;
        public float ComboCooldown = 2f;
        public float SightRange = 10f;
        public int MaxComboHits = 5;
    }

    public class AttackState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_OUT_OF_SIGHT = 1;
        public const int RES_TOO_MANY_ATTACKERS = 2;
        public const int RES_COMBO_END = 3;
        public const int RES_ORBIT_REACTION_COMBO_END = 4;

        public AttackStateConfig Cfg;

        private float lastAttack;
        private float lastCombo;

        private int currentAttackIndex;
        private EAttackType[] combo =
        {
            EAttackType.Weak,
            EAttackType.Weak,
            EAttackType.Strong,
            EAttackType.Weak,
            EAttackType.Strong,
            EAttackType.Strong
        };
        private int comboLength;

        public AttackState(GameObject gameObject, AttackStateConfig config)
            : base(gameObject)
        {
            Cfg = config;
        }

        public void OnExit(Transform target)
        {
            AIManager.Instance.DecreaseAttackers(target.gameObject);
        }

        public void OnEnter(Transform target, bool orbitReaction = false)
        {
            if (!orbitReaction)
            {
                AIManager.Instance.IncreaseAttackers(target.gameObject);
            }
            lastAttack = Time.time;
            comboLength = UnityEngine.Random.Range(1, Cfg.MaxComboHits);
        }

        public int Update(Transform target, bool orbitReaction = false)
        {
            int nAttackers = AIManager.Instance.GetNumberOfAttackers(target.gameObject);
            int maxAttackers = AIManager.Instance.GetMaxAttackers(target.gameObject);

            if (nAttackers > maxAttackers && !orbitReaction)
            {
                return RES_TOO_MANY_ATTACKERS;
            }

            float distanceToTarget = Vector3.Distance(gameObject.transform.position, target.position);

            if (distanceToTarget <= Cfg.DistanceToAttack)
            {
                navMeshAgent.isStopped = true;
                if (Time.time > lastAttack + Cfg.AttackCooldown && Time.time > lastCombo + Cfg.ComboCooldown)
                {
                    if (currentAttackIndex > comboLength - 1)
                    {
                        if (orbitReaction)
                        {
                            return RES_ORBIT_REACTION_COMBO_END;
                        }
                        else
                        {
                            currentAttackIndex = 0;
                            lastCombo = Time.time;
                        }
                    }

                    var attackType = combo[(currentAttackIndex++) % comboLength];
                    combat.RequestAttack(attackType);
                    lastAttack = Time.time;
                }
            }
            else if (distanceToTarget >= Cfg.SightRange)
            {
                return RES_OUT_OF_SIGHT;
            }
            else
            {
                navMeshAgent.isStopped = health.IsOnGround;
                navMeshAgent.SetDestination(target.position);
            }

            return RES_CONTINUE;
        }
    }

    #endregion

    #region ORBIT_STATE

    [System.Serializable]
    public class OrbitStateConfig
    {
        public float OrbitRadius = 3f;
        public float DiceRollCooldown = 2f;
    }

    public class OrbitState : BaseState
    {
        OrbitStateConfig Cfg;

        public const int RES_CONTINUE = 0;
        public const int RES_NOT_ENOUGH_ATTACKERS = 1;
        public const int RES_ORBIT_ATTACK = 2;
        
        private float lastDistance;
        private float lastAttackRoll;
        private float lastPathChange;

        public OrbitState(GameObject obj, OrbitStateConfig config)
            : base(obj)
        {
            Cfg = config;
        }

        public void OnEnter()
        {
            lastAttackRoll = Time.time;
        }

        public int Update(Transform target)
        {
            float distanceToTarget = Vector3.Distance(gameObject.transform.position, target.position);

            if (AIManager.Instance.GetNumberOfAttackers(target.gameObject) < AIManager.Instance.GetMaxAttackers(target.gameObject))
            {
                //MovementStatus = EBrawlerAIStates.Attack;
                return RES_NOT_ENOUGH_ATTACKERS;
            }

            if (Mathf.Abs(distanceToTarget - lastDistance) > 0.025f)
            {
                {
                    navMeshAgent.isStopped = health.IsOnGround;
                    float angle = gameObject.GetInstanceID() % 360f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    navMeshAgent.SetDestination(target.transform.position + offset * Cfg.OrbitRadius);
                    lastPathChange = Time.time;
                }
            }

            if (Time.time > lastAttackRoll + Cfg.DiceRollCooldown)
            {
                if (UnityEngine.Random.value > 0.75)
                {
                    //MovementStatus = EBrawlerAIStates.OrbitAttack;
                    return RES_ORBIT_ATTACK;
                }

                lastAttackRoll = Time.time;
            }

            lastDistance = distanceToTarget;
            return RES_CONTINUE;
        }

    }

    #endregion

}