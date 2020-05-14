using UnityEngine;

public class SpawnObjectOnDeath : MonoBehaviour
{
    public CharacterHealth Health;
    public GameObject[] ObjectPool;

    [Range(0f, 1f)]
    public float Probability;

    [Range(1, 10)]
    public int Instances = 1;

    [Range(0, 10)]
    public int RangeOfInstances = 0;

    private void OnEnable()
    {
        if (!Health)
        {
            Health = GetComponent<CharacterHealth>();
            if (!Health)
            {
                Debug.LogError("No Character Health found!");
                Destroy(this);
            }
        }

        Health.OnDamaged += OnDamaged;    
    }

    private void OnDisable()
    {
        if (!Health) return;
        Health.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(CharacterAttackData obj)
    {
        if (!Health.IsDead) return;

        int count = Instances - Random.Range(0, RangeOfInstances);
        for (int i = 0; i < Instances; i++)
        {
            if (Random.value < Probability)
            {
                Instantiate(ObjectPool[Random.Range(0, ObjectPool.Length)], transform.position, Quaternion.identity);
            }
        }
    }
}
