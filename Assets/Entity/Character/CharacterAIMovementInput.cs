using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAIMovementInput : MonoBehaviour
{
    /*private static int nAttackers = 0;
    private static int MaxAttackers = 1;*/

    private GameObject target;
    private float lastDistance;

    [Header("Attack State")]
    public float DistanceToAttack = 1.5f;
    public float AttackCooldown = 0.15f;
    private float lastAttack;

    public int MaxComboHits = 5;
    private int comboLength;

    public float ComboCooldown = 4f;
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

    private float lastCombo;
    
    [Header("Wander State")]
    public float SightRange = 5f;
    public float WanderRadius = 1f;
    public float SleepTime = 2f;
    private float lastPathChange;

    [Header("Orbit State")]
    public float OrbitRadius = 6f;

    private NavMeshAgent navMeshAgent;

    private CharacterHealth characterHealth;
    private CharacterCombat characterCombat;

    private enum EMovementStatus
    {
        Wandering,
        Attacking,
        Orbiting,
    }

    private EMovementStatus movementStatus;
    private EMovementStatus MovementStatus
    {
        get { return movementStatus; }
        set
        {
            if (value == movementStatus) return;

            var animator = GetComponent<Animator>();
            animator.ResetTrigger("WeakAttack");
            animator.ResetTrigger("StrongAttack");

            if (movementStatus == EMovementStatus.Attacking && value != movementStatus)
            {
                AIManager.Instance.DecreaseAttackers(target);
                //nAttackers--;
            }

            movementStatus = value;
            switch (movementStatus)
            {
                case EMovementStatus.Attacking:
                    AIManager.Instance.IncreaseAttackers(target);
                    //nAttackers++;
                    lastAttack = Time.time;
                    comboLength = Random.Range(1, MaxComboHits);
                    return;
            }
        }
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        characterHealth = GetComponent<CharacterHealth>();
        characterCombat = GetComponent<CharacterCombat>();

        MovementStatus = EMovementStatus.Orbiting;

        UpdateTarget();
    }

    private void Update()
    {
        if (target == null)
        {
            MovementStatus = EMovementStatus.Wandering;
        }

        float distanceToTarget = target != null ? Vector3.Distance(transform.position, target.transform.position) : float.MaxValue;
        switch (MovementStatus)
        {
            case EMovementStatus.Attacking:
                AttackState(target.transform, distanceToTarget);
                break;
            case EMovementStatus.Wandering:
                WanderState(distanceToTarget);
                break;
            case EMovementStatus.Orbiting:
                OrbitState(distanceToTarget);
                break;
        }

        lastDistance = distanceToTarget;
    }

    private void OnDisable()
    {
        if (MovementStatus == EMovementStatus.Attacking)
        {
            AIManager.Instance.DecreaseAttackers(target);
            //nAttackers--;
        }
    }

    void UpdateTarget()
    {
        /*GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        target = players.OrderBy(p => Vector3.Distance(p.transform.position, transform.position)).FirstOrDefault();*/
        if (target != null)
        {
            AIManager.Instance.ClearTarget(target);
        }
        target = AIManager.Instance.GetTarget(gameObject);
    }

    void AttackState(Transform target, float distanceToTarget)
    {
        if (AIManager.Instance.GetNumberOfAttackers(target.gameObject) > AIManager.Instance.GetMaxAttackers(target.gameObject))
        {
            MovementStatus = EMovementStatus.Orbiting;
            return;
        }
        
        if (distanceToTarget <= DistanceToAttack)
        {
            navMeshAgent.isStopped = true;
            if (Time.time > lastAttack + AttackCooldown && Time.time > lastCombo + ComboCooldown)
            {
                var attackType = combo[(currentAttackIndex++) % comboLength];
                characterCombat.RequestAttack(attackType);
                lastAttack = Time.time;

                if (currentAttackIndex == MaxComboHits - 1)
                {
                    currentAttackIndex = 0;
                    lastCombo = Time.time;
                }
            }
        }
        else if (distanceToTarget >= SightRange)
        {
            MovementStatus = EMovementStatus.Wandering;
        }
        else
        {
            navMeshAgent.isStopped = characterHealth.IsOnGround;
            navMeshAgent.SetDestination(target.position);
        }
    }

    void WanderState(float distanceToTarget)
    {
        if (distanceToTarget < SightRange)
        {
            MovementStatus = EMovementStatus.Orbiting;
            return;
        }
        
        navMeshAgent.isStopped = false;
        if (!navMeshAgent.hasPath || navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            if (Time.time > lastPathChange + SleepTime)
            {
                navMeshAgent.SetDestination(transform.position + Random.insideUnitSphere * WanderRadius);
                lastPathChange = Time.time;
            }
        }
    }

    void OrbitState(float distanceToTarget)
    {
        if (AIManager.Instance.GetNumberOfAttackers(target.gameObject) < AIManager.Instance.GetMaxAttackers(target.gameObject))
        {
            MovementStatus = EMovementStatus.Attacking;
            return;
        }

        if (!navMeshAgent.hasPath || 
            navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete ||
            distanceToTarget != lastDistance)
        {
            if (Time.time > lastPathChange + SleepTime)
            {
                float angle = gameObject.GetInstanceID() % 360f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                navMeshAgent.SetDestination(target.transform.position + offset * OrbitRadius);
                lastPathChange = Time.time;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (navMeshAgent == null || !navMeshAgent.hasPath) return;

        var path = navMeshAgent.path;
        for (int i = 1; i < path.corners.Length; i++)
        {
            var a = path.corners[i - 1];
            var b = path.corners[i];
            Gizmos.DrawLine(a, b);
        }
    }
}
