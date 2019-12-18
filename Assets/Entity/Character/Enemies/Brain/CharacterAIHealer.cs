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

        void Update()
        {
            if (Time.time > lastDamageCheck + 1f)
            {
                CharacterHealth mostDamagedAlly = allies.OrderBy(c => c.Health).First();
            }
        }

        CharacterHealth[] UpdateAllies()
        {
            GameObject[] allAllies = GameObject.FindGameObjectsWithTag("Enemy");
            return allAllies.Select(g => g.GetComponent<CharacterHealth>())
                .Where(ally => Vector3.Distance(transform.position, ally.transform.position) < 30f)
                .ToArray();
        }

        protected override BaseState CreateNewState(EHealerAIStates previousState, EHealerAIStates currentState)
        {
            return new WanderState(gameObject, WanderStateConfig.DefaultConfig);
        }

        protected override void HandleStateResult(EHealerAIStates state, StateResult result)
        {

        }
    }
}