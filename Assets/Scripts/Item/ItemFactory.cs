﻿using System;
using Catacumba.Data.Interactions;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using QFSW.QC;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Entity
{
    public class QCItemParser : BasicQcParser<Item>
    {
        public override Item Parse(string itemConfiguration)
        {
            return Resources.Load<Item>($"{ItemFactory.PATH_ITEMS}/{itemConfiguration}");
        }
    }

    public static class ItemFactory 
    {    
        public const string PATH_ITEMS = "Data/Items";
        public const string PATH_INTERACTIONS = "Data/Interactions";

        [Command("spawn_item")]
        public static void Command_CreateItem(string itemConfiguration)
        {
            Item item = Resources.Load<Item>($"{PATH_ITEMS}/{itemConfiguration}");
            if (item == null)
            {
                QuantumConsole.Instance.LogToConsole($"Couldn't load item: {itemConfiguration}");
                return;
            }
            
            GameObject instance = new GameObject(itemConfiguration);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.layer = LayerMask.NameToLayer("Item");
            instance.tag = "Item";

            GameObject modelRoot = new GameObject("ModelRoot");
            modelRoot.transform.SetParent(instance.transform);
            modelRoot.transform.localPosition = Vector3.zero; 
            modelRoot.transform.localRotation = Quaternion.identity;

            Animator animator = instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = ItemTemplateConfiguration.Default.AnimatorController;

            GameObject highlight = GameObject.Instantiate(ItemTemplateConfiguration.Default.Highlight);
            highlight.transform.SetParent(instance.transform);
            highlight.transform.localScale    = Vector3.one;
            highlight.transform.localPosition = Vector3.zero;
            highlight.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            ItemComponent itemComponent      = instance.AddComponent<ItemComponent>();
                          itemComponent.Item = item;

            InteractiveBaseComponent interactiveComponent = CreateInteractiveComponent(itemComponent);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(instance.transform.position, out hit, 1f, NavMesh.AllAreas))
                instance.transform.position = hit.position;
        }

        public static InteractiveBaseComponent CreateInteractiveComponent(ItemComponent component)
        {
            Item item = component.Item;
            InteractiveBaseComponent interactive = null;
            Type interactiveType = null;
            string interactionName = string.Empty;
            bool isEquippable = item.HasCharacteristic<CharacteristicEquippable>();
            bool isStackable = item.HasCharacteristic<CharacteristicStackable>();
            bool isConsumable = item.HasCharacteristic<CharacteristicConsumable>();
            if (!isEquippable || (isEquippable && (isStackable || isConsumable)))
            {
                interactiveType = typeof(InteractiveImmediateComponent);
                interactionName = "InteractionGrabItemFromComponent";
            }
            else // equippable but not stackable or consumable
            {
                interactiveType = typeof(InteractiveComponent);
                interactionName = "InteractionEquipItemFromComponent";
            }

            interactive = (InteractiveBaseComponent)component.gameObject.AddComponent(interactiveType);
            interactive.Interaction = Resources.Load<Interaction>($"{PATH_INTERACTIONS}/{interactionName}");
            return interactive;
        }
    }
}