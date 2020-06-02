using System;
using System.Collections;
using System.Collections.Generic;
using Catacumba.Exploration;
using Frictionless;
using UnityEngine;

public class MsgOnPlayerDied { public CharacterData player; }

public class CharacterHealth : MonoBehaviour
{
    public bool CanBeKnockedOut = true;
    public bool IgnoreDamage = false;

    public System.Action<CharacterAttackData> OnDamaged;
    public System.Action OnFall;
    public System.Action OnRecover;
    public System.Action OnGetUp;
    public System.Action<CharacterHealth> OnDeath;

    private Rigidbody _rigidbody;
    private Collider collider;

    public MeshRenderer HealthQuad;

    private float lastHit;
    private CharacterData characterData;
    private CharacterAnimator characterAnimator;

    /// ============ HEALTH
    private Material[] Materials
    {
        get; set;
    }

    private float hitEffectFactor;
    private float HitEffectFactor
    {
        get { return hitEffectFactor; }
        set
        {
            hitEffectFactor = value;
            if (Materials == null) return;
            for (int i = 0; i < Materials.Length; i++)
            {
                Material m = Materials[i];
                if (m == null) continue;
                m.SetFloat("_HitFactor", value);
            }
        }
    }

    public int Health
    {
        get { return characterData.Stats.Health; }
    }

    public bool IsDead
    {
        get { return Health <= 0; }
    }

    public float HealthNormalized
    {
        get { return characterData.Stats.HealthNormalized; }
    }

    // hora q caiu no chão do Knockdown
    private float recoverTimer;
    private float recoverCooldown = 2f;

    public bool IsOnGround { get; private set; }
    public bool IsBeingDamaged; // rolando animação de dano

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        characterData = GetComponent<CharacterData>();
        characterAnimator = GetComponent<CharacterAnimator>();
        RefreshMaterials(characterAnimator?.animator);

        UpdateHealthQuad(1f, 1f);
    }

    private void Update()
    {
        if (Time.time > lastHit + 2f) // TODO: especificar o tempo pra reiniciar o poise
        {
            characterData.Stats.CurrentPoise = characterData.Stats.Poise;
            UpdatePoise(1f);
        }

        // timer pra se recuperar
        if (IsOnGround && recoverTimer > 0f)
        {
            recoverTimer -= Time.deltaTime;
            if (recoverTimer < 0f)
            {
                if (IsDead)
                {
                    OnDeath?.Invoke(this);
                    if (characterData.BrainType == ECharacterBrainType.Input)
                    {
                        ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new MsgOnPlayerDied { player = characterData });
                    }

                    Destroy(gameObject);
                }
                else
                {
                    OnRecover?.Invoke();
                }
            }
        }

        UpdateHitFactor();
    }

    private void UpdateHitFactor()
    {
        if (!Mathf.Approximately(HitEffectFactor, 0f))
        {
            HitEffectFactor = Mathf.Max(0f, HitEffectFactor - Time.deltaTime * 2f);
        }
    }

    private void OnEnable()
    {
        OnFall += OnFallCallback;
        OnGetUp += OnGetUpAnimationEnd;
        characterData.Stats.OnStatsChanged += OnStatsChangedCallback;
        
        if (characterAnimator)
        {
            characterAnimator.OnRefreshAnimator += RefreshMaterials;
        }
    }

    private void OnDisable()
    {
        OnFall -= OnFallCallback;
        OnGetUp -= OnGetUpAnimationEnd;
        characterData.Stats.OnStatsChanged -= OnStatsChangedCallback;

        if (characterAnimator)
        {
            characterAnimator.OnRefreshAnimator -= RefreshMaterials;
        }
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
        collider.enabled = true;
    }

    private void OnFallCallback()
    {
        //_rigidbody.isKinematic = true;
        //_rigidbody.useGravity = false;
        collider.enabled = false;
        IsOnGround = true;
        recoverTimer = recoverCooldown;
        if (IsDead)
        {
            recoverTimer *= 2f;
        }
        
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

        UpdateHealthQuad(data.DefenderStats.HealthNormalized, data.DefenderStats.PoiseBar);

        if (data.Knockdown && data.CancelAnimation ||
            data.DefenderStats.Health <= 0)
        {
            characterData.UnEquip(EInventorySlot.Weapon, data.Attacker.transform.forward);
        }

        OnDamaged?.Invoke(data);

        if (IsDead)
        {
            collider.enabled = false;

            if (!characterAnimator)
            {
                Destroy(gameObject);
            }
        }

        HitEffectFactor = 1f;
    }

    private void UpdateHealthQuad(float healthPercentage, float poiseBar)
    {
        if (!HealthQuad) return;

        UpdateHealth(healthPercentage);
        UpdatePoise(poiseBar);
    }

    private void UpdateHealth(float healthPercentage)
    {
        if (!HealthQuad) return;

        HealthQuad.material.SetFloat("_Health", healthPercentage);
    }

    private void UpdatePoise(float poiseBar)
    {
        if (!HealthQuad) return;

        HealthQuad.material.SetFloat("_Poise", poiseBar);
    }

    void RefreshMaterials(Animator animator)
    {
        List<Material> materials = new List<Material>();
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            materials.Add(r.material);
        }

        Materials = materials.ToArray();
    }

    public void SetIgnoreDamage(bool v)
    {
        IgnoreDamage = v;
    }
}
