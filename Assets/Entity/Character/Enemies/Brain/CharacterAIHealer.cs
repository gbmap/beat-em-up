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
        private CharacterHealth MostDamagedAlly
        {
            get { return allies.OrderBy(c => c.Health).First(); }
        }

        float lastDamageCheck;

        void Start()
        {
            allies = UpdateAllies();
            SetCurrentState(EHealerAIStates.Wandering);
        }

        protected override void Update()
        {
            base.Update();

            if (!allies.Any(ally => ally != null))
            {
                UpdateAllies();

                if (CurrentAIState != EHealerAIStates.Wandering)
                {
                    SetCurrentState(EHealerAIStates.Wandering);
                }
            }

            if (Time.time > lastDamageCheck + 1f)
            {
                CharacterHealth mda = MostDamagedAlly;

                bool isWandering = CurrentAIState == EHealerAIStates.Wandering;
                bool isOrbitingAlready = (CurrentAIState == EHealerAIStates.Orbiting) &&
                                         (currentState as OrbitState).Target == mda.gameObject;

                if (isWandering || !isOrbitingAlready)
                {
                    SetCurrentState(EHealerAIStates.Orbiting, mda.gameObject);
                }

                lastDamageCheck = Time.time;
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
                case EHealerAIStates.Orbiting: return new OrbitState(gameObject, OrbitStateConfig.DefaultConfig, data[0] as GameObject);
                default: return new WanderState(gameObject, WanderStateConfig.DefaultConfig);
            }
        }

        protected override void HandleStateResult(EHealerAIStates state, StateResult result)
        {
            switch(state)
            {
                case EHealerAIStates.Orbiting:
                    if (result.code == OrbitState.RES_CONTINUE)
                    {
                        CharacterHealth health = (currentState as OrbitState).Target.GetComponent<CharacterHealth>();
                        if (health.HealthNormalized < 0.8f)
                        {
                            SetCurrentState(EHealerAIStates.Healing, health.gameObject);
                        }
                    }
                    break;
            }

        }
    }
}