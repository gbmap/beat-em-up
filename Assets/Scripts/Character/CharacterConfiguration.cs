using UnityEngine;

using Catacumba.Entity;
using Catacumba.Data.Items;

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
        public Inventory Inventory;
        public CharacterSkillConfiguration Skills;
        public CharacterViewConfiguration View;

        public void Configure(Entity.CharacterData character, System.Action CallbackFinished = null, int modelIndex = -1)
        {
            if (character.transform.childCount == 0 && View != null)
                View.Configure(character, modelIndex);

            CallbackFinished?.Invoke();
        }
    }
}