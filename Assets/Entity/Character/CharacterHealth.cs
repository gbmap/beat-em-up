using System;
using System.Collections;
using System.Collections.Generic;
using Catacumba.Exploration;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    public System.Action<CharacterAttackData> OnDamaged;
    public System.Action OnFall;
    public System.Action OnRecover;
    public System.Action OnGetUp;
    public System.Action OnDeath;

    private bool _isOnFloor;

    private Rigidbody _rigidbody;
    private Collider _collider;

    public MeshRenderer HealthQuad;

    private float lastHit;
    private CharacterData characterData;

    public int Health
    {
        get { return characterData.Stats.Health; }
    }

    public float HealthNormalized
    {
        get { return characterData.Stats.HealthNormalized; }
    }

    // hora q caiu no chão do Knockdown
    private float recoverTimer;
    private float recoverCooldown = 2f;

    public bool IsOnGround;
    public bool IsBeingDamaged; // rolando animação de dano

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        characterData = GetComponent<CharacterData>();

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
        if (IsOnGround && recoverTimer > 0f)
        {
            recoverTimer -= Time.deltaTime;
            if (recoverTimer < 0f)
            {
                OnRecover?.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        OnFall += OnFallCallback;
        OnGetUp += OnGetUpAnimationEnd;
        characterData.Stats.OnStatsChanged += OnStatsChangedCallback;
    }

    private void OnDisable()
    {
        OnFall -= OnFallCallback;
        OnGetUp -= OnGetUpAnimationEnd;
        characterData.Stats.OnStatsChanged -= OnStatsChangedCallback;
    }

    private void OnStatsChangedCallback(CharacterStats stats)
    {
        UpdateHealthQuad(stats.HealthNormalized, stats.PoiseBar);
    }

    public void OnGetUpAnimationEnd()
    {
        //_rigidbody.isKinematic = false;
        //_rigidbody.useGravity = true;
        IsOnGround = false;
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
        
        // Shake camera
        CameraManager.Instance.Shake();
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

        var fx = FX.Instance;
        fx.ImpactHit(data);

        //fx.ImpactBlood(transform.position + Vector3.up);
        fx.DamageLabel(transform.position + Vector3.up, data.Damage);

        UpdateHealthQuad(data.DefenderStats.HealthNormalized, data.DefenderStats.PoiseBar);

        if (data.Knockdown && data.CancelAnimation)
        {
            characterData.UnEquip(EInventorySlot.Weapon, data.Attacker.transform.forward);
        }

        if (data.DefenderStats.Health <= 0)
        {
            // TODO: dar um funeral digno pros personagens
            OnDeath?.Invoke();


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
