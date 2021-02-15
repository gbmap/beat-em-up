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

        public void ProjectileAttack(CharacterData data, CharacterData defender, Transform origin, EAttackType attackType)
        {
            //base.Attack(data, origin, attackType);
            AttackResult[] results = new AttackResult[1];
            int hits = 0;
            base.AttackCharacter(data, defender, attackType, ref results, ref hits);
        }

        protected GameObject InstantiateProjectile(CharacterData data, GameObject Projectile, Quaternion rotation)
        {
            GameObject projectile = Instantiate(Projectile, 
                                                GetProjectilePosition(data.transform), 
                                                rotation);

            projectile.layer = LayerMask.NameToLayer("Projectiles");

            ProjectileComponent projComponent = projectile.AddComponent<ProjectileComponent>();
            projComponent.Caster = data;
            projComponent.Weapon = this;
            return projectile;
        }

        public override void DebugDraw(CharacterData data, EAttackType type)
        {
            //base.DebugDraw(data, type);
        }

        protected Vector3 GetProjectilePosition(Transform attacker)
        {
            Vector3 up  = attacker.transform.up * (1 + AttackCollider.OrientationOffset.y);
            //Vector3 fwd = attacker.transform.forward * AttackCollider.OrientationOffset.z;
            return attacker.transform.position + up;
        }
    }
}