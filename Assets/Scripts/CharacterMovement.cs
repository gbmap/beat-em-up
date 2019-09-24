using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAttackType
{
    Weak,
    Strong
}

public struct CharacterAttackData
{
    public EAttackType type;
    public GameObject attacker;
    public int hitNumber;
    public Collider[] hits;
}

public class CharacterMovement : MonoBehaviour
{
    // === REFS
    CharacterHealth _health;

    // ==== MOVEMENT
    public Vector3 direction;
    public Vector3 velocity { get { return _rigidbody.velocity; } }
    public float moveSpeed = 3.0f;

    private Vector3 _acceleration;
    private Vector3 _speed;
    private Rigidbody _rigidbody;

    private float _speedBumpT;
    private Vector3 _speedBumpDir;

    // ===== COMBAT
    [HideInInspector] public bool IsOnCombo;
    
    private int _nComboHits;

    public System.Action<EAttackType> OnRequestCharacterAttack;
    public System.Action<CharacterAttackData> OnCharacterAttack;
    public System.Action OnComboStarted;
    public System.Action OnComboEnded;

    private void Awake()
    {
        _health = GetComponent<CharacterHealth>();
        _health.OnDamaged += OnDamagedCallback;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        OnComboStarted += OnComboStartedCallback;
        OnComboEnded += OnComboEndedCallback;
    }

    private void OnDisable()
    {
        OnComboStarted -= OnComboStartedCallback;
        OnComboEnded -= OnComboEndedCallback;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOnCombo)
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

    public void RequestAttack(EAttackType type)
    {
        OnRequestCharacterAttack?.Invoke(type);
    }

    /*
     *  CHAMADO PELO ANIMATOR!!!1111
     */
    public void Attack(EAttackType type)
    {
        Attack(new CharacterAttackData { type = type, attacker = gameObject, hitNumber = ++_nComboHits });
    }

    private void Attack(CharacterAttackData attack)
    {
        // bump speed factor on attack after events because it gets zeroed on combo start
        _speedBumpT = 1f;
        _speedBumpDir = transform.forward;

        Collider[] colliders = Physics.OverlapBox(transform.position + transform.forward, Vector3.one);
        attack.hits = colliders;
        foreach (var c in colliders)
        {
            if (c.gameObject == gameObject) continue;
            c.gameObject.GetComponent<CharacterHealth>()?.TakeDamage(attack);
        }

        OnCharacterAttack?.Invoke(attack);
    }
    
    /*
     * CALLBACKS
     */ 
    private void OnComboStartedCallback()
    {
        Debug.Log("Combo started");
        IsOnCombo = true;
    }

    private void OnComboEndedCallback()
    {
        Debug.Log("Combo ended");
        IsOnCombo = false;
        _nComboHits = 0;
    }

    private void OnDamagedCallback(CharacterAttackData attack)
    {
        //_speedBumpDir = -transform.forward;
        _speedBumpDir = attack.attacker.transform.forward * (1f + 0.15f*attack.hitNumber);
        _speedBumpT = 1f;
    }

}
