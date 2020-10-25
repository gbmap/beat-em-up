using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Catacumba.Entity;
using Catacumba.Data;

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

        public InteractEvent EventHasItem = new InteractEvent();
        public InteractEvent EventNoItem = new InteractEvent();

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
                if (string.IsNullOrEmpty(HasItemMessage))
                {
                    CB_HasItem();
                }
                else
                {
                    DialogueBox.Show(HasItemMessage, CB_HasItem);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(NoItemMessage))
                {
                    CB_NoItem();
                }
                else
                {
                    DialogueBox.Show(NoItemMessage, CB_NoItem);
                }
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

        private void CB_HasItem()
        {
            EventHasItem.Invoke();
            Destroy(this);
            var interactable = GetComponent<Interactable>();
            if (interactable) Destroy(interactable);
        }
        
        private void CB_NoItem()
        {
            EventNoItem.Invoke();
        }
    }

}