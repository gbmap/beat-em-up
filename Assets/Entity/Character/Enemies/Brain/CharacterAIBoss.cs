using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Character.AI
{
    public enum EBossAIStates
    {
        Wander,
        Attack,
        Summon
        // Orbit,
        // OrbitAttack
    }

    public class CharacterAIBoss : CharacterAIBaseMachine<EBossAIStates>
    {
        private GameObject target;
        private float lastDistance;

        [Header("Wander State")]
        public WanderStateConfig WanderStateConfig;

        [Header("Attack State")]
        public AttackStateConfig AttackStateConfig;

        // [Header("Orbit State")]
        // public OrbitStateConfig OrbitStateConfig;

        private float itemCheckTime = 1.0f;
        private float lastItemCheck;

        protected override void Awake()
        {
            base.Awake();

            SetCurrentState(EBossAIStates.Wander);
        }
 
        private void OnEnable()
        {
            health.OnDamaged += OnDamagedCallback;
        }

        private void OnDisable()
        {
            health.OnDamaged -= OnDamagedCallback;
            if (CurrentAIState == EBossAIStates.Attack)
            {
                AttackState attackState = currentState as AttackState;
                AIManager.Instance?.DecreaseAttackers(attackState.Target);
            }
        }

        protected override void Update()
        {
            base.Update();


            // if (Time.time > lastItemCheck + itemCheckTime)
            // {
            //     /* OTIMIZAR ISSO AQUI >>>>EVENTUALMENTE<<<< */
            //     ItemData[] items = FindObjectsOfType<ItemData>().Where(item => Vector3.Distance(gameObject.transform.position, item.transform.position) < 5.0f).ToArray();
            //     //List<ItemData> itemsInRange = characterData.ItemsInRange;
            //
            //     if (items.Length > 0 && CurrentAIState != EBrawlerAIStates.EquipItem)
            //     {
            //         for (int i = 0; i < items.Length; i++)
            //         {
            //             ItemData item = items[i];
            //             if (!data.Stats.Inventory.HasEquip(item.Stats.Slot))
            //             {
            //                 SetCurrentState(EBrawlerAIStates.EquipItem, item);
            //             }
            //         }
            //     }
            //     lastItemCheck = Time.time;
            // }
        }

        private void OnDamagedCallback(CharacterAttackData obj)
        {
            // if (CurrentAIState == EBrawlerAIStates.Orbit)
            // {
            //     if (obj.Attacker.CompareTag("Player"))
            //     {
            //         SetCurrentState(EBrawlerAIStates.OrbitAttack, obj.Attacker);
            //     }
            // }
        }

        protected override BaseState CreateNewState(EBossAIStates previousState, EBossAIStates newState, params object[] data)
        {
            switch (newState)
            {
                case EBossAIStates.Wander:
                    return new WanderState(gameObject, WanderStateConfig);
                case EBossAIStates.Attack:
                    return new AttackState(gameObject, AttackStateConfig, data[0] as GameObject, newState == EBossAIStates.Attack);
                default:
                    return new WanderState(gameObject, WanderStateConfig);
            }
        }

        protected override void HandleStateResult(EBossAIStates state, StateResult result)
        {
            switch (state)
            {
                case EBossAIStates.Attack:
                    if (result.code == AttackState.RES_TOO_MANY_ATTACKERS ||
                        result.code == AttackState.RES_ORBIT_REACTION_COMBO_END)
                    {
                        SetCurrentState(EBossAIStates.Wander, result.data[0] as GameObject);
                    }
                    else if (result.code == AttackState.RES_OUT_OF_SIGHT)
                    {
                        SetCurrentState(EBossAIStates.Wander);
                    }
                    break;
                case EBossAIStates.Wander:
                    if (result.code == WanderState.RES_ENEMY_IN_SIGHT)
                    {
                        SetCurrentState(EBossAIStates.Attack, result.data[0] as GameObject);
                    }
                    break;
            }
        }
    }
}