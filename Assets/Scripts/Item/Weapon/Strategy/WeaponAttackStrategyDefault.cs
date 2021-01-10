using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Weapons.Strategies
{
    [CreateAssetMenu(menuName="Data/Item/Weapons/Strategies/Default", fileName="WeaponAttackStrategyDefault")]
    public class WeaponAttackStrategyDefault : WeaponAttackStrategy
    {
        public override Vector3 ModulateDestinationPosition(
            Item item, 
            CharacterData character,
            CharacterData enemy
        )
        {
            Vector3 delta = enemy.transform.position - character.transform.position;
            float d = delta.magnitude;
            float md = DistanceToAttack;
            float td = d - Mathf.Max(d - md, 0f);
            return delta.normalized * d;
        }

        public override bool IsCloseEnoughToAttack(float distance)
        {
            return distance < DistanceToAttack;
        }
    }
}