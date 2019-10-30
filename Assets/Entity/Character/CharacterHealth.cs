using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    public System.Action<CharacterAttackData> OnDamaged;
    public System.Action OnFall;
    public System.Action OnGetUp;

    private bool _isOnFloor;

    FX _fx;

    private Rigidbody _rigidbody;
    private Collider _collider;

    public MeshRenderer HealthQuad;

    private float lastHit;
    private CharacterData characterData;
    private CharacterMovement characterMovement;

    // hora q caiu no chão do Knockdown
    private float recoverTimer;
    private float recoverCooldown = 2f;
    public bool IsOnGround;

    private void Awake()
    {
        _fx = FindObjectOfType<FX>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        characterData = GetComponent<CharacterData>();
        characterMovement = GetComponent<CharacterMovement>();

        UpdateHealthQuad(1f, 1f);
    }

    private void Update()
    {
        if (Time.time > lastHit + 2f) // TODO: especificar o tempo pra reiniciar o poise
        {
            characterData.Stats.PoiseBar = 1f;
            UpdatePoise(1f);
        }

        // timer pra se recuperar
        if (IsOnGround && !characterMovement.IsOnAir && recoverTimer > 0f)
        {
            recoverTimer -= Time.deltaTime;
            if (recoverTimer < 0f)
            {
                IsOnGround = false;
                OnGetUp?.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        OnFall += OnFallCallback;
        OnGetUp += OnGetUpCallback;
    }

    private void OnDisable()
    {
        OnFall -= OnFallCallback;
        OnGetUp -= OnGetUpCallback;
    }

    private void OnGetUpCallback()
    {
        //_rigidbody.isKinematic = false;
        //_rigidbody.useGravity = true;
        _collider.enabled = true;
        _isOnFloor = false;
    }

    private void OnFallCallback()
    {
        //_rigidbody.isKinematic = true;
        //_rigidbody.useGravity = false;
        _collider.enabled = false;
        //_isOnFloor = true;
        IsOnGround = true;
        recoverTimer = recoverCooldown;
    }

    public void TakeDamage(CharacterAttackData data)
    {
        if (IsOnGround /* && characterMovement.IsOnAir */)
        {
            return;
        }

        lastHit = Time.time;

        var lookAt = data.Attacker.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);

        _fx.ImpactHit(transform.position + Vector3.up);
        _fx.DamageLabel(transform.position + Vector3.up, data.Damage);

        UpdateHealthQuad(((float)data.DefenderStats.Health) / data.DefenderStats.MaxHealth, data.DefenderStats.PoiseBar);

        if (data.DefenderStats.Health <= 0)
        {
            // TODO: dar um funeral digno pros personagens
            Destroy(gameObject);
        }
        else
        {
            OnDamaged?.Invoke(data);
        }
    }

    private void UpdateHealthQuad(float healthPercentage, float poiseBar)
    {
        UpdateHealth(healthPercentage);
        UpdatePoise(poiseBar);
    }

    private void UpdateHealth(float healthPercentage)
    {
        HealthQuad.material.SetFloat("_Health", healthPercentage);
    }

    private void UpdatePoise(float poiseBar)
    {
        HealthQuad.material.SetFloat("_Poise", poiseBar);
    }
}
