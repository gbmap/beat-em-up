using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Character.AI
{
    /*
    public class CharacterAIHealer : CharacterAIBase
    {
        private CharacterHealth[] allies;
        private NavMeshAgent navMeshAgent;

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

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void WanderState()
        {
            navMeshAgent.isStopped = false;
            if (!navMeshAgent.hasPath || navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                if (Time.time > lastPathChange + SleepTime)
                {
                    navMeshAgent.SetDestination(transform.position + UnityEngine.Random.insideUnitSphere * WanderRadius);
                    lastPathChange = Time.time;
                }
            }
        }

        void UpdateAllies()
        {
            GameObject[] allAllies = GameObject.FindGameObjectsWithTag("Enemy");
            allies = allAllies.Select(g => g.GetComponent<CharacterHealth>())
                .Where(ally => Vector3.Distance(transform.position, ally.transform.position) < 30f)
                .ToArray();
        }
    }
    */
}