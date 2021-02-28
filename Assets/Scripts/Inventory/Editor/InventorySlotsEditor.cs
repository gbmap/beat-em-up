using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Items;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(InventorySlots))]
public class InventorySlotsEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        InventorySlots slots = (InventorySlots)target;
        return new InventorySlotsVisualElement(slots.Slots);
    }
}
