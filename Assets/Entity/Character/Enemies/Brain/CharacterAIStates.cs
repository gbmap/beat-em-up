using System;
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

        public virtual void OnDebugDrawGizmos()
        {

        }

        public virtual void OnGUI()
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

        public static WanderStateConfig DefaultConfig
        {
            get
            {
                return new WanderStateConfig();
            }
        }
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
                    return new StateResult(RES_ENEMY_IN_SIGHT, target.gameObject);
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

        public static AttackStateConfig DefaultConfig
        {
            get
            {
                return new AttackStateConfig();
            }
        }
    }

    public class AttackState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_OUT_OF_SIGHT = 1;
        public const int RES_TOO_MANY_ATTACKERS = 2;
        public const int RES_COMBO_END = 3;
        public const int RES_ORBIT_REACTION_COMBO_END = 4;

        public AttackStateConfig Cfg;
        public GameObject Target;
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

        public AttackState(GameObject gameObject,
                          AttackStateConfig config,
                          GameObject target,
                          bool orbitReaction = false)
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

            health.OnDamaged += OnDamagedCallback;
        }
        
        public override void OnExit()
        {
            if (!orbitReaction)
            {
                AIManager.Instance.DecreaseAttackers(Target.gameObject);
            }

            health.OnDamaged -= OnDamagedCallback;
        }

        public override StateResult Update()
        {
            if (!orbitReaction)
            {
                int nAttackers = AIManager.Instance.GetNumberOfAttackers(Target.gameObject);
                int maxAttackers = AIManager.Instance.GetMaxAttackers(Target.gameObject);

                if (nAttackers > maxAttackers)
                {
                    return new StateResult(RES_TOO_MANY_ATTACKERS, Target);
                }
            }

            float distanceToTarget = Vector3.Distance(gameObject.transform.position, Target.transform.position);

            if (distanceToTarget <= Cfg.DistanceToAttack)
            {
                navMeshAgent.isStopped = true;
                if (Time.time > lastAttack + Cfg.AttackCooldown && Time.time > lastCombo + Cfg.ComboCooldown)
                {
                    if (currentAttackIndex > comboLength - 1)
                    {
                        if (orbitReaction)
                        {
                            return new StateResult(RES_ORBIT_REACTION_COMBO_END, Target);
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
                navMeshAgent.SetDestination(Target.transform.position);
            }

            return new StateResult(RES_CONTINUE);
        }

        private void OnDamagedCallback(CharacterAttackData attackData)
        {
            if (attackData.Type == EAttackType.Strong)
            {
                lastAttack = Time.time + 0.2f;
            }
            else
            {
                lastAttack = Time.time;
            }
        }

    }

    #endregion

    #region ORBIT_STATE

    [System.Serializable]
    public class OrbitStateConfig
    {
        public float OrbitRadius = 3f;
        public float DiceRollCooldown = 2f;

        public static OrbitStateConfig DefaultConfig
        {
            get
            {
                return new OrbitStateConfig();
            }
        }
    }

    public class OrbitState : BaseState
    {
        public OrbitStateConfig Cfg;
        public GameObject Target;

        public const int RES_CONTINUE = 0;
        public const int RES_TARGET_IS_NULL = -1;

        protected float lastDistance;
        private float lastPathChange;

        public OrbitState(GameObject obj, OrbitStateConfig config, GameObject target)
            : base(obj)
        {
            Cfg = config;
            Target = target;
        }

        public override StateResult Update()
        {
            if (Target == null)
            {
                return new StateResult(RES_TARGET_IS_NULL);
            }

            float distanceToTarget = Vector3.Distance(gameObject.transform.position, Target.transform.position);

            if (Mathf.Abs(distanceToTarget - lastDistance) > 0.025f)
            {
                navMeshAgent.isStopped = health.IsOnGround;
                float angle = gameObject.GetInstanceID() % 360f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                navMeshAgent.SetDestination(Target.transform.position + offset * Cfg.OrbitRadius);
                lastPathChange = Time.time;
            }

            lastDistance = distanceToTarget;
            return new StateResult(RES_CONTINUE);
        }

    }

    public class OrbitStateEnemy : OrbitState
    {
        public const int RES_NOT_ENOUGH_ATTACKERS = 1;
        public const int RES_ORBIT_ATTACK = 2;

        private float lastAttackRoll;

        public OrbitStateEnemy(GameObject obj, OrbitStateConfig config, GameObject target)
            : base(obj, config, target)
        { }

        public override void OnEnter()
        {
            lastAttackRoll = Time.time;
        }

        public override StateResult Update()
        {
            if (AIManager.Instance.GetNumberOfAttackers(Target) < AIManager.Instance.GetMaxAttackers(Target))
            {
                //MovementStatus = EBrawlerAIStates.Attack;
                return new StateResult(RES_NOT_ENOUGH_ATTACKERS, Target);
            }

            if (Time.time > lastAttackRoll + Cfg.DiceRollCooldown)
            {
                if (UnityEngine.Random.value > 0.75)
                {
                    //MovementStatus = EBrawlerAIStates.OrbitAttack;
                    return new StateResult(RES_ORBIT_ATTACK, Target);
                }

                lastAttackRoll = Time.time;
            }

            return base.Update();
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

        public static HealStateConfig DefaultConfig
        {
            get
            {
                return new HealStateConfig();
            }
        }
    }

    public class HealState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_HEALING = 1; // esperando o cast time (talvez seja desnecessário)
        public const int RES_HEALED = 2;
        public const int RES_CASTING_HEAL = 3; // rodando a animação de heal
        public const int RES_TARGET_IS_DEAD = 4;

        public GameObject Target;
      

        private HealStateConfig Cfg;

        private float healCastT;
        public float LastHeal
        {
            get; private set;
        }

        bool isHealing = false;
        bool isCasting = false;

        public HealState(GameObject gameObject, HealStateConfig cfg, GameObject target) 
            : base(gameObject)
        {
            Cfg = cfg;
            Target = target;

        }

        public override void OnEnter()
        {
            base.OnEnter();
            health.OnDamaged += OnDamagedCallback;
            combat.OnSkillUsed += OnSkillUsedCallback;
        }

        public override void OnExit()
        {
            base.OnExit();
            health.OnDamaged -= OnDamagedCallback;
            combat.OnSkillUsed -= OnSkillUsedCallback;
        }

        private void OnDamagedCallback(CharacterAttackData obj)
        {
            healCastT = 0f;
            isCasting = false;
        }

        private void OnSkillUsedCallback(BaseSkill skill)
        {
            isHealing = false;
            isCasting = false;
            LastHeal = Time.time;
            CombatManager.Heal(data.Stats, Target.GetComponent<CharacterData>().Stats);
        }

        public override StateResult Update()
        {
            if (Target == null)
            {
                return new StateResult(RES_TARGET_IS_DEAD);
            }

            float d = Vector3.Distance(gameObject.transform.position, Target.transform.position);
            if (d > Cfg.MinHealDistance && !isHealing)
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
                isHealing = true;

                healCastT += Time.deltaTime;
                if (healCastT >= Cfg.HealCastTime)
                {
                    // terminou o cast time
                    if (!isCasting && !combat.IsOnCombo && !health.IsOnGround)
                    {
                        combat.RequestSkillUse(new BaseSkill());
                        isCasting = true;
                    }

                    // esperando a animação de cast
                    else
                    {
                        gameObject.transform.forward = (Target.transform.position - gameObject.transform.position).normalized;
                        navMeshAgent.isStopped = true;
                        return new StateResult(RES_CASTING_HEAL, Target);
                    }
                }

                // esperando o cast time
                else
                {
                    gameObject.transform.forward = (Target.transform.position - gameObject.transform.position).normalized;
                    navMeshAgent.isStopped = true;
                    return new StateResult(RES_HEALING, Target);
                }
            }

            else if (healCastT >= Cfg.HealCastTime && !isHealing && !isCasting)
            {
                healCastT = 0f;
                return new StateResult(RES_HEALED, Target);
            }

            return new StateResult(RES_CONTINUE);
        }

        public override void OnGUI()
        {
            base.OnGUI();
            Rect r = UIManager.WorldSpaceGUI(gameObject.transform.position + Vector3.down * 2f, 
                new Vector2(100f*(healCastT/Cfg.HealCastTime), 15f));
            GUI.Box(r, "HealingProgress");
        }
    }

    #endregion

    #region EQUIP_ITEM_STATE

    public class EquipItemState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_ITEM_DESTROYED = 1;
        public const int RES_ITEM_EQUIPPED = 2;
        public const int RES_CANCEL = 3;

        private ItemData targetItem;

        public EquipItemState(GameObject gameObject, ItemData targetItem) : base(gameObject)
        {
            this.targetItem = targetItem;
        }

        public override StateResult Update()
        {
            if (targetItem == null)
            {
                return new StateResult(RES_ITEM_DESTROYED);
            }
            else if (Vector3.Distance(gameObject.transform.position, targetItem.transform.position) < 0.5f)
            {
                if (data.Interact())
                {
                    return new StateResult(RES_ITEM_EQUIPPED, targetItem);
                }
                else
                    return new StateResult(RES_ITEM_DESTROYED);
            }
            else
            {
                if (Vector3.Distance(navMeshAgent.destination, targetItem.transform.position) > 0.5f)
                {
                    navMeshAgent.SetDestination(targetItem.transform.position);
                }

                navMeshAgent.isStopped = false;
                return new StateResult(RES_CONTINUE);
            }


        }
    }


    #endregion

}