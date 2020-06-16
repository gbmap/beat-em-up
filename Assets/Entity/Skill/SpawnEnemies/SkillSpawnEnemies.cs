using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSpawnEnemies : SkillData
{
    public GameObject[] Enemies;
    public int NumberOfEnemies;
    public float SpawnRange = 5f;

    public ParticleSystem SpawnEffect;
    public int ParticlesPerSpawn = 10;

    public List<GameObject> Minions { get; private set; }

    private void Awake()
    {
        Minions = new List<GameObject>();
    }

    void Spawn()
    {
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        int[] indexes = new int[NumberOfEnemies];
        Vector3[] poss = new Vector3[NumberOfEnemies];

        if (SpawnEffect)
        {
            for (int i = 0; i < NumberOfEnemies; i++)
            {
                indexes[i] = UnityEngine.Random.Range(0, Enemies.Length);

                Vector2 posA = UnityEngine.Random.insideUnitCircle * SpawnRange;
                poss[i] = new Vector3(posA.x, 0f, posA.y);

                GameObject enemy = Enemies[indexes[i]];
                Vector3 pos = poss[i];

                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
                {
                    position = transform.position + pos
                };
                SpawnEffect.Emit(emitParams, ParticlesPerSpawn);
            }
        }

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < NumberOfEnemies; i++)
        {
            GameObject enemy = Enemies[indexes[i]];
            Vector3 pos = transform.position + poss[i];

            var obj = Instantiate(enemy, pos, Quaternion.identity);
            obj.GetComponent<CharacterHealth>().OnDeath += OnMinionDeathCallback;
            Minions.Add(obj);
        }
    }

    private void OnMinionDeathCallback(CharacterHealth health)
    {
        //Minions.Remove(health.gameObject);
        health.OnDeath -= OnMinionDeathCallback;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, SpawnRange);
    }

    public override void Cast()
    {
        Spawn();
    }
}
