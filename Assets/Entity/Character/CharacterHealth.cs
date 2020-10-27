using System;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using UnityEngine;
using Catacumba.Data;
using Catacumba.Effects;

namespace Catacumba.Entity
{
public class MsgOnPlayerDied { public CharacterData player; }

public class CharacterHealth : CharacterComponentBase
{
    public bool CanBeKnockedOut = true;
    public bool IgnoreDamage = false;

    public ParticleEffectConfiguration HitEffect;

    public System.Action<CharacterAttackData> OnDamaged;
    public System.Action OnFall;
    public System.Action OnRecover;
    public System.Action OnGetUp;
    public System.Action<CharacterHealth> OnDeath;

    private new Collider collider;

    public MeshRenderer HealthQuad;

    private float lastHit;
    private CharacterAnimator animator;

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
        get { return data.Stats.Health; }
    }

    public bool IsDead
    {
        get { return Health <= 0; }
    }

    public float HealthNormalized
    {
        get { return data.Stats.HealthNormalized; }
    }

    // hora q caiu no chão do Knockdown
    private float recoverTimer;
    private float recoverCooldown = 2f;

    public bool IsOnGround { get; private set; }
    public bool IsBeingDamaged; // rolando animação de dano

    public override void OnConfigurationEnded()
    {
        base.OnConfigurationEnded();
        SetupDamageEffect();
    }

    protected override void OnComponentAdded(CharacterComponentBase component)
    {
        base.OnComponentAdded(component);

        if (component is CharacterAnimator)
        {
            animator = component as CharacterAnimator;
            animator.OnRefreshAnimator += RefreshMaterials; 
        }
    }

    protected override void OnComponentRemoved(CharacterComponentBase component)
    {
        base.OnComponentRemoved(component);

        if (component is CharacterAnimator)
        {
            animator.OnRefreshAnimator -= RefreshMaterials; 
            animator = null;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        collider = GetComponent<Collider>();
        RefreshMaterials(animator?.animator);

        UpdateHealthQuad(1f, 1f);

    }

    private void SetupDamageEffect()
    {
        if (!HitEffect)
            HitEffect = data.CharacterCfg.View.DamageEffect;

        HitEffect?.Setup(this);
    }

    private void Update()
    {
        if (Time.time > lastHit + 2f) // TODO: especificar o tempo pra reiniciar o poise
        {
            data.Stats.CurrentStamina = data.Stats.Stamina;
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
                    if (data.BrainType == ECharacterBrainType.Input)
                    {
                        ServiceFactory.Instance.Resolve<MessageRouter>().RaiseMessage(new MsgOnPlayerDied { player = data });
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

    void UpdateHitFactor()
    {
        if (!Mathf.Approximately(HitEffectFactor, 0f))
        {
            HitEffectFactor = Mathf.Max(0f, HitEffectFactor - Time.deltaTime * 2f);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnFall += OnFallCallback;
        OnGetUp += OnGetUpAnimationEnd;

        data.Stats.OnStatsChanged += OnStatsChangedCallback;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        OnFall -= OnFallCallback;
        OnGetUp -= OnGetUpAnimationEnd;

        data.Stats.OnStatsChanged -= OnStatsChangedCallback;
    }

    private void OnStatsChangedCallback(CharacterStats stats)
    {
        UpdateHealthQuad(stats.HealthNormalized, stats.StaminaBar);
    }

    public void OnGetUpAnimationEnd()
    {
        IsOnGround = false;
        collider.enabled = true;
    }

    private void OnFallCallback()
    {
        collider.enabled = false;
        IsOnGround = true;
        recoverTimer = recoverCooldown;
        if (IsDead)
        {
            recoverTimer *= 2f;
        }
    }

    public void TakeDamage(CharacterAttackData data)
    {
        if (IsOnGround /* && characterMovement.IsOnAir */)
        {
            return;
        }

        if (data.Attacker == null)
        {
            data.Attacker = gameObject;
            data.AttackerStats = this.data.Stats;
        }

        if (data.Defender == null)
        {
            data.Defender = gameObject;
            data.DefenderStats = this.data.Stats;
        }

        lastHit = Time.time;

        UpdateHealthQuad(data.DefenderStats.HealthNormalized, data.DefenderStats.StaminaBar);

        if (data.Knockdown && data.CancelAnimation ||
            data.DefenderStats.Health <= 0)
        {
            //characterData.UnEquip(EInventorySlot.Weapon, data.Attacker.transform.forward);
        }

        if (HitEffect)
            HitEffect.EmitBurst(this, 20);

        OnDamaged?.Invoke(data);

        if (IsDead)
        {
            collider.enabled = false;
            OnDeath?.Invoke(this);

            if (!animator)
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

}