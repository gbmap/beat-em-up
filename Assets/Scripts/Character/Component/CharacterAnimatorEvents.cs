using UnityEngine;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Configuration;

namespace Catacumba.Entity
{
    public class CharacterAnimatorEvents
    {
        public static void Attack(CharacterCombat combat, EAttackType type)
        {
            if (!combat) return;
            if (!combat.CanAttack) return;

            combat.AttackImmediate(type);
        }

        public static void Dash(CharacterData data, float force)
        {
            CharacterMovementBase movement = data.Components.Movement;
            if (!movement) return;
            Vector3 dir = movement.transform.forward;
            movement.ApplySpeedBump(dir, force * CharacterVariables.AttackDashForceWeak);


            PlaySound(data, "WOOSH");

            // TODO: optimize this mess.
            AudioClip sfx = data.Stats.Inventory.GetWeapon().GetCharacteristic<CharacteristicWeaponizable>().SoundsWoosh?.GetRandomClip();
            if (sfx)
                AudioSource.PlayClipAtPoint(sfx, data.transform.position);
        }

        public static void UseSkill(CharacterData cer, string skill)
        {

        }

        public static void EmitParticle(ParticleSystem system)
        {

        }

        public static void EmitParticleRing()
        {

        }

        public static void PlaySound(CharacterData data, string sound)
        {
            SFXPool pool = data.ConfigurationView.SFXBank.GetSound(sound);
            if (pool == null)
                pool = data.Stats.Inventory.GetWeapon()?.GetCharacteristic<CharacteristicWeaponizable>()?.SFXBank.GetSound(sound);

            if (pool == null)
                return;

            AudioClip clip = pool.GetRandomClip();
            AudioSource.PlayClipAtPoint(clip, data.transform.position);
        }
    }

}