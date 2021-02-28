using System.Collections;
using System.Collections.Generic;
using Catacumba.Data.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryEditorWindow : EditorWindow
{

    public void OnEnable()
    {
        VisualElement root = rootVisualElement;
        root.style.width = 800;
        root.style.height = 600;

        root.style.flexDirection = FlexDirection.Row;

        //List<Inventory> inventoryList = new List<Inventory>() 
    }
}
