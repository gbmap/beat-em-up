using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Items;
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
            if (!defender || defender == Caster) return;

            Weapon.ProjectileAttack(Caster, transform, EAttackType.Strong);
            Destroy(this.gameObject);
        }
    }
}