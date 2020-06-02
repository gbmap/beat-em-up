using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Catacumba
{
    public enum EInteractType
    {
        None,
        ItemCheck,
        AttributeCheck,
        ObjectsDestroyed
    }

    public enum EAttributeOperation
    {
        Greater,
        Less,
        Equals
    }

    [System.Serializable]
    public class InteractEvent : UnityEvent { }

    [RequireComponent(typeof(Interactable))]
    public class InteractableNeedsItem : MonoBehaviour
    {
        public EInteractType InteractionType;

        // === ITEM CHECK
        public ItemConfig Item;

        // === ATTRIBUTE CHECK
        public EAttribute AttrToCheck;
        public EAttributeOperation AttrOperation;
        public int AttrValue;

        // === OBJECTS DESTROYED
        public GameObject[] TargetObjects;

        public string HasItemMessage;
        public string NoItemMessage;

        public InteractEvent EventHasItem;
        public InteractEvent EventNoItem;

        Interactable interactable;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            interactable.OnInteract += OnInteract;
        }

        private void OnDisable()
        {
            interactable.OnInteract -= OnInteract;
        }

        private void OnInteract(CharacterData character)
        {
            if (EvaluateInteraction(character))
            {
                DialogueBox.Show(HasItemMessage, delegate 
                {
                    EventHasItem.Invoke();
                    Destroy(this);
                    var interactable = GetComponent<Interactable>();
                    if (interactable) Destroy(interactable);
                });
            }
            else
            {
                DialogueBox.Show(NoItemMessage, delegate { EventNoItem.Invoke(); });
            }
        }

        public bool EvaluateInteraction(CharacterData character)
        {
            switch (InteractionType)
            {
                case EInteractType.None: return true;
                case EInteractType.ItemCheck: return character.Stats.Inventory.HasKey(Item);
                case EInteractType.AttributeCheck:
                    int value = character.Stats.GetAttributeTotal(AttrToCheck); 
                    switch (AttrOperation)
                    {
                        case EAttributeOperation.Equals: return value == AttrValue;
                        case EAttributeOperation.Greater: return value >= AttrValue;
                        case EAttributeOperation.Less: return value <= AttrValue;
                        default: return false;
                    }
                case EInteractType.ObjectsDestroyed:
                    return !TargetObjects.Any(o => o != null);
                default:
                    return true;
            }
        }
    }

}