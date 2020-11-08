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
            public CharacterAnimator Animator { get; private set; }
            public List<CharacterComponentBase> CharacterComponents { get; private set; }

            public ComponentsCache(
                CharacterData owner, 
                CharacterComponentBase[] components)
            {
                Owner = owner;
                CharacterComponents = new List<CharacterComponentBase>(components);

                foreach (CharacterComponentBase component in components)
                    Cache(component);
            }

            public void Cache(CharacterComponentBase component)
            {
                if (component is CharacterHealth)
                    Health = component as CharacterHealth;
                else if (component is CharacterCombat)
                    Combat = component as CharacterCombat;
                else if (component is CharacterMovementBase)
                    Movement = component as CharacterMovementBase;
                else if (component is CharacterAnimator)
                    Animator = component as CharacterAnimator;

                if (!CharacterComponents.Contains(component))
                    CharacterComponents.Add(component);
            }

            public void Decache(CharacterComponentBase component)
            {
                if (component is CharacterHealth)
                    Health = null;
                else if (component is CharacterCombat)
                    Combat = null;
                else if (component is CharacterMovementBase)
                    Movement = null;
                else if (component is CharacterAnimator)
                    Animator = null;

                if (CharacterComponents.Contains(component))
                    CharacterComponents.Remove(component);
            }

            public void OnComponentAdded(CharacterComponentBase newComponent)
            {
                ForEachComponent(c => c.OnComponentAdded(newComponent));

                // If initial configuration has been run, we should signal the newly added component.
                // It was most likely added after the object's initialization.
                if (!CharacterComponents.Contains(newComponent))
                {
                    if (Owner.IsConfigured)
                    {
                        // Fire OnComponentAdded for each existing component
                        // on the new component.
                        ForEachComponent(c => newComponent.OnComponentAdded(c));
                        newComponent.OnConfigurationEnded();
                    }
                }


                Cache(newComponent);
            }

            public void OnComponentRemoved(CharacterComponentBase removedComponent)
            {
                ForEachComponent(c => c.OnComponentRemoved(removedComponent));

                // Make sure removedComponent's events are unsubscribed from other components
                ForEachComponent(c => removedComponent.OnComponentRemoved(c));
                Decache(removedComponent);
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

        [HideInInspector] public CharacterStats Stats;

        public ECharacterBrainType BrainType { get; private set; }

        public bool IsConfigured 
        {
            get; private set;
        }

        public ComponentsCache Components { get; private set; }

        void Awake()
        {
            ControllerComponent controller = GetComponent<ControllerComponent>(); 
            if (controller && controller.Controller)
                BrainType = controller.Controller.BrainType;
            else
                BrainType = ECharacterBrainType.AI;

            SetupCharacterStats();
            SetupCharacterComponentsCache();
        }

        private void SetupCharacterStats()
        {
            if (CharacterCfg == null) 
                CharacterCfg = Catacumba.Data.CharacterConfiguration.Default;

            Stats = new CharacterStats(CharacterCfg.Stats, CharacterCfg.Inventory);
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
            Stats.Inventory.DispatchItemEquippedForAllItems();

            IsConfigured = true; 
            // This is set after firing the event on all CharacterComponents
            // because it might be useful for them to know that this
            // character has just been instantiated.
        }

        public void SignalComponentAdded(CharacterComponentBase component)
        {
            Components.OnComponentAdded(component);        
        }

        public void SignalComponentRemoved(CharacterComponentBase component)
        {
            Components.OnComponentRemoved(component);        
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

            Rect r = WorldSpaceGUI(transform.position, Vector2.one * 500f);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Components.ForEachComponent(c => sb.AppendFormat("--- {0} ---\n{1}\n", c.GetType().Name, c.GetDebugString()));

            GUI.Label(r, sb.ToString());
        }

        public static Rect WorldSpaceGUI(Vector3 worldPosition, Vector2 size)
        {
            Vector3 posW = worldPosition;
            Vector2 pos = Camera.main.WorldToScreenPoint(posW);

            Rect r = new Rect(pos, size);
            r.y = Screen.height - pos.y;
            return r;
        }

    #endif
    }
}