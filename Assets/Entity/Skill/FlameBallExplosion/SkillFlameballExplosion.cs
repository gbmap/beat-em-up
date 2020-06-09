using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFlameballExplosion : SkillData
{
    public ParticleSystem Flames;
    public GameObject SkillFlameball;
    public int NumberOfSpawns = 6;

    public IEnumerator EmitFlameballs()
    {
        if (!SkillFlameball) yield break;

        for (int i = 0; i < NumberOfSpawns; i++)
        {
            float degree = ((float)i / NumberOfSpawns) * 360f;
            float rad = Mathf.Deg2Rad * degree;
            Vector3 pos = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
            Quaternion rot = Quaternion.LookRotation(pos);

            var instance = Instantiate(SkillFlameball, transform.position + pos * 2f, rot);
            instance.GetComponent<SkillData>().Caster = Caster;
        }

        yield break;
    }

    public override void Cast()
    {
        Flames.Emit(300);
        StartCoroutine(EmitFlameballs());
    }
}
