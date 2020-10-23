using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

/********
*  Character component for instance-based data.
*  Holds stats, effects, inventory, etc.
*********/

namespace Catacumba.Entity 
{
    public enum ECharacterBrainType
    {
        AI,
        Input
    }

    [Serializable]
    public class CharacterData : ConfigurableObject<CharacterStats>
    {
        [Space]
        [Header("Configuration")]
        public Catacumba.Data.CharacterConfiguration CharacterCfg;

        public ECharacterBrainType BrainType { get; private set; }

        public List<ItemData> ItemsInRange { get { return itemsInRange; } }
        private List<ItemData> itemsInRange = new List<ItemData>();

        void Awake()
        {
            BrainType = GetComponent<CharacterPlayerInput>() != null ? ECharacterBrainType.Input : ECharacterBrainType.AI;

            // Load basic cfg if no configuration is set.
            if (CharacterCfg == null) 
                CharacterCfg = Catacumba.Data.CharacterConfiguration.Default;

            // setup attribs
            Stats = new CharacterStats(CharacterCfg.Stats);
        }

        private void Start()
        {
            CharacterCfg.Configure(this);
        }

        private bool ValidItem(ItemData item, bool enterExit)
        {
            return itemsInRange.Contains(item) ^ enterExit;
        }

        public void OnItemInRange(ItemData item)
        {
            if (ValidItem(item, true))
                itemsInRange.Add(item);
        }

        public void OnItemOutOfRange(ItemData item)
        {
            if (ValidItem(item, false))
                itemsInRange.Remove(item);
        }

        public bool Interact()
        {
            if (itemsInRange.Count == 0) return false;

            var item = itemsInRange[0];
            while (item == null)
            {
                itemsInRange.RemoveAt(0);

                if (itemsInRange.Count == 0)
                {
                    return false;
                }

                item = itemsInRange[0];
            }

            bool r = true;

            if (r)
            {
                OnItemOutOfRange(item);
                Destroy(item.gameObject);
            }
            return r;
        }
    }
}