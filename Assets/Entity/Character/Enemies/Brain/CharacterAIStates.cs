using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace Catacumba.Entity.AI
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
        protected CharacterMovement movement;
        protected CharacterAnimator animator;

        public BaseState(GameObject gameObject)
        {
            this.gameObject = gameObject;
            data = gameObject.GetComponent<CharacterData>();
            health = gameObject.GetComponent<CharacterHealth>();
            combat = gameObject.GetComponent<CharacterCombat>();
            movement = gameObject.GetComponent<CharacterMovement>();
            animator = gameObject.GetComponent<CharacterAnimator>();
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

            movement.IsAgentStopped = false;
            if (!movement.HasPath || movement.PathStatus == NavMeshPathStatus.PathComplete)
            {
                if (Time.time > lastPathChange + Cfg.SleepTime)
                {
                    movement.SetDestination(gameObject.transform.position + UnityEngine.Random.insideUnitSphere * Cfg.WanderRadius);
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
        public const int RES_TARGET_DESTROYED = 5;

        private StateResult RESULT_CONTINUE = new StateResult { code = RES_CONTINUE };

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

        protected virtual bool IsInAttackPosition(float distanceToTarget)
        {
            bool rangedAttack = data.Stats.Inventory.HasEquip(EInventorySlot.Weapon) &&
                   data.Stats.Inventory[EInventorySlot.Weapon].IsRanged;

            if (rangedAttack)
            {
                return distanceToTarget <= Cfg.DistanceToAttack + 5f;
            }
            else
            {
                return distanceToTarget <= Cfg.DistanceToAttack;
            }
        }

        protected virtual bool IsTargetOutOfSight(float distanceToTarget)
        {
            return distanceToTarget >= Cfg.SightRange;
        }

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
            if (Target == null) return new StateResult { code = RES_TARGET_DESTROYED };

            StateResult result = CheckNumberOfAttackers();
            if (result.code != RES_CONTINUE) return result;

            float distanceToTarget = Vector3.Distance(gameObject.transform.position, Target.transform.position);

            if (IsInAttackPosition(distanceToTarget))
            {
                result = UpdateAttack();
                if (result.code != RES_CONTINUE) return result;
            }
            /*else if (IsTargetOutOfSight(distanceToTarget))
            {
                return new StateResult(RES_OUT_OF_SIGHT);
            }*/
            else
            {
                UpdateDesiredDestination();
            }

            // olha pro alvo
            var dir = (Target.transform.position - gameObject.transform.position).normalized;
            dir.y = gameObject.transform.forward.y;
            gameObject.transform.forward = dir;

            return RESULT_CONTINUE;
        }

        // Se tem muitos atacantes nesse alvo, sair do estado.
        protected virtual StateResult CheckNumberOfAttackers()
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
            return RESULT_CONTINUE;
        }

        protected virtual StateResult UpdateAttack()
	    {
            movement.IsAgentStopped = true;
            if (Time.time > lastAttack + Cfg.AttackCooldown && Time.time > lastCombo + Cfg.ComboCooldown)
            {
                // se terminou o o combo....
                if (currentAttackIndex > comboLength - 1)
                {
                    // e tá numa reação ao ataque de alguém,
                    if (orbitReaction)
                    {
                        // sai desse estado
                        return new StateResult(RES_ORBIT_REACTION_COMBO_END, Target);
                    }

                    // se não, reseta o combo.
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

            return RESULT_CONTINUE;
        }

        protected virtual void UpdateDesiredDestination()
        {
            bool rangedAttack = data.Stats.Inventory.HasEquip(EInventorySlot.Weapon) &&
                data.Stats.Inventory[EInventorySlot.Weapon].IsRanged;

            Vector3 targetPosition;

            if (!rangedAttack)
            {
                targetPosition = Target.transform.position;
            }
            else
            {
                Vector3 pt = Target.transform.position;
                Vector3 pc = gameObject.transform.position;

                float targetDistance = Cfg.DistanceToAttack;
                targetPosition = pt + (pc-pt).normalized * targetDistance;
            }

            if (Vector3.Distance(movement.Destination, targetPosition) > 0.75f)
            {
                movement.SetDestination(targetPosition);
                movement.IsAgentStopped = false;
            }
        }

        protected void OnDamagedCallback(CharacterAttackData attackData)
        {
            if (!health.CanBeKnockedOut)
            {
                //lastAttack -= 0.2f; // HACK
                return;
            }

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

    public class RangedAttackState : BaseState
    {
        public RangedAttackState(GameObject gameObject) : base(gameObject)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
  
        public override StateResult Update()
        {
            return base.Update();
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
                //movement.IsAgentStopped = health.IsOnGround;
                float angle = gameObject.GetInstanceID() % 360f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Cfg.OrbitRadius;
                movement.SetDestination(Target.transform.position + offset);
                lastPathChange = Time.time;
            }

            lastDistance = distanceToTarget;

            gameObject.transform.forward = (Target.transform.position - gameObject.transform.position).normalized;

            return new StateResult(RES_CONTINUE);
        }

    }

    public class OrbitStateEnemy : OrbitState
    {
        public const int RES_NOT_ENOUGH_ATTACKERS = 1;
        public const int RES_ORBIT_ATTACK = 2;
        public const int RES_TARGET_DESTROYED = 3;

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
            if (Target == null) return new StateResult(RES_TARGET_DESTROYED);

            int nAttackers = AIManager.Instance.GetNumberOfAttackers(Target);
            if (nAttackers == -1)
            {
                return new StateResult(RES_TARGET_DESTROYED);
            }

            int maxAttackers = AIManager.Instance.GetMaxAttackers(Target);

            if (nAttackers < maxAttackers)
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
            FX.Instance.EmitHealEffect(Target);
            
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
                float navDistanceToTarget = Vector3.Distance(movement.Destination, Target.transform.position);

                if (navDistanceToTarget > Cfg.MinHealDistance)
                {
                    Vector3 targetPosition = (gameObject.transform.position - Target.transform.position).normalized * Cfg.MinHealDistance;
                    movement.SetDestination(targetPosition);
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
                        movement.IsAgentStopped = false;
                        FX.Instance.EmitHealFlame(animator.ModelInfo.LeftHandBone.Bone.gameObject);
                        FX.Instance.EmitHealFlame(animator.ModelInfo.RightHandBone.Bone.gameObject);
                        combat.RequestSkillUse(new BaseSkill());
                        isCasting = true;
                    }

                    // esperando a animação de cast
                    else
                    {
                        gameObject.transform.forward = (Target.transform.position - gameObject.transform.position).normalized;
                        movement.IsAgentStopped = true;
                        return new StateResult(RES_CASTING_HEAL, Target);
                    }
                }

                // esperando o cast time
                else
                {
                    gameObject.transform.forward = (Target.transform.position - gameObject.transform.position).normalized;
                    movement.IsAgentStopped = true;
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
                if (Vector3.Distance(movement.Destination, targetItem.transform.position) > 0.5f)
                {
                    movement.SetDestination(targetItem.transform.position);
                }

                movement.IsAgentStopped = false;
                return new StateResult(RES_CONTINUE);
            }
        }
    }

    #endregion

    #region USE_SKILL

    public class UseSkillStateCfg
    {
        public int skillIndex;
        public GameObject target;
    }

    public class UseSkillState : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_CASTING = 1;
        public const int RES_CASTED = 2;

        private bool usingSkill;
        private bool usedSkill;
        public int SkilIndex
        {
            get; private set;
        }

        public UseSkillState(GameObject obj, int skillIndex) : base(obj)
        {
            usingSkill = false;
            usedSkill = false;

            SkilIndex = skillIndex;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            animator.OnStartUsingSkill += OnStartUsingSkill;
            animator.OnEndUsingSkill += OnEndUsingSkill;

            combat.UseSkill(SkilIndex);
        }

        public override void OnExit()
        {
            base.OnExit();

            animator.OnStartUsingSkill -= OnStartUsingSkill;
            animator.OnEndUsingSkill -= OnEndUsingSkill;
        }

        private void OnStartUsingSkill(CharacterAnimator obj)
        {
            usingSkill = true;
        }

        private void OnEndUsingSkill(CharacterAnimator obj)
        {
            usingSkill = false;
            usedSkill = true;
        }

        public override StateResult Update()
        {
            if (usedSkill)
                return new StateResult(RES_CASTED);
            else if (usingSkill)
                return new StateResult(RES_CASTING);

            return new StateResult(RES_CONTINUE);
        }
    }

    #endregion

}