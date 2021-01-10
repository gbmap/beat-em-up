using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Weapons.Strategies
{
    public abstract class WeaponAttackStrategy : ScriptableObject
    {
        public float DistanceToAttack = 2.25f;

        public abstract Vector3 ModulateDestinationPosition(
            Item item, 
            CharacterData character,
            CharacterData enemy
        );

        public abstract bool IsCloseEnoughToAttack(float distance);
    }
}