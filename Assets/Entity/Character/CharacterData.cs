using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using Catacumba.Data;

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
    public class CharacterData : MonoBehaviour
    {
        [Space]
        [Header("Configuration")]
        public Catacumba.Data.CharacterConfiguration CharacterCfg;

        public CharacterStats Stats;

        public ECharacterBrainType BrainType { get; private set; }

        public List<ItemData> ItemsInRange { get { return itemsInRange; } }
        private List<ItemData> itemsInRange = new List<ItemData>();

        public System.Action<CharacterComponentBase> OnComponentAdded;
        public System.Action<CharacterComponentBase> OnComponentRemoved;

        public List<CharacterComponentBase> CharacterComponents;

        void Awake()
        {
            BrainType = GetComponent<CharacterPlayerInput>() != null ? ECharacterBrainType.Input : ECharacterBrainType.AI;

            // Load basic cfg if no configuration is set.
            if (CharacterCfg == null) 
                CharacterCfg = Catacumba.Data.CharacterConfiguration.Default;

            // setup attribs
            Stats = new CharacterStats(CharacterCfg.Stats);

            CharacterComponents = new List<CharacterComponentBase>(GetComponentsInChildren<CharacterComponentBase>());
            OnComponentAdded += Callback_OnComponentAdded;
            OnComponentRemoved += Callback_OnComponentRemoved;
        }


        void Destroy()
        {
            OnComponentAdded -= Callback_OnComponentAdded;
            OnComponentRemoved -= Callback_OnComponentRemoved;
        }

        private void Callback_OnComponentAdded(CharacterComponentBase obj)
        {
            if (!CharacterComponents.Contains(obj))
                CharacterComponents.Add(obj);
        }

        private void Callback_OnComponentRemoved(CharacterComponentBase obj)
        {
            if (CharacterComponents.Contains(obj))
                CharacterComponents.Remove(obj);
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

    #if UNITY_EDITOR
        private bool showDebug = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                showDebug = !showDebug;
        }

        private void OnGUI()
        {
            if (!showDebug) return;

            /*if (data.BrainType != ECharacterBrainType.Input)
                return;*/

            Rect r = UIManager.WorldSpaceGUI(transform.position, Vector2.one * 200f);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (CharacterComponentBase component in CharacterComponents)
            { 
                sb.AppendFormat("--- {0} ---\n", component.GetType().Name);
                sb.AppendLine(component.GetDebugString());
            }

            GUI.Label(r, sb.ToString());
        }
    #endif
    }
}