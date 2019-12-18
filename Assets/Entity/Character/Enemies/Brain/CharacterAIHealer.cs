using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Character.AI
{
    public enum EHealerAIStates
    {
        Wandering,
        Orbiting,
        Healing,
    }

    public class CharacterAIHealer : CharacterAIBaseMachine<EHealerAIStates>
    {
        private CharacterHealth[] allies;

        [Header("Wander State")]
        public WanderStateConfig WanderStateConfig;

        [Header("Orbit State")]
        public OrbitStateConfig OrbitStateConfig;

        float lastDamageCheck;

        void Start()
        {
            allies = UpdateAllies();
        }

        protected override void Update()
        {
            base.Update();

            if (Time.time > lastDamageCheck + 1f)
            {
                CharacterHealth mostDamagedAlly = allies.OrderBy(c => c.Health).First();

                if (mostDamagedAlly.Health < 0.5f)
                {
                    SetCurrentState(EHealerAIStates.Healing, mostDamagedAlly.gameObject);
                }

            }
        }

        CharacterHealth[] UpdateAllies()
        {
            GameObject[] allAllies = GameObject.FindGameObjectsWithTag("Enemy");
            return allAllies.Select(g => g.GetComponent<CharacterHealth>())
                .Where(ally => Vector3.Distance(transform.position, ally.transform.position) < 30f)
                .ToArray();
        }

        protected override BaseState CreateNewState(EHealerAIStates previousState, EHealerAIStates currentState, params object[] data)
        {
            switch (currentState)
            {
                case EHealerAIStates.Healing: return new HealState(gameObject, HealStateConfig.DefaultConfig, data[0] as GameObject);
                default: return new WanderState(gameObject, WanderStateConfig.DefaultConfig);
            }
        }

        protected override void HandleStateResult(EHealerAIStates state, StateResult result)
        {

        }
    }
}