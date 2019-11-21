using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    // === REFS
    CharacterHealth _health;
    CharacterCombat _combat;
    CharacterData data;

    // ==== MOVEMENT
    [HideInInspector]
    public Vector3 Direction;
    public Vector3 Velocity { get { return brainType == ECharacterBrainType.AI ? navMeshAgent.velocity : Direction; } }
    private Vector3 velocity;

    public float MoveSpeed = 3f;

    private Rigidbody _rigidbody;

    private float _speedBumpT;
    private Vector3 _speedBumpDir;

    public System.Action OnJump;
    public System.Action OnRoll;


    private bool isRolling;
    private float rollSpeedT;
    private Vector3 rollDirection;

    NavMeshAgent navMeshAgent;

    private ECharacterBrainType brainType
    {
        get
        {
            return data.BrainType;
        }
    }

    public bool IsOnAir
    {
        get
        {
            return false;
        }
    }

    private void Awake()
    {
        data = GetComponent<CharacterData>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        _combat = GetComponent<CharacterCombat>();
        _health = GetComponent<CharacterHealth>();

        _health.OnDamaged += OnDamagedCallback;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        navMeshAgent.speed = MoveSpeed * (data.BrainType == ECharacterBrainType.AI ? 0.75f : 1f);
    }

    private void OnEnable()
    {
        _combat.OnCharacterAttack += OnCharacterAttackCallback;
    }

    private void OnDisable()
    {
        _combat.OnCharacterAttack -= OnCharacterAttackCallback;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_combat.IsOnCombo && !IsOnAir && !_health.IsOnGround)
        {
            float rollSpeed = 1f + Mathf.Clamp01(Mathf.Pow(rollSpeedT, 1f/3));

            var dir = Direction.normalized;
            if (isRolling)
            {
                float a = Vector3.Dot(dir, rollDirection);
                dir = Vector3.Lerp(rollDirection, dir, Mathf.Max(0f, a));
                rollDirection = dir;
            }

            var dirNorm = dir * MoveSpeed * (data.BrainType == ECharacterBrainType.AI ? 0.85f : 1f);
            dirNorm.y = velocity.y;




            velocity = dirNorm * rollSpeed;
            if (Direction.sqrMagnitude > 0.025)
            {
                dirNorm.y = 0f;
                transform.LookAt(transform.position + dirNorm);
            }
        }

        if (_speedBumpT > 0f)
        {
            // applies dash on attack
            float t = 1f - _speedBumpT;
            var dir = 4f * _speedBumpDir * Mathf.Pow(-t + 1f, 3f);
            //dir.y = velocity.y;
            velocity = dir;

            _speedBumpT = Mathf.Max(0, _speedBumpT - Time.deltaTime * 2f);
        }

        if (brainType == ECharacterBrainType.Input || _speedBumpT > 0f)
        {
            navMeshAgent.Move(velocity * Time.deltaTime);
        }

        rollSpeedT = Mathf.Clamp01(rollSpeedT - Time.deltaTime*0.8f);
    }

    private void LateUpdate()
    {
        if (brainType == ECharacterBrainType.AI)
        {
            navMeshAgent.isStopped |= _speedBumpT > 0f;
        }
    }

    private void OnDamagedCallback(CharacterAttackData attack)
    {
        {
            _speedBumpDir = attack.Attacker.transform.forward * (1f + 0.15f * attack.HitNumber);
            _speedBumpT = 1f;
        }
    }

    private void OnCharacterAttackCallback(CharacterAttackData attack)
    {
        _speedBumpT = 1f;
        _speedBumpDir = transform.forward;
    }

    public void Roll()
    {
        OnRoll?.Invoke();
        rollSpeedT = 1f;
        rollDirection = Direction;
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
}
