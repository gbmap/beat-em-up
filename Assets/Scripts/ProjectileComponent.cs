using Catacumba.Data.Items.Characteristics;
using UnityEngine;

namespace Catacumba.Entity
{
    public class ProjectileComponent : MonoBehaviour
    {
        public CharacterData Caster;
        public CharacteristicWeaponizableRanged Weapon;
 
        void OnTriggerEnter(Collider collider)
        {
            CharacterData defender = collider.GetComponent<CharacterData>();
            if (defender)
            {
                if (defender != Caster)
                    Weapon.ProjectileAttack(Caster, transform, EAttackType.Strong);
                else
                    return;
            } 

            Destroy(this.gameObject);
        }

        public void Reflect(CharacterData attacker)
        {
            Caster = attacker;
            Vector3 rotation = attacker.transform.forward;
            transform.forward = attacker.transform.forward;
        }
    }
}