using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FX : Singleton<FX>
{
    public ParticleSystem ParticleImpactHit;
    public ParticleSystem ParticleImpactBlood;

    public GameObject PrefabDamageLabel;

    private Canvas canvas;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void ImpactHit(Vector3 position)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            rotation = Random.value * 360f
        };
        ParticleImpactHit.Emit(emitParams, 1);
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
}
