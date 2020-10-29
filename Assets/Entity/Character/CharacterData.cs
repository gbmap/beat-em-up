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
        public class ComponentsCache
        {
            // Most referenced components
            public CharacterData Owner { get; private set; }
            public CharacterMovementBase Movement { get; private set; }
            public CharacterCombat Combat { get; private set; }
            public CharacterHealth Health { get; private set; }
            public List<CharacterComponentBase> CharacterComponents { get; private set; }

            public ComponentsCache(
                CharacterData owner, 
                CharacterComponentBase[] components)
            {
                CharacterComponents = new List<CharacterComponentBase>();

                foreach (CharacterComponentBase component in components)
                    Cache(component);

                Owner = owner;
                Owner.OnComponentAdded += Callback_OnComponentAdded;
                Owner.OnComponentRemoved += Callback_OnComponentRemoved;
            }

            ~ComponentsCache()
            {
                Owner.OnComponentAdded -= Callback_OnComponentAdded;
                Owner.OnComponentRemoved -= Callback_OnComponentRemoved;
            }

            public void Cache(CharacterComponentBase component)
            {
                if (component is CharacterHealth)
                    Health = component as CharacterHealth;
                else if (component is CharacterCombat)
                    Combat = component as CharacterCombat;
                else if (component is CharacterMovementBase)
                    Movement = component as CharacterMovementBase;
            }

            public void Decache(CharacterComponentBase component)
            {
                if (component is CharacterHealth)
                    Health = null;
                else if (component is CharacterCombat)
                    Combat = null;
                else if (component is CharacterMovementBase)
                    Movement = null;
            }

            private void Callback_OnComponentAdded(CharacterComponentBase obj)
            {
                if (!CharacterComponents.Contains(obj))
                {
                    CharacterComponents.Add(obj);

                    // If initial configuration has been run, we should signal the newly added component.
                    // It was most likely added after the object's initialization.
                    if (Owner.IsConfigured)
                        obj.OnConfigurationEnded();
                }

                Cache(obj);
            }

            private void Callback_OnComponentRemoved(CharacterComponentBase obj)
            {
                if (CharacterComponents.Contains(obj))
                    CharacterComponents.Remove(obj);

                Decache(obj);
            }

            public void ForEachComponent(System.Action<CharacterComponentBase> function)
            {
                foreach (CharacterComponentBase component in CharacterComponents)
                    function(component);
            }
        }

        [Space]
        [Header("Configuration")]
        public Catacumba.Data.CharacterConfiguration CharacterCfg;

        public CharacterStats Stats;

        public ECharacterBrainType BrainType { get; private set; }

        public List<ItemData> ItemsInRange { get { return itemsInRange; } }
        private List<ItemData> itemsInRange = new List<ItemData>();

        public System.Action<CharacterComponentBase> OnComponentAdded;
        public System.Action<CharacterComponentBase> OnComponentRemoved;

        private bool IsConfigured = false;

        public ComponentsCache Components { get; private set; }

        void Awake()
        {
            BrainType = GetComponent<CharacterPlayerInput>() != null ? ECharacterBrainType.Input : ECharacterBrainType.AI;

            // Load basic cfg if no configuration is set.
            if (CharacterCfg == null) 
                CharacterCfg = Catacumba.Data.CharacterConfiguration.Default;

            // setup attribs
            Stats = new CharacterStats(CharacterCfg.Stats);

            SetupCharacterComponentsCache();
        }

        private void SetupCharacterComponentsCache()
        {
            Components = new ComponentsCache(this, GetComponentsInChildren<CharacterComponentBase>());
        }

        void Destroy()
        {
            Components = null;
        }

        private void Start()
        {
            CharacterCfg.Configure(this, OnCharacterConfigurationEnded);
        }

        private void OnCharacterConfigurationEnded()
        {
            Components.ForEachComponent(c => c.OnConfigurationEnded());

            IsConfigured = true;
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
            Components.ForEachComponent(c => sb.AppendFormat("--- {0} ---\n{1}\n", c.GetType().Name, c.GetDebugString()));

            GUI.Label(r, sb.ToString());
        }
    #endif
    }
}