using UnityEngine;

public class HealingConsumable : MonoBehaviour
{
    public int HealValue;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<CharacterData>().Stats.Health += HealValue;
        Destroy(gameObject);
    }
}
