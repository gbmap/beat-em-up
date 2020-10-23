using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu()]
    public class CharacterConfiguration : ScriptableObject
    {
        private const string DEFAULT = "Data/Characters/Character_Default";

        private static CharacterConfiguration _defaultInstance;
        public static CharacterConfiguration Default
        {
            get 
            {
                return _defaultInstance ?? (_defaultInstance = Resources.Load<CharacterConfiguration>(DEFAULT));
            }
        }

        public CharacterStatConfiguration Stats;
        public CharacterSkillConfiguration Skills;
        public CharacterViewConfiguration View;
    }
}