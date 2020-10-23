using System;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Entity {
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
                (data.BrainType == ECharacterBrainType.AI ? Time.time > (combat.LastDamageData.Time + (combat.LastDamageData.Type == EAttackType.Strong ? 0.25f : 0.75f)) : true);
        }
    }
    public bool IsBeingMoved { get { return speedBumpT > 0f; } }

    public bool IgnoreSpeedBump = false;

    #region INTERFACE WITH NAVMESH

    public NavMeshAgent NavMeshAgent
    {
        get; private set;
    }

    public bool NavAgentValid
    {
        get { return NavMeshAgent && NavMeshAgent.enabled; }
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

    private Vector3 targetDestination;
    public Vector3 Destination
    {
        get { return NavMeshAgent.destination; }
    }

    public bool HasPath { get { return NavMeshAgent.hasPath; } }

    public bool SetDestination(Vector3 pos)
    {
        targetDestination = pos;
        if (NavMeshAgent.enabled)
        {
            return NavMeshAgent.SetDestination(pos);
        }
        return false;
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
        health.OnFall += OnFallCallback;
        health.OnGetUp += OnGetUpCallback;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (NavMeshAgent)
        {
            NavMeshAgent.speed = MoveSpeed * (data.BrainType == ECharacterBrainType.AI ? 0.75f : 1f);
        }
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

        health.OnDamaged -= OnDamagedCallback;
        health.OnFall -= OnFallCallback;
        health.OnGetUp -= OnGetUpCallback;
    }

    // Update is called once per frame
    void Update()
    {
        //NavMeshAgent.

        if (health.IsOnGround)
        {
            NavMeshAgent.enabled = speedBumpT > 0f;
        }
        //if (targetDestination != NavMeshAgent.destination && NavMeshAgent.enabled) SetDestination(targetDestination);
        
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
            if (Direction.sqrMagnitude > 0f && !combat.IsOnCombo)
            {
                dirNorm.y = 0f;
                
                transform.LookAt(transform.position + forward);
            }
        }

        if ( NavAgentValid && (brainType == ECharacterBrainType.Input || speedBumpT > 0f) )
        {
            NavMeshAgent.Move(velocity * Time.deltaTime);
        }

        rollSpeedT = Mathf.Clamp01(rollSpeedT - Time.deltaTime);
        if (Mathf.Approximately(rollSpeedT, 0f))
        {
            OnRollEnded?.Invoke();
        }

        if (NavAgentValid)
        {
            NavMeshAgent.isStopped = IsAgentStopped;
        }
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
        if (attack.CancelAnimation)
        {
            var lookAt = attack.Attacker.transform.position;
            lookAt.y = transform.position.y;
            transform.LookAt(lookAt);
        }

        if (attack.Knockdown && attack.AttackerStats != data.Stats)
        {
            ApplySpeedBump(attack.Attacker.transform.forward, GetSpeedBumpForce(attack));
        }
    }

    public float GetSpeedBumpForce(CharacterAttackData attack)
    {
        if (IgnoreSpeedBump) return 0f;
        return (attack.Type == EAttackType.Weak ? 3f : 6f);

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
            health.IsOnGround/* ||
            combat.IsOnHeavyAttack*/)
        {
            return;
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        combat.IsOnCombo = false;
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

    private void OnGetUpCallback()
    {
        NavMeshAgent.enabled = true;
    }

    private void OnFallCallback()
    {
        //NavMeshAgent.enabled = false;
    }

    public static Vector3 DistanceIndependentRotation2D(Vector3 targetPosition, Vector3 currentPosition, Vector3 currentForward)
    {
        float maxRotationAngle = 60f;

        // rotação independente de distância do alvo
        Vector3 dir3d = (targetPosition - currentPosition);
        dir3d.y = 0f;

        Vector3 fwd3d = currentForward;
        dir3d.y = 0f;

        Vector2 d = new Vector2(dir3d.x, dir3d.z).normalized;
        Vector2 f = new Vector2(fwd3d.x, fwd3d.z).normalized;

        float t = Vector2.SignedAngle(f, d);
        float a = Mathf.Min(Mathf.Abs(t), maxRotationAngle);
        a *= Mathf.Sign(t);
        a *= Time.deltaTime;

        // talvez tenha uma forma suportada de rotacionar um v2d
        Vector2 y = new Vector2();

        float sin = Mathf.Sin(a * Mathf.Deg2Rad);
        float cos = Mathf.Cos(a * Mathf.Deg2Rad);

        float tx = f.x;
        float ty = f.y;
        y.x = (cos * tx) - (sin * ty);
        y.y = (sin * tx) + (cos * ty);

        return new Vector3(y.x, currentForward.y, y.y);
    }

#if UNITY_EDITOR

    private bool showDebug = false;

    private void OnGUI()
    {
        

        if (!showDebug) return;

        /*if (data.BrainType != ECharacterBrainType.Input)
            return;*/

        Rect r = UIManager.WorldSpaceGUI(transform.position, Vector2.one * 200f);
        GUI.Label(r, "NavMeshAgent: " + NavAgentValid +
                     "\nIsOnCombo: " + combat.IsOnCombo +
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
}