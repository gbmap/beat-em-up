using UnityEngine;

[RequireComponent(typeof(CharacterHealth))]
public class SpawnObjectOnDeath : MonoBehaviour
{
    public CharacterHealth Health;
    public GameObject Object;

    [Range(0f, 1f)]
    public float Probability;

    private void OnEnable()
    {
        Health.OnDamaged += OnDamaged;    
    }

    private void OnDisable()
    {
        Health.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(CharacterAttackData obj)
    {
        if (!Health.IsDead) return;
        if (Random.value < Probability)
        {
            Instantiate(Object, transform.position, Quaternion.identity);
        }
    }
}
