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

    private void Awake()
    {
        _fx = FindObjectOfType<FX>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        UpdateHealthQuad(1f);
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
        _rigidbody.useGravity = true;
        _collider.enabled = true;
        _isOnFloor = false;
    }

    private void OnFallCallback()
    {
        //_rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _collider.enabled = false;
        _isOnFloor = true;
    }

    public void TakeDamage(CharacterAttackData data)
    {
        if (_isOnFloor)
        {
            return;
        }

        var lookAt = data.Attacker.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);

        _fx.ImpactHit(transform.position + Vector3.up);
        _fx.DamageLabel(transform.position + Vector3.up, data.Damage);

        UpdateHealthQuad(((float)data.DefenderStats.Health) / data.DefenderStats.MaxHealth);

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

    private void UpdateHealthQuad(float healthPercentage)
    {
        HealthQuad.material.SetFloat("_Health", healthPercentage);
    }
}
