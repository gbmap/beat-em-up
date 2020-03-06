using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FX : Singleton<FX>
{
    public ParticleSystem ParticleImpactHit;
    public ParticleSystem ParticleImpactHitSmall;

    public ParticleSystem ParticleImpactBlood;
    public GameObject HealEffect;

    public GameObject PrefabDamageLabel;

    private Canvas canvas;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void ImpactHit(Vector3 position, Vector3 direction, EAttackType attackType)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            velocity = Vector3.up
        };

        ParticleSystem ps = null;

        switch (attackType)
        {
            case EAttackType.Weak:
                ps = ParticleImpactHitSmall;
                break;
            case EAttackType.Strong:
                ps = ParticleImpactHit;
                break;
        }

        ps.Emit(emitParams, 1);
    }

    public void ImpactHit(CharacterAttackData data)
    {
        Vector3 pos = data.Defender.transform.position + Vector3.up * 1.1f + UnityEngine.Random.insideUnitSphere * 0.25f;
        Vector3 dir = (data.Attacker.transform.position - data.Defender.transform.position).normalized;

        ImpactHit(pos, dir, data.Type);
    }

    public void ImpactBlood(Vector3 position)
    {
        ParticleImpactBlood.transform.position = position;
        ParticleImpactBlood.Emit(100);
    }

    public void DamageLabel(Vector3 worldPosition, int damage)
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(worldPosition);

        // POOLING NISSO AQUI PELO AMOR DE DEUS PENSE NAS CRIANÇAS
        var label = Instantiate(PrefabDamageLabel, canvas.transform);
        label.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(worldPosition);
        label.GetComponent<Text>().text = damage.ToString();
    }

    public void EmitHealEffect(GameObject target)
    {
        var obj = Instantiate(HealEffect.gameObject, Vector3.zero, Quaternion.identity, target.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }

}
