using UnityEngine;

using Catacumba.Entity;
using Catacumba.Data.Items;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Character Configuration", fileName="Character_")]
    public class CharacterConfiguration : ScriptableObject
    {
        public const string DEFAULT_FOLDER = "Data/Characters";
        public const string DEFAULT = "Data/Characters/Character_Default";

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
        public CharacterComponentConfiguration Components;

        /// <summary> 
        /// Sets configuration objects to CharacterData and adds
        /// components used in CharacterComponentConfiguration. Doesn't actually
        /// sets up the character object.
        /// </summary>
        public void Configure(Entity.CharacterData character, System.Action CallbackFinished = null)
        {
            character.ConfigurationStats = Stats;
            character.ConfigurationView = View;
            character.ConfigurationInventory = Inventory;
            Components.AddComponentsToObject(character.gameObject);
        }
    }
}