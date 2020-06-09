using Frictionless;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Catacumba.Character.AI
{
    public class MsgOnBossDied { }

    public enum EBossAIStates
    {
        UseSkill,
        Wait,
        WaitMinionsDie,
    }

    public enum EBossSkills
    {
        GroundSmash, // spawna rochas de fogo
        OneSlash, // ataque unico, pouca antecipação
        ThreeSlashes, // 3 ataques, bastante antecipação
        SwordSlashesSlow, // 2 ataques, muita antecipação
        FireBreath, 
        SpawnMinions 
    }

    public class AIBossStateMachine : CharacterAIBaseMachine<EBossAIStates>
    {
        [Range(1, 6)]
        public int NumberOfSkills = 6;
        private int lastSkill;

        [Range(1f, 10f)]
        public float Cooldown = 2f;

        [Header("Skill References")]
        public SkillSpawnEnemies SpawnSkill;

        private GameObject target;
        private ShuffleBag<EBossSkills> SkillWeights;
        private int tier = 0;
        private float nextTier = .75f;

        private void OnEnable()
        {
            target = FindTarget();

            SkillWeights = new ShuffleBag<EBossSkills>();
            SkillWeights.Add(EBossSkills.SwordSlashesSlow);
            SkillWeights.Add(EBossSkills.ThreeSlashes);

            health.OnDamaged += OnDamagedCallback;
            health.OnDeath += OnDeathCallback;

            SetCurrentState(EBossAIStates.Wait);
        }

        private void OnDisable()
        {
            health.OnDamaged -= OnDamagedCallback;
            health.OnDeath -= OnDeathCallback;
        }
             

        protected override void Update()
        {
            base.Update();

            if (target == null)
            {
                target = FindTarget();
            }
            else if (CurrentAIState != EBossAIStates.UseSkill)
            {
                Vector3 tp = target.transform.position;
                tp.y = transform.position.y;
                
                transform.LookAt(tp);
            }
        }

        protected override BaseState CreateNewState(EBossAIStates previousState, EBossAIStates currentState, params object[] data)
        {
            switch (currentState)
            {
                case EBossAIStates.Wait:
                    return new StateWait(gameObject, Cooldown);
                case EBossAIStates.UseSkill:
                    return new UseSkillState(gameObject, (int)data[0]);
                case EBossAIStates.WaitMinionsDie:
                    return new StateWaitMinionsDie(gameObject, data[0] as List<GameObject>);
            }

            return new UseSkillState(gameObject, (int)data[0]);
        }

        protected override void HandleStateResult(EBossAIStates state, StateResult result)
        {
            switch (state)
            {
                case EBossAIStates.Wait:
                    switch (result.code)
                    {
                        case StateWait.RES_CONTINUE: break;
                        case StateWait.RES_END_WAIT:
                            {
                                if (target != null)
                                {
                                    SetCurrentState(EBossAIStates.UseSkill, GetNextSkill(target));
                                }
                                break;
                            }
                        default: throw new System.Exception("Unexpected state result at EBossAIStates.Wait: " + result.code);
                    }
                    break;
                case EBossAIStates.UseSkill:
                    switch (result.code)
                    {
                        case UseSkillState.RES_CASTED:
                            if ((currentState as UseSkillState).SkilIndex == (int)EBossSkills.SpawnMinions ||
                                SpawnSkill.Minions.Count > 0)
                            {
                                SetCurrentState(EBossAIStates.WaitMinionsDie, SpawnSkill.Minions);
                                break;
                            }
                            else
                            {
                                SetCurrentState(EBossAIStates.Wait, Cooldown);
                                break;
                            }
                        default:
                            break;
                    }
                    break;

                case EBossAIStates.WaitMinionsDie:
                    switch (result.code)
                    {
                        case StateWaitMinionsDie.RES_MINIONS_DIED:
                            {
                                SetCurrentState(EBossAIStates.Wait);
                                break;
                            }
                        default: break;
                    }
                    break;

                default: throw new System.Exception("Unexpected state result at state " + state.ToString() + ": " + result.code);
            }
        }

        private void OnDamagedCallback(CharacterAttackData data)
        {
            if (health.HealthNormalized <= nextTier)
            {
                tier++;
                Cooldown *= .5f;
            }

            switch (tier)
            {
                case 1: SkillWeights.Add(EBossSkills.OneSlash); break;
                case 2:
                    {
                        SkillWeights.Add(EBossSkills.OneSlash);
                        SkillWeights.Add(EBossSkills.SpawnMinions);
                        break;
                    }
                case 3: SkillWeights.Add(EBossSkills.FireBreath); break;
            }

            if (CurrentAIState == EBossAIStates.WaitMinionsDie)
            {
                SetCurrentState(EBossAIStates.UseSkill, EBossSkills.OneSlash);
            }

            if (data.DefenderStats.Health <= 0)
            {
                foreach (GameObject minion in SpawnSkill.Minions)
                {
                    var d = minion.GetComponent<CharacterData>();
                    CharacterAttackData att = new CharacterAttackData()
                    {
                        Damage = int.MaxValue,
                    };
                    CombatManager.CalculateAttackStats(null, d.Stats, ref att);
                    d.GetComponent<CharacterHealth>()?.TakeDamage(att);
                }
            }
        }

        private void OnDeathCallback(CharacterHealth obj)
        {
            ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new MsgOnBossDied { });
        }

        private EBossSkills GetNextSkill(GameObject target)
        {
            float d = Vector3.Distance(target.transform.position, gameObject.transform.position);
            if (d > 3f)
            {
                return UnityEngine.Random.value > 0.5f ? EBossSkills.GroundSmash : EBossSkills.SpawnMinions;
            }

            return SkillWeights.Next();
        }

        private GameObject FindTarget()
        {
            var players = FindObjectsOfType<CharacterPlayerInput>();
            if (players.Length == 0) return null;
            return players.OrderBy(p => Vector3.Distance(gameObject.transform.position, p.transform.position)).FirstOrDefault().gameObject;
        }
    }

    public class StateWait : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_END_WAIT = 1;

        float waitTime;
        float timer;

        public StateWait(GameObject gameObject, float waitTime) : base(gameObject)
        {
            this.waitTime = waitTime;
        }

        public override StateResult Update()
        {
            timer += Time.deltaTime;
            return new StateResult(timer >= waitTime ? RES_END_WAIT : RES_CONTINUE);
        }
    }

    public class StateWaitMinionsDie : BaseState
    {
        public const int RES_CONTINUE = 0;
        public const int RES_MINIONS_DIED = 1;

        GameObject[] minions;

        public StateWaitMinionsDie(GameObject gameObject, List<GameObject> minions) : base(gameObject)
        {
            this.minions = minions.ToArray();
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
            return new StateResult(minions.Any(m => m != null) ? RES_CONTINUE : RES_MINIONS_DIED);
        }
    }
}