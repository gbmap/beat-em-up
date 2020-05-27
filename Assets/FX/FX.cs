using UnityEngine;
using UnityEngine.UI;

public class FX : SimpleSingleton<FX>
{
    [Tooltip("Used in heal targets")]
    public GameObject HealEffect;

    [Tooltip("Used on the healer's hands")]
    public GameObject HealFlame;

    public GameObject PrefabDamageLabel;

    private Canvas canvas;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    private void SpawnEffect(GameObject ps, GameObject target)
    {
        var obj = Instantiate(ps, Vector3.zero, Quaternion.identity, target.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
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
        SpawnEffect(HealEffect, target);
    }

    public void EmitHealFlame(GameObject target)
    {
        SpawnEffect(HealFlame, target);
    }

}
