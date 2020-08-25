using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagingProp : MonoBehaviour
{
    public Collider Collider;
    public bool DestroyOnTouch = true;

    private void OnTriggerEnter(Collider other)
    {
        var caster = GetComponent<SkillData>().Caster;
        if (other.gameObject == null || 
            other.gameObject == caster.gameObject ||
            other.gameObject.CompareTag(caster.tag)) return;

        if (other.gameObject.layer == caster.gameObject.layer) return;

        var movement = other.GetComponent<CharacterMovement>();
        if (movement != null && movement.IsRolling) return;


        CharacterAttackData ad = new CharacterAttackData(EAttackType.Weak, gameObject)
        {
            Damage = 10,
            Attacker = caster.gameObject
        };
        CombatManager.Attack(ref ad, transform.position, Vector3.one, transform.rotation);

        var p = GetComponentsInChildren<UnparentParticleSystemOnDeath>();
        System.Array.ForEach(p, d => d.Detach());

        if (DestroyOnTouch)
            Destroy(gameObject);
    }
}
