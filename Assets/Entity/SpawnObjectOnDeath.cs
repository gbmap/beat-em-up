using UnityEngine;

[RequireComponent(typeof(CharacterHealth))]
public class SpawnObjectOnDeath : MonoBehaviour
{
    public CharacterHealth Health;
    public GameObject[] ObjectPool;

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
            Instantiate(ObjectPool[Random.Range(0, ObjectPool.Length)], transform.position, Quaternion.identity);
        }
    }
}
