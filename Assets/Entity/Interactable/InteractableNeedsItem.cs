using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InteractEvent : UnityEvent { }

[RequireComponent(typeof(Interactable))]
public class InteractableNeedsItem : MonoBehaviour
{
    public ItemConfig Item;
    public string HasItemMessage;
    public string NoItemMessage;

    Interactable interactable;

    public InteractEvent EventHasItem;
    public InteractEvent EventNoItem;

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
        if (Item == null || character.Stats.Inventory.HasKey(Item))
        {
            DialogueBox.Show(HasItemMessage);
            EventHasItem.Invoke();
        }
        else
        {
            DialogueBox.Show(NoItemMessage);
            EventNoItem.Invoke();
        }
    }
}
