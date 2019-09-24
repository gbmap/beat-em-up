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

    private void Awake()
    {
        _fx = FindObjectOfType<FX>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
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
        _rigidbody.isKinematic = false;
        _collider.enabled = true;
        _isOnFloor = false;
    }

    private void OnFallCallback()
    {
        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        _isOnFloor = true;
    }

    public void TakeDamage(CharacterAttackData data)
    {
        if (_isOnFloor)
        {
            return;
        }

        var lookAt = data.attacker.transform.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);

        OnDamaged?.Invoke(data);

        _fx.FxImpactHit(transform.position + Vector3.up);
    }

}
