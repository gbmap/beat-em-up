using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.Character.AI
{
    // ================= STATES
    public struct StateResult
    {
        public int code;
        public object[] data;

        public StateResult(int r, params object[] data)
        {
            code = r;
            this.data = data;
        }
    }

    public class BaseState
    {
        protected GameObject gameObject;
        protected CharacterData data;
        protected CharacterHealth health;
        protected CharacterCombat combat;
        protected NavMeshAgent navMeshAgent;

        public BaseState(GameObject gameObject)
        {
            this.gameObject = gameObject;
            data = gameObject.GetComponent<CharacterData>();
            health = gameObject.GetComponent<CharacterHealth>();
            combat = gameObject.GetComponent<CharacterCombat>();
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        }

        public virtual StateResult Update()
        {
            return new StateResult(0);
        }

        public virtual void OnEnter()
        {

        }

        public virtual void OnExit()
        {

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
        private float lastEntityCheck;

        public WanderState(GameObject gameObject, WanderStateConfig config)
            : base(gameObject)
        {
            Cfg = config;
            health = gameObject.GetComponent<CharacterHealth>();
            combat = gameObject.GetComponent<CharacterCombat>();
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();

            lastPathChange = float.MinValue;
        }

        public override StateResult Update()
        {
            if (Time.time > lastEntityCheck + 2f)
            {
                Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, Cfg.SightRange);
                Collider target = colliders.Where(c => c.CompareTag("Player")).FirstOrDefault();

                if (target != null)
                {
                    return new StateResult(RES_ENEMY_IN_SIGHT, target.transform);
                }

                lastEntityCheck = Time.time;
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

            return new StateResult(RES_CONTINUE);
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
        public Transform Target;
        private bool orbitReaction;

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

        public AttackState(GameObject gameObject, AttackStateConfig config, Transform target, bool orbitReaction = false)
            : base(gameObject)
        {
            Cfg = config;
            Target = target;
            this.orbitReaction = orbitReaction;
        }

        public override void OnEnter()
        {
            if (!orbitReaction)
            {
                AIManager.Instance.IncreaseAttackers(Target.gameObject);
            }
            lastAttack = Time.time;
            comboLength = UnityEngine.Random.Range(1, Cfg.MaxComboHits);
        }

        public override void OnExit()
        {
            if (!orbitReaction)
            {
                AIManager.Instance.DecreaseAttackers(Target.gameObject);
            }
        }

        public override StateResult Update()
        {
            int nAttackers = AIManager.Instance.GetNumberOfAttackers(Target.gameObject);
            int maxAttackers = AIManager.Instance.GetMaxAttackers(Target.gameObject);

            if (nAttackers > maxAttackers && !orbitReaction)
            {
                return new StateResult(RES_TOO_MANY_ATTACKERS);
            }

            float distanceToTarget = Vector3.Distance(gameObject.transform.position, Target.position);

            if (distanceToTarget <= Cfg.DistanceToAttack)
            {
                navMeshAgent.isStopped = true;
                if (Time.time > lastAttack + Cfg.AttackCooldown && Time.time > lastCombo + Cfg.ComboCooldown)
                {
                    if (currentAttackIndex > comboLength - 1)
                    {
                        if (orbitReaction)
                        {
                            return new StateResult(RES_ORBIT_REACTION_COMBO_END);
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
                return new StateResult(RES_OUT_OF_SIGHT);
            }
            else
            {
                navMeshAgent.isStopped = health.IsOnGround;
                navMeshAgent.SetDestination(Target.position);
            }

            return new StateResult(RES_CONTINUE);
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
        public OrbitStateConfig Cfg;
        public Transform Target;

        public const int RES_CONTINUE = 0;
        public const int RES_NOT_ENOUGH_ATTACKERS = 1;
        public const int RES_ORBIT_ATTACK = 2;
        
        private float lastDistance;
        private float lastAttackRoll;
        private float lastPathChange;

        public OrbitState(GameObject obj, OrbitStateConfig config, Transform target)
            : base(obj)
        {
            Cfg = config;
            Target = target;
        }

        public override void OnEnter()
        {
            lastAttackRoll = Time.time;
        }

        public override StateResult Update()
        {
            float distanceToTarget = Vector3.Distance(gameObject.transform.position, Target.position);

            if (AIManager.Instance.GetNumberOfAttackers(Target.gameObject) < AIManager.Instance.GetMaxAttackers(Target.gameObject))
            {
                //MovementStatus = EBrawlerAIStates.Attack;
                return new StateResult(RES_NOT_ENOUGH_ATTACKERS);
            }

            if (Mathf.Abs(distanceToTarget - lastDistance) > 0.025f)
            {
                {
                    navMeshAgent.isStopped = health.IsOnGround;
                    float angle = gameObject.GetInstanceID() % 360f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    navMeshAgent.SetDestination(Target.transform.position + offset * Cfg.OrbitRadius);
                    lastPathChange = Time.time;
                }
            }

            if (Time.time > lastAttackRoll + Cfg.DiceRollCooldown)
            {
                if (UnityEngine.Random.value > 0.75)
                {
                    //MovementStatus = EBrawlerAIStates.OrbitAttack;
                    return new StateResult(RES_ORBIT_ATTACK);
                }

                lastAttackRoll = Time.time;
            }

            lastDistance = distanceToTarget;
            return new StateResult(RES_CONTINUE);
        }

    }

    #endregion

    /*
     * Esse estado deveria se chamar UseSkillState e receber uma skill pra ser usada num alvo quando possível.
     */
    #region HEAL_STATE

    [System.Serializable]
    public class HealStateConfig
    {
        public float MinHealDistance = 4f;
        public float HealCastTime = 1f;
        public float HealCooldown = 3f;
    }

    public class HealState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_HEALED = 1;

        public GameObject Target;

        private HealStateConfig Cfg;

        private float healCastT;
        public float LastHeal
        {
            get; private set;
        }

        public HealState(GameObject gameObject, HealStateConfig cfg, GameObject target) 
            : base(gameObject)
        {
            Cfg = cfg;
            Target = target;
        }

        public override StateResult Update()
        {
            float d = Vector3.Distance(gameObject.transform.position, Target.transform.position);
            if (d > Cfg.MinHealDistance)
            {
                float navDistanceToTarget = Vector3.Distance(navMeshAgent.destination, Target.transform.position);

                if (navDistanceToTarget > Cfg.MinHealDistance)
                {
                    Vector3 targetPosition = (gameObject.transform.position - Target.transform.position).normalized * Cfg.MinHealDistance;
                    navMeshAgent.SetDestination(targetPosition);
                }
            }
            
            else if (Time.time > LastHeal + Cfg.HealCooldown)
            {
                healCastT += Time.deltaTime;
                if (healCastT >= Cfg.HealCastTime)
                {
                    CombatManager.Heal(data.Stats, Target.GetComponent<CharacterData>().Stats);
                    return new StateResult(RES_HEALED);
                }
            }

            return new StateResult(RES_CONTINUE);
        }
    }

    #endregion

}