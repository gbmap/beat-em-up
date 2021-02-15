
using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName = "Data/Item Characteristic/Weaponizable/Ranged (Multiple Projectiles)", fileName="WeaponizableRangedMultipleProjectiles")]
    public class CharacteristicWeaponizableRangedMultipleProjectiles : CharacteristicWeaponizableRanged
    {
        public int NumberOfProjectiles = 3;
        public float AngleRange = 60f;

        public override AttackResult[] Attack(CharacterData data, Transform origin, EAttackType attackType)
        {
            float angle = -AngleRange/2;
            float angleDelta = AngleRange/NumberOfProjectiles;
            for (int i = 0; i < NumberOfProjectiles; i++)
            {
                Quaternion rot = Quaternion.Euler(0f, data.transform.rotation.eulerAngles.y + angle, 0f);
                InstantiateProjectile(data, Projectile, rot);
                angle += angleDelta;
            }

            return null;
        }
    }
}