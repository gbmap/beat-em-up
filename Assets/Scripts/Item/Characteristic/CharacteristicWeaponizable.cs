using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Character;
using Catacumba.Effects;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    public abstract class CharacteristicWeaponizable : CharacteristicEquippable
    {
        public ParticleEffectConfiguration AttackEffect;
        public ParticleEffectConfiguration HitEffect;
        public WeaponType WeaponType;

        public abstract AttackResult[] Attack(CharacterData data, Transform origin, EAttackType attackType);
        public virtual void DebugDraw(CharacterData data, EAttackType type) {}

        public override bool Equip(CharacterData data, Item item, BodyPart slot)
        {
            bool equips = base.Equip(data, item, slot);
            if (!equips) return false;
            return true;

            /*
            if (WeaponType.animatorController == null)
                return true;

            data.Components.Animator.UpdateAnimator(WeaponType.animatorController);
            return true;
            */
        }
    }
}