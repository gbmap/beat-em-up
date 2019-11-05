using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAIMovementInput : MonoBehaviour
{
    [Header("Follow State")]
    public float SightRange = 5f;
    private GameObject target;

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
    public float WanderRadius = 1f;
    public float SleepTime = 2f;
    private float lastPathChange;

    private NavMeshAgent navMeshAgent;

    private CharacterHealth characterHealth;
    private CharacterCombat characterCombat;

    private enum EMovementStatus
    {
        Wandering,
        FollowingEnemy,
        Attacking
    }

    private EMovementStatus movementStatus;
    private EMovementStatus MovementStatus
    {
        get { return movementStatus; }
        set
        {
            var animator = GetComponent<Animator>();
            animator.ResetTrigger("WeakAttack");
            animator.ResetTrigger("StrongAttack");

            movementStatus = value;
            switch (movementStatus)
            {
                case EMovementStatus.Attacking:
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

        UpdateTarget();
    }

    private void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, navMeshAgent.destination);
        if (target == null)
        {
            MovementStatus = EMovementStatus.Wandering;
        }

        switch (MovementStatus)
        {
            case EMovementStatus.FollowingEnemy:
                Follow(target.transform, distanceToTarget);
                break;
            case EMovementStatus.Attacking:
                Attack(distanceToTarget);
                break;
            case EMovementStatus.Wandering:
                Wander(distanceToTarget);
                break;
        }
    }
    
    void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        target = players.OrderBy(p => Vector3.Distance(p.transform.position, transform.position)).FirstOrDefault();
    }

    void Follow(Transform target, float distanceToTarget)
    {
        navMeshAgent.isStopped = characterHealth.IsOnGround;
        navMeshAgent.SetDestination(target.position);
        if (distanceToTarget <= DistanceToAttack)
        {
            MovementStatus = EMovementStatus.Attacking;
        }
        else if (distanceToTarget >= SightRange)
        {
            MovementStatus = EMovementStatus.Wandering;
        }
    }

    void Attack(float distanceToTarget)
    {
        navMeshAgent.isStopped = true;
        if (distanceToTarget <= DistanceToAttack)
        {
            if (Time.time > lastAttack + AttackCooldown && Time.time > lastCombo + ComboCooldown)
            {
                var attackType = combo[(currentAttackIndex++) % comboLength];
                characterCombat.RequestAttack(attackType);
                lastAttack = Time.time;

                if (currentAttackIndex == MaxComboHits-1)
                {
                    currentAttackIndex = 0;
                    lastCombo = Time.time;
                }
            }
        }
        else
        {
            currentAttackIndex = 0;
            MovementStatus = distanceToTarget >= SightRange ? EMovementStatus.Wandering : EMovementStatus.FollowingEnemy;
        }
    }

    void Wander(float distanceToTarget)
    {
        if (distanceToTarget < SightRange)
        {
            MovementStatus = EMovementStatus.FollowingEnemy;
            return;
        }
        
        navMeshAgent.isStopped = false;
        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            if (Time.time > lastPathChange + SleepTime)
            {
                navMeshAgent.SetDestination(Random.insideUnitSphere * WanderRadius);
                lastPathChange = Time.time;
            }
        }
    }
}
