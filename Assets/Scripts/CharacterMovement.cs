using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // === REFS
    CharacterHealth _health;
    CharacterCombat _combat;

    // ==== MOVEMENT
    public Vector3 direction;
    public Vector3 velocity { get { return _rigidbody.velocity; } }
    public float moveSpeed = 3.0f;

    public float jumpForce = 1f;

    private Vector3 _acceleration;
    private Vector3 _speed;
    private Rigidbody _rigidbody;

    private float _speedBumpT;
    private Vector3 _speedBumpDir;

    private CapsuleCollider capsuleCollider;
    public CapsuleCollider Collider
    {
        get { return capsuleCollider; }
    }

    [SerializeField]
    private float raycastDistance = 0.2f;

    public bool IsJumping
    {
        get
        {
            // cuidado!!!! chances de hemorragia ocular!!!!11111111 
            Ray r = new Ray
            {
                origin = transform.position + Vector3.up * 0.05f, // remove as chances do raycast começar dentro do collider do chão
                direction = Vector3.down
            };
            return !Physics.Raycast(r, raycastDistance, 1 << LayerMask.NameToLayer("Level"), QueryTriggerInteraction.Ignore);
        }
    }

    private void Awake()
    {
        _combat = GetComponent<CharacterCombat>();
        _health = GetComponent<CharacterHealth>();
        _health.OnDamaged += OnDamagedCallback;

        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
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
        if (!_combat.IsOnCombo && !IsJumping)
        {
            var dirNorm = direction.normalized * moveSpeed;
            dirNorm.y = _rigidbody.velocity.y;
            _rigidbody.velocity = dirNorm;

            if (direction.sqrMagnitude > 0.025)
            {
                dirNorm.y = 0f;
                transform.LookAt(transform.position + dirNorm);
            }
        }

        if (_speedBumpT > 0f)
        {
            // applies dash on attack
            float t = 1f - _speedBumpT;
            var dir =  4f * _speedBumpDir * Mathf.Pow(-t + 1f, 3f);
            //dir.y = _rigidbody.velocity.y;
            _rigidbody.velocity = dir;

            _speedBumpT = Mathf.Max(0, _speedBumpT - Time.deltaTime * 2f);
        }
    }

    private void OnDamagedCallback(CharacterAttackData attack)
    {
        //_speedBumpDir = -transform.forward;
        _speedBumpDir = attack.attacker.transform.forward * (1f + 0.15f*attack.hitNumber);
        _speedBumpT = 1f;
    }

    private void OnCharacterAttackCallback(CharacterAttackData obj)
    {
        _speedBumpT = 1f;
        _speedBumpDir = transform.forward;
    }

    public void Jump()
    {
        if (IsJumping) return;

        //_rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _rigidbody.velocity = _rigidbody.velocity + Vector3.up * jumpForce;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = IsJumping ? Color.red : Color.green;
        var origin = transform.position;
        Gizmos.DrawLine(origin, origin + Vector3.down * raycastDistance);
    }

}
