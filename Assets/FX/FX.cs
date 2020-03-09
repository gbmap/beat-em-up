using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FX : Singleton<FX>
{
    public GameObject HealEffect;

    public GameObject PrefabDamageLabel;

    private Canvas canvas;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
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
