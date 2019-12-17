using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Character.AI
{
    public class CharacterAIHealer : MonoBehaviour
    {
        private CharacterHealth[] allies;
        private NavMeshAgent navMeshAgent;

        [Header("Wander State")]
        public WanderStateConfig WanderStateConfig;

        [Header("Attack State")]
        public AttackStateConfig AttackStateConfig;

        [Header("Orbit State")]
        public OrbitStateConfig OrbitStateConfig;

        private WanderState wanderState;
        private AttackState attackState;
        private OrbitState orbitState;

        float lastDamageCheck;

        private enum EMovementStatus
        {
            Wandering,
            Avoiding,
            Healing,
        }

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

        }

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
    }
}