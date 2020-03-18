using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagingProp : MonoBehaviour
{
    public Collider Collider;

    private void OnTriggerEnter(Collider other)
    {
        var caster = GetComponent<SkillData>()?.Caster;

        CharacterAttackData ad = new CharacterAttackData(EAttackType.Weak, gameObject)
        {
            Damage = 10,
            Attacker = caster.gameObject
        };
        CombatManager.Attack(ref ad, transform.position, Vector3.one, transform.rotation);
        Destroy(gameObject);
    }
}
