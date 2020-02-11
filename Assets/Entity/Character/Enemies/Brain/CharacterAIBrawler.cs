using System;
using System.Collections.Generic;
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
        EquipItem
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

        private float itemCheckTime = 1.0f;
        private float lastItemCheck;

        protected override void Awake()
        {
            base.Awake();

            SetCurrentState(EBrawlerAIStates.Wander);
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
                AttackState attackState = currentState as AttackState;
                AIManager.Instance?.DecreaseAttackers(attackState.Target);
            }
        }

        protected override void Update()
        {
            base.Update();


            if (Time.time > lastItemCheck + itemCheckTime)
            {
                /* OTIMIZAR ISSO AQUI >>>>EVENTUALMENTE<<<< */
                ItemData[] items = FindObjectsOfType<ItemData>().Where(item => Vector3.Distance(gameObject.transform.position, item.transform.position) < 5.0f).ToArray();
                //List<ItemData> itemsInRange = characterData.ItemsInRange;

                if (items.Length > 0 && CurrentAIState != EBrawlerAIStates.EquipItem)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        ItemData item = items[i];
                        if (!characterData.Stats.Inventory.HasEquip(item.Stats.Slot))
                        {
                            SetCurrentState(EBrawlerAIStates.EquipItem, item);
                        }
                    }
                }
                lastItemCheck = Time.time;
            }
        }

        private void OnDamagedCallback(CharacterAttackData obj)
        {
            if (CurrentAIState == EBrawlerAIStates.Orbit)
            {
                if (obj.Attacker.CompareTag("Player"))
                {
                    SetCurrentState(EBrawlerAIStates.OrbitAttack, obj.Attacker);
                }
            }
        }

        protected override BaseState CreateNewState(EBrawlerAIStates previousState, EBrawlerAIStates newState, params object[] data)
        {
            switch (newState)
            {
                case EBrawlerAIStates.Wander:
                    return new WanderState(gameObject, WanderStateConfig);
                case EBrawlerAIStates.Orbit:
                    return new OrbitStateEnemy(gameObject, OrbitStateConfig, data[0] as GameObject);
                case EBrawlerAIStates.OrbitAttack:
                case EBrawlerAIStates.Attack:
                    return new AttackState(gameObject, AttackStateConfig, data[0] as GameObject, newState == EBrawlerAIStates.OrbitAttack);
                case EBrawlerAIStates.EquipItem:
                    return new EquipItemState(gameObject, data[0] as ItemData);
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
                        SetCurrentState(EBrawlerAIStates.Orbit, result.data[0] as GameObject);
                    }
                    else if (result.code == AttackState.RES_OUT_OF_SIGHT)
                    {
                        SetCurrentState(EBrawlerAIStates.Wander);
                    }
                    break;
                case EBrawlerAIStates.Orbit:
                    switch (result.code)
                    {
                        case OrbitStateEnemy.RES_TARGET_IS_NULL:
                            SetCurrentState(EBrawlerAIStates.Wander);
                            break;
                        case OrbitStateEnemy.RES_NOT_ENOUGH_ATTACKERS:
                            SetCurrentState(EBrawlerAIStates.Attack, result.data[0] as GameObject);
                            break;
                        case OrbitStateEnemy.RES_ORBIT_ATTACK:
                            SetCurrentState(EBrawlerAIStates.OrbitAttack, result.data[0] as GameObject);
                            break;
                    }
                    break;
                
                case EBrawlerAIStates.Wander:
                    if (result.code == WanderState.RES_ENEMY_IN_SIGHT)
                    {
                        SetCurrentState(EBrawlerAIStates.Orbit, result.data[0] as GameObject);
                    }
                    break;
                case EBrawlerAIStates.EquipItem:
                    if (result.code != EquipItemState.RES_CONTINUE)
                    {
                        SetCurrentState(EBrawlerAIStates.Wander);
                    }
                    break;
            }
        }
    }
}