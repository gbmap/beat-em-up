using System;
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
    public float OrbitRadius = 2f;
    public float DiceRollCooldown = 2f;
    private float lastAttackRoll;

    private NavMeshAgent navMeshAgent;

    private CharacterHealth characterHealth;
    private CharacterCombat characterCombat;

    private enum EMovementStatus
    {
        Wandering,
        Attacking,
        Orbiting,
        OrbitAttack,
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
                case EMovementStatus.Orbiting:
                    lastAttackRoll = Time.time;
                    break;
                case EMovementStatus.OrbitAttack:
                case EMovementStatus.Attacking:
                    if (movementStatus == EMovementStatus.Attacking)
                    {
                        AIManager.Instance.IncreaseAttackers(target);
                    }
                    //nAttackers++;
                    lastAttack = Time.time;
                    comboLength = UnityEngine.Random.Range(1, MaxComboHits);
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
            case EMovementStatus.OrbitAttack:
                OrbitAttackState(target.transform, distanceToTarget);
                break;
        }

        lastDistance = distanceToTarget;
    }

    private void OnEnable()
    {
        characterHealth.OnDamaged += OnDamagedCallback;
    }

    private void OnDisable()
    {
        characterHealth.OnDamaged -= OnDamagedCallback;
        if (MovementStatus == EMovementStatus.Attacking)
        {
            AIManager.Instance.DecreaseAttackers(target);
        }
    }

    private void OnDamagedCallback(CharacterAttackData obj)
    {
        if (MovementStatus == EMovementStatus.Orbiting)
        {
            MovementStatus = EMovementStatus.OrbitAttack;
        }
    }

    void UpdateTarget()
    {
        if (target != null)
        {
            AIManager.Instance.ClearTarget(target);
        }
        target = AIManager.Instance.GetTarget(gameObject);
    }

    void AttackState(Transform target, float distanceToTarget, bool orbitReaction = false)
    {
        if (AIManager.Instance.GetNumberOfAttackers(target.gameObject) > AIManager.Instance.GetMaxAttackers(target.gameObject) && !orbitReaction)
        {
            MovementStatus = EMovementStatus.Orbiting;
            return;
        }
        
        if (distanceToTarget <= DistanceToAttack)
        {
            navMeshAgent.isStopped = true;
            if (Time.time > lastAttack + AttackCooldown && Time.time > lastCombo + ComboCooldown)
            {
                if (currentAttackIndex >= comboLength - 1)
                {
                    if (orbitReaction)
                    {
                        MovementStatus = EMovementStatus.Orbiting;
                        return;
                    }
                    else
                    {
                        currentAttackIndex = 0;
                        lastCombo = Time.time;
                    }
                }

                var attackType = combo[(currentAttackIndex++) % comboLength];
                characterCombat.RequestAttack(attackType);
                lastAttack = Time.time;
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
                navMeshAgent.SetDestination(transform.position + UnityEngine.Random.insideUnitSphere * WanderRadius);
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

        if (Mathf.Abs(distanceToTarget - lastDistance) > 0.025f)
        {
            {
                navMeshAgent.isStopped = characterHealth.IsOnGround;
                float angle = gameObject.GetInstanceID() % 360f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                navMeshAgent.SetDestination(target.transform.position + offset * OrbitRadius);
                lastPathChange = Time.time;
            }
        }

        if (Time.time > lastAttackRoll + DiceRollCooldown)
        {
            if (UnityEngine.Random.value > 0.75)
            {
                MovementStatus = EMovementStatus.OrbitAttack;
                return;
            }

            lastAttackRoll = Time.time;
        }
    }

    void OrbitAttackState(Transform target, float distanceToTarget)
    {
        AttackState(target, distanceToTarget, true);
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

    private void OnGUI()
    {
        if (!Application.isEditor) return;

        Vector3 posW = transform.position;
        //posW.y = -posW.y;

        Vector2 pos = Camera.main.WorldToScreenPoint(posW);

        Rect r = new Rect(pos, Vector2.one * 100f);
        r.y = Screen.height - pos.y;
        GUI.Label(r, "State: " + MovementStatus);
    }
}
