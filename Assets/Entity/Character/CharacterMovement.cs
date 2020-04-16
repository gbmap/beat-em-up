using System;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    // === REFS
    CharacterHealth health;
    CharacterCombat combat;
    CharacterData data;

    // ==== MOVEMENT
    [HideInInspector]
    public Vector3 Direction;
    public Vector3 Velocity { get { return brainType == ECharacterBrainType.AI ? NavMeshAgent.velocity : CalculateVelocity(Direction.normalized); } }
    /*private Vector3 velocity;*/

    private Vector3 forward;

    public float MoveSpeed = 3f;
    private float speedFactor = 1f;

    private Rigidbody _rigidbody;

    private float speedBumpT;
    private Vector3 _speedBumpDir;
    public Vector3 SpeedBumpDir
    {
        get { return _speedBumpDir; }
    }

    public System.Action OnJump;
    public System.Action OnRoll;
    public System.Action OnRollEnded;

    private bool isRolling;
    public bool IsRolling
    {
        get { return isRolling; }
    }

    private float rollSpeedT;
    private float lastRoll;
    private Vector3 rollDirection;

    public bool CanMove
    {
        get
        {
            return !combat.IsOnCombo &&
                !health.IsOnGround &&
                !health.IsBeingDamaged &&
                !IsBeingMoved &&
                Time.time > (combat.LastDamageData.Time + (combat.LastDamageData.Type == EAttackType.Strong ? 0.25f : 0.75f));
        }
    }
    public bool IsBeingMoved { get { return speedBumpT > 0f; } }

    public bool IgnoreSpeedBump = false;

    #region INTERFACE WITH NAVMESH

    public NavMeshAgent NavMeshAgent
    {
        get; private set;
    }

    private bool isAgentStopped;
    public bool IsAgentStopped
    {
        get { return !CanMove || isAgentStopped; }
        set
        {
            isAgentStopped = value;
        }
    }

    public NavMeshPathStatus PathStatus
    {
        get { return NavMeshAgent.pathStatus; }
    }

    public Vector3 Destination
    {
        get { return NavMeshAgent.destination; }
    }

    public bool HasPath { get { return NavMeshAgent.hasPath; } }

    public bool SetDestination(Vector3 pos)
    {
        return NavMeshAgent.SetDestination(pos);
    }

    #endregion


    [Header("Dash when attacks")]
    public float SpeedBumpForce = 0.9f;
    
    private const float speedBumpScale = 7f;

    private ECharacterBrainType brainType
    {
        get
        {
            return data.BrainType;
        }
    }

    private void Awake()
    {
        data = GetComponent<CharacterData>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        health = GetComponent<CharacterHealth>();

        health.OnDamaged += OnDamagedCallback;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        NavMeshAgent.speed = MoveSpeed * (data.BrainType == ECharacterBrainType.AI ? 0.75f : 1f);
    }

    private void OnEnable()
    {
        combat.OnCharacterAttack += OnCharacterAttackCallback;
        combat.OnRequestCharacterAttack += OnCharacterRequestAttackCallback;
    }

    private void OnDisable()
    {
        combat.OnCharacterAttack -= OnCharacterAttackCallback;
        combat.OnRequestCharacterAttack -= OnCharacterRequestAttackCallback;
    }

    // Update is called once per frame
    void Update()
    {
        //Move1();
        Move_Old();

#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.F4))
        {
            showDebug = !showDebug;
        }
#endif
    }

    void Move1()
    {
        Vector3 velocity = CalculateVelocity(Direction.normalized);

        Vector3 bumpDirection = CalculateSpeedBump();
        NavMeshAgent.Move((velocity + bumpDirection) * Time.deltaTime);

        forward = CalculateLookDir(velocity, forward);
        transform.LookAt(transform.position + forward);
    }

    Vector3 CalculateVelocity(Vector3 direction)
    {
        bool canMove = !combat.IsOnCombo && !health.IsOnGround && speedBumpT == 0f;
        float targetFactor = canMove ? 1f : 0f;
        speedFactor = Mathf.Lerp(speedFactor, targetFactor, Time.deltaTime * (4f + targetFactor * 2f));

        return Vector3.ClampMagnitude(direction, 1f) * MoveSpeed * speedFactor * (data.BrainType == ECharacterBrainType.AI ? 0.85f : 1f);
    }

    Vector3 CalculateLookDir(Vector3 velocity, Vector3 currentForward)
    {
        Vector3 targetForward = Vector3.Lerp(Vector3.zero, velocity.normalized, velocity.sqrMagnitude);
        return Vector3.Slerp(currentForward, targetForward, Time.deltaTime * 15f).normalized;
    }

    Vector3 CalculateSpeedBump()
    {
        float t = 1f - speedBumpT;
        var dir = speedBumpScale * _speedBumpDir * Mathf.Pow(-t + 1f, 3f);

        speedBumpT = Mathf.Max(0, speedBumpT - Time.deltaTime * 2f);
        return dir;
    }

    void Move_Old()
    {
      
        Vector3 velocity = Vector3.zero;

        if (IsBeingMoved)
        {
            // applies dash on attack
            float t = 1f - speedBumpT;
            var dir = speedBumpScale * _speedBumpDir * Mathf.Pow(-t + 1f, 3f);
            //dir.y = velocity.y;
            velocity = dir;

            speedBumpT = Mathf.Max(0, speedBumpT - Time.deltaTime * 5f);
        }

        else if (CanMove)
        {
            //float rollSpeed = 1f + Mathf.Clamp01(1f-Mathf.Pow((1f-rollSpeedT), 3f));
            // x * (1 - x) * 4
            float rollSpeed = 1f + Mathf.Clamp01(rollSpeedT * (1f - rollSpeedT) * 4f);
            // x*(1-x)*(1-x)/0.15
            //float rollSpeed = 1f + Mathf.Clamp01(rollSpeedT * (1f-rollSpeedT)*(1f-rollSpeedT) / 0.15f );


            var dir = Direction.normalized;
            if (isRolling)
            {
                //dir = rollDirection;
                /*float a = Vector3.Dot(dir, rollDirection);*/
                float a = 1f - rollSpeedT;
                a = a * a * a * a;
                dir = Vector3.Slerp(rollDirection, dir, a);

                /*if (dir.sqrMagnitude >= .9f)
                    rollDirection = dir;*/
                //dir = rollDirection;
            }

            // escalar direção c a velocidade
            var dirNorm = dir * MoveSpeed * (data.BrainType == ECharacterBrainType.AI ? 0.85f : 1f);
            forward = Vector3.Slerp(forward, dirNorm, 0.5f * Time.deltaTime * 30f).normalized;

            //dirNorm.y = velocity.y;

            // aplicar roll speed
            velocity = dirNorm * rollSpeed;
            if (Direction.sqrMagnitude > 0f)
            {
                dirNorm.y = 0f;
                transform.LookAt(transform.position + forward);
            }
        }

        else // fix 
        {
            //velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 2f);
        }

        if ((brainType == ECharacterBrainType.Input || speedBumpT > 0f))
        {
            NavMeshAgent.Move(velocity * Time.deltaTime);
        }

        rollSpeedT = Mathf.Clamp01(rollSpeedT - Time.deltaTime);
        if (Mathf.Approximately(rollSpeedT, 0f))
        {
            OnRollEnded?.Invoke();
        }

        NavMeshAgent.isStopped = IsAgentStopped;
    }

    public void ApplySpeedBump(Vector3 direction, float force)
    {
        //transform.LookAt(transform.position + direction);
        speedBumpT = 1f;
        _speedBumpDir = direction.normalized * force;
    }

    /*private void LateUpdate()
    {
        if (brainType == ECharacterBrainType.AI)
        {
            NavMeshAgent.isStopped |= speedBumpT > 0f || health.IsOnGround;
        }
    }*/

    private void OnDamagedCallback(CharacterAttackData attack)
    {
        var lookAt = attack.Attacker.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);

        if (/*attack.CancelAnimation || attack.Knockdown*/ true)
        {
            ApplySpeedBump(attack.Attacker.transform.forward, GetSpeedBumpForce(attack));
        }
    }

    public float GetSpeedBumpForce(CharacterAttackData attack)
    {
        if (IgnoreSpeedBump) return 0f;

        float modifier = (attack.Type == EAttackType.Weak ? 1f : 2f);
        modifier = attack.Knockdown ? 1f : modifier;

        return Mathf.Min(7f, ((float)attack.Damage / 25) * modifier);
    }
    
    private void OnCharacterAttackCallback(CharacterAttackData attack)
    {
        //ApplySpeedBump(transform.forward, SpeedBumpForce, attack.Type);
    }

    private void OnCharacterRequestAttackCallback(EAttackType obj)
    {
        forward = Direction.normalized;
    }

    public void Roll(Vector3 direction)
    {
        if (Time.time < lastRoll + 0.75f ||
            health.IsOnGround)
        {
            return;
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }


        OnRoll?.Invoke();
        rollSpeedT = 1f;
        speedBumpT = 0f;
        rollDirection = direction;
        forward = rollDirection;
        lastRoll = Time.time;
        transform.LookAt(transform.position + direction);
    }

    public void BeginRoll()
    {
        isRolling = true;
    }

    public void EndRoll()
    {
        isRolling = false;
        //rollSpeedT = 0f;
    }

#if UNITY_EDITOR

    private bool showDebug = false;

    private void OnGUI()
    {
        

        if (!showDebug) return;

        /*if (data.BrainType != ECharacterBrainType.Input)
            return;*/

        Rect r = UIManager.WorldSpaceGUI(transform.position, Vector2.one * 200f);
        GUI.Label(r, "IsOnCombo: " + combat.IsOnCombo +
                     "\nspeedBumpT: " + speedBumpT +
                     "\nrollSpeedT: " + rollSpeedT +
                     "\nrollDir: " + rollDirection +
                     "\nmoveDir: " + Direction +
                     "\nvelocity: " + CalculateVelocity(Direction.normalized) +
                     "\ncanMove: " + CanMove);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _speedBumpDir);
    }
#endif
}
