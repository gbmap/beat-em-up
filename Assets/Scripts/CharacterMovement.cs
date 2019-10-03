using System;
using System.Collections;
using System.Collections.Generic;
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

    private Vector3 _acceleration;
    private Vector3 _speed;
    private Rigidbody _rigidbody;

    private float _speedBumpT;
    private Vector3 _speedBumpDir;

    private void Awake()
    {
        _combat = GetComponent<CharacterCombat>();
        _health = GetComponent<CharacterHealth>();
        _health.OnDamaged += OnDamagedCallback;
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
        if (!_combat.IsOnCombo)
        {
            var dirNorm = direction.normalized;
            _rigidbody.velocity = dirNorm * moveSpeed;

            if (direction.sqrMagnitude > 0.025)
            {
                transform.LookAt(transform.position + dirNorm);
            }
        }

        if (_speedBumpT > 0f)
        {
            // applies dash on attack
            float t = 1f - _speedBumpT;
            _rigidbody.velocity = 4f * _speedBumpDir * Mathf.Pow(-t + 1f, 3f);

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

}
