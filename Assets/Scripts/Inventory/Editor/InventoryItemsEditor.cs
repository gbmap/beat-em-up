using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Items;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using BodyPart = Catacumba.Data.Character.BodyPart;

public class InventoryItemsVisualElement : VisualElement
{
    private InventoryItems InventoryItems;

    VisualElement _container;
    VisualElement _itemList;

    public InventoryItemsVisualElement(InventoryItems items) 
    {
        InventoryItems = items;

        _container = new VisualElement();

        ObjectField objField = new ObjectField("Slots");
        objField.objectType = typeof(InventorySlots);
        objField.value = items.Slots;
        objField.RegisterValueChangedCallback(delegate(ChangeEvent<Object> ce)
        {
            Cb_OnInventorySlotsChanged(ce);
        });
        _container.Add(objField);

        _itemList = new VisualElement();
        LoadItemList(_itemList,items);

        _container.Add(_itemList);
        Add(_container);
    }

    private void LoadItemList(VisualElement itemList, InventoryItems items)
    {
        int i = 0;
        IEnumerable<(BodyPart, Item)> it = items.GetEnumerable();
        foreach ((BodyPart, Item) t in it)
        {
            if (t.Item1 == null) continue;
            //_itemList.Add(new Label(t.Item1.name));

            ObjectField of = new ObjectField(t.Item1.name);
            of.objectType =typeof(Item);
            of.value = t.Item2;
            _itemList.Add(of);
        }
    }

    private void Cb_OnInventorySlotsChanged(ChangeEvent<Object> ce)
    {
        InventorySlots slots = ce.newValue as InventorySlots;
        InventoryItems.SetSlots(slots);
    }
}

[CustomEditor(typeof(InventoryItems))]
public class InventoryItemsEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        return new InventoryItemsVisualElement(target as InventoryItems);
    }
}
