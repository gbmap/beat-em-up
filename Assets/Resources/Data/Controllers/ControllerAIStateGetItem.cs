using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Character;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using Catacumba.Entity;
using UnityEngine;

namespace Catacumba.Data.Controllers
{
    [CreateAssetMenu(menuName="Data/Controllers/AI/States/Get Item", fileName="AI_GetItemState")]
    public class ControllerAIStateGetItem : ControllerAIState
    {
        private class ItemEquipPossibility
        {
            public ItemComponent Item;
            public BodyPart Slot;
        }

        public float SearchRadius = 3f;
        public LayerMask SearchLayers;

        public int TargetPriority = 10;

        private int _currentPriority = 0;
        public override int CurrentPriority => _currentPriority;

        private float _checkItemTimer = 0f;
        private float _checkItemTimerDelay = 2f;

        ItemEquipPossibility Target;
        bool HasTarget { get { return Target != null && Target.Item != null; } }

        CharacterMovementBase _movement;

        public override void Destroy(ControllerComponent component)
        {
        }

        public override void OnCreate(ControllerComponent component)
        {
            _movement = component.Data.Components.Movement;
        }

        public override void OnEnter(ControllerComponent component)
        {
            if (!HasTarget) return;
            _movement.SetDestination(Target.Item.transform.position);
        }

        public override void OnExit(ControllerComponent component)
        {
        }

        public override void OnUpdate(ControllerComponent component, ref ControllerCharacterInput input)
        {
            if (Target.Item == null)
            {
                _currentPriority = 0;
                return;
            }

            float distanceToDestination = Vector3.Distance(_movement.Destination, Target.Item.transform.position);
            if (distanceToDestination >= 1f)
                _movement.SetDestination(Target.Item.transform.position);

            float distanceToItem = Vector3.Distance(component.transform.position, Target.Item.transform.position);
            if (distanceToItem >= 1f)
            {
                input.Direction = _movement.Direction;
                return;
            }
            else
            {
                input.Interact = true;
            }
        }

        public override int UpdatePriority(ControllerComponent component)
        {
            if (Target != null) return TargetPriority;

            int priority = 0;
            Inventory inventory = component.Data.Stats.Inventory;

            _checkItemTimer += Time.deltaTime;
            if (_checkItemTimer <= _checkItemTimerDelay)
                return priority;
                
            _checkItemTimer = 0f;
            Collider[] items = Physics.OverlapSphere(component.transform.position, SearchRadius, SearchLayers);
            bool noItems = items == null || items.Length == 0;
            if (noItems) return priority;

            List<ItemEquipPossibility> acceptableItems = new List<ItemEquipPossibility>();

            ItemComponent[] itemComponents = items.Select(i => i.GetComponent<ItemComponent>()).ToArray();
            foreach (ItemComponent itemComponent in itemComponents)
            {
                Item item = itemComponent.Item;
                CharacteristicEquippable equippable = item.GetCharacteristic<CharacteristicEquippable>();

                CharacteristicEquippable.SlotData slotData = equippable.Slots.FirstOrDefault(s => Compare(inventory.GetSlot(s.BodyPart), item));
                if (slotData == null)
                    continue;

                acceptableItems.Add(new ItemEquipPossibility 
                {
                    Item = itemComponent,
                    Slot = slotData.BodyPart
                });
            }

            if (acceptableItems.Count == 0)
                return priority;

            Target = acceptableItems.OrderByDescending(i => i.Item.Item.Attributes.Sum()).First();
            priority = TargetPriority; 
            return priority;
        }

        private bool Compare(InventorySlot slot, Item item)
        {
            if (slot == null) return false;
            if (slot.IsEmpty()) return true;
            return slot.Item.Attributes.Sum() < item.Attributes.Sum();
        }

        private void CheckItems()
        {

        }
    }
}