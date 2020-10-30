using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristic
{
    public abstract class CharacteristicWeaponizable : EquippableCharacteristic
    {
        public abstract CharacterAttackData[] Attack(CharacterData data, EAttackType attackType);
        public virtual void DebugDraw(CharacterData data, EAttackType type) {}
    }
}