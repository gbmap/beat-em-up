using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Items.Characteristic
{
    [CreateAssetMenu(menuName = "Data/Item Characteristic/Weaponizable/Melee", fileName = "Weaponizable")]
    public class CharacteristicWeaponizableMelee : CharacteristicWeaponizable
    {
        public AttackColliderSize AttackColliderSize;

        public override CharacterAttackData[] Attack(CharacterData character, EAttackType attackType)
        {
            string layer = "Entities";

            Collider[] colliders = Physics.OverlapBox(
                GetColliderPosition(character), 
                GetColliderSize(character, attackType), 
                GetColliderRotation(character),
                1 << LayerMask.NameToLayer(layer)
            );

            if (colliders.Length == 0) return null;

            CharacterAttackData[] attackResults = new CharacterAttackData[colliders.Length];

            int hits = 0;
            foreach (var c in colliders)
            {
                if (c.gameObject == character.gameObject) continue;

                CharacterData defender = c.GetComponent<CharacterData>();

                AttackRequest request = new AttackRequest(character, defender, attackType);
                CharacterAttackData attackData = CombatManager.AttackCharacter(request);
                if (attackData == null) continue;

                attackResults[hits] = attackData;
                hits++;
            }

            return attackResults;
        }

        private Vector3 GetColliderPosition(CharacterData character)
        {
            return character.transform.position + (character.transform.forward*1.25f + Vector3.up);
        }

        private Vector3 GetColliderSize(CharacterData character, EAttackType attackType)
        {
            Vector3 attackColliderSize = (Vector3.one * 0.65f + Vector3.right * 0.65f); 
            attackColliderSize.x *= AttackColliderSize.Size.x;
            attackColliderSize.y *= AttackColliderSize.Size.y;
            attackColliderSize.z *= AttackColliderSize.Size.z;

            attackColliderSize *= attackType == EAttackType.Weak ? 1.0f : 1.5f;
            return (attackColliderSize + Vector3.one)/2f;
        }

        private Quaternion GetColliderRotation(CharacterData character)
        {
            return character.transform.rotation;
        }
        
        public override void DebugDraw(CharacterData data, EAttackType type)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(
                GetColliderPosition(data),
                GetColliderRotation(data),
                GetColliderSize(data, type)
            );
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}