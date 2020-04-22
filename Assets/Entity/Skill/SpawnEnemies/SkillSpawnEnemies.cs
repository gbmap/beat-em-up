using System.Collections;
using UnityEngine;

public class SkillSpawnEnemies : MonoBehaviour
{
    public GameObject[] Enemies;
    public int NumberOfEnemies;
    public float SpawnRange = 5f;

    public ParticleSystem SpawnEffect;
    public int ParticlesPerSpawn = 10;
    
    IEnumerator Start()
    {
        int[] indexes = new int[NumberOfEnemies];
        Vector3[] poss = new Vector3[NumberOfEnemies];

        if (SpawnEffect)
        {
            for (int i = 0; i < NumberOfEnemies; i++)
            {
                indexes[i] = Random.Range(0, Enemies.Length);

                Vector2 posA = Random.insideUnitCircle * SpawnRange;
                poss[i] = new Vector3(posA.x, 0f, posA.y);

                GameObject enemy = Enemies[indexes[i]];
                Vector3 pos = poss[i];

                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
                {
                    position = pos
                };
                SpawnEffect.Emit(emitParams, ParticlesPerSpawn);
            }
        }

        yield return new WaitForSeconds(0.75f);

        for (int i = 0; i < NumberOfEnemies; i++)
        {
            GameObject enemy = Enemies[indexes[i]];
            Vector3 pos = poss[i];

            Instantiate(enemy, pos, Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, SpawnRange);
    }
}
