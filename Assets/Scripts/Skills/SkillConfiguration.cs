using System.Collections;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data
{
    public enum ESkillType
    {
        Active,
        Targeted,
        Area 
    }

    //[CreateAssetMenu(menuName="Catacumba/Skill", fileName="SkillConfiguration")]
    public abstract class SkillConfiguration : ScriptableObject
    {
        public string Name;
        public string Description;
        public ESkillType Type;
        public GameObject Prefab;
        public float Cooldown;

        private enum ESkillErrorCode
        {
            None,
            NoTargetProvided,
            NoPositionProvided
        }

        private bool ValidParameters(Hashtable parameters, out ESkillErrorCode error)
        {
            bool isValid = parameters.ContainsKey("caster");
            error = ESkillErrorCode.None;

            switch(Type)
            {
                case ESkillType.Targeted:
                    isValid &= parameters.ContainsKey("target");
                    error = ESkillErrorCode.NoTargetProvided;
                    break;
                case ESkillType.Area:
                    isValid &= parameters.ContainsKey("position");
                    error = ESkillErrorCode.NoPositionProvided;
                    break;
                case ESkillType.Active:
                default:
                    break;
            }

            return isValid;
        }

        public void Use(CharacterData caster, Hashtable parameters)
        {
            parameters.Add("caster", caster);
            if (!ValidParameters(parameters, out ESkillErrorCode error))
            {
                Log.Error(ELogSystemBitmask.Skills, $"{caster.name} couldn't cast skill {Name}. Reason: {error}");
                return;
            }
        }
    }
}