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

    public class CharacterAIBrawler : CharacterAIBaseMachine<EBrawlerAIStates>
    {
        private GameObject target;
        private float lastDistance;

        [Header("Wander State")]
        public WanderStateConfig WanderStateConfig;

        [Header("Attack State")]
        public AttackStateConfig AttackStateConfig;

        [Header("Orbit State")]
        public OrbitStateConfig OrbitStateConfig;

        protected override void Awake()
        {
            base.Awake();

            CurrentAIState = EBrawlerAIStates.Wander;
        }

        private void Update()
        {
            if (target == null && CurrentAIState != EBrawlerAIStates.Wander)
            {
                CurrentAIState = EBrawlerAIStates.Wander;
            }

            if (currentState != null)
            {
                StateResult result = currentState.Update();
                HandleStateResult(CurrentAIState, result);
            }
        }

        private void OnEnable()
        {
            characterHealth.OnDamaged += OnDamagedCallback;
        }

        private void OnDisable()
        {
            characterHealth.OnDamaged -= OnDamagedCallback;
            if (CurrentAIState == EBrawlerAIStates.Attack)
            {
                AIManager.Instance?.DecreaseAttackers(target);
            }
        }

        private void OnDamagedCallback(CharacterAttackData obj)
        {
            if (CurrentAIState == EBrawlerAIStates.Orbit)
            {
                CurrentAIState = EBrawlerAIStates.OrbitAttack;
            }
        }

        protected override BaseState CreateNewState(EBrawlerAIStates previousState, EBrawlerAIStates newState)
        {
            switch (newState)
            {
                case EBrawlerAIStates.Wander:
                    return new WanderState(gameObject, WanderStateConfig);
                case EBrawlerAIStates.Orbit:
                    return new OrbitState(gameObject, OrbitStateConfig, target.transform);
                case EBrawlerAIStates.OrbitAttack:
                case EBrawlerAIStates.Attack:
                    return new AttackState(gameObject, AttackStateConfig, target.transform, currentAIState == EBrawlerAIStates.OrbitAttack);
                default:
                    return new WanderState(gameObject, WanderStateConfig);
            }
        }

        protected override void HandleStateResult(EBrawlerAIStates state, StateResult result)
        {
            switch (state)
            {
                case EBrawlerAIStates.OrbitAttack:
                case EBrawlerAIStates.Attack:
                    if (result.code == AttackState.RES_TOO_MANY_ATTACKERS ||
                        result.code == AttackState.RES_ORBIT_REACTION_COMBO_END)
                    {
                        CurrentAIState = EBrawlerAIStates.Orbit;
                    }
                    else if (result.code == AttackState.RES_OUT_OF_SIGHT)
                    {
                        CurrentAIState = EBrawlerAIStates.Wander;
                    }
                    break;
                case EBrawlerAIStates.Orbit:
                    if (result.code == OrbitState.RES_NOT_ENOUGH_ATTACKERS)
                    {
                        CurrentAIState = EBrawlerAIStates.Attack;
                    }
                    else if (result.code == OrbitState.RES_ORBIT_ATTACK)
                    {
                        CurrentAIState = EBrawlerAIStates.OrbitAttack;
                    }
                    break;
                
                case EBrawlerAIStates.Wander:
                    if (result.code == WanderState.RES_ENEMY_IN_SIGHT)
                    {
                        target = (result.data[0] as Transform).gameObject;
                        CurrentAIState = EBrawlerAIStates.Orbit;
                    }
                    break;
            }
        }
    }
}