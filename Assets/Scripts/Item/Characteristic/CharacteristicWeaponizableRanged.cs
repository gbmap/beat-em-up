using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName = "Data/Item Characteristic/Weaponizable/Ranged", fileName="WeaponizableRanged")]
    public class CharacteristicWeaponizableRanged : CharacteristicWeaponizableMelee
    {
        public GameObject Projectile;

        public override AttackResult[] Attack(CharacterData data, Transform origin, EAttackType attackType)
        {
            InstantiateProjectile(data, Projectile, data.transform.rotation);
            return null;
        }

        public void ProjectileAttack(CharacterData data, Transform origin, EAttackType attackType)
        {
            base.Attack(data, origin, attackType);
        }

        protected GameObject InstantiateProjectile(CharacterData data, GameObject Projectile, Quaternion rotation)
        {
            GameObject projectile = Instantiate(Projectile, 
                                                GetColliderPosition(data.transform), 
                                                rotation);

            projectile.layer = LayerMask.NameToLayer("Projectiles");

            ProjectileComponent projComponent = projectile.AddComponent<ProjectileComponent>();
            projComponent.Caster = data;
            projComponent.Weapon = this;
            return projectile;
        }
    }
}