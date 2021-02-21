using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
using Catacumba.Data.Items;
using System.Linq;
using Catacumba.Data;


public class ItemEditorWindow : EditorWindow
{
    [MenuItem("Catacumba/Item Editor %i")]
    public static void ShowExample()
    {
        ItemEditorWindow wnd = GetWindow<ItemEditorWindow>();
        wnd.titleContent = new GUIContent("ItemEditorWindow");
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        root.style.width = 800;
        root.style.height = 600;

        root.style.flexDirection = FlexDirection.Row;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Resources/Data/Items/Editor/ItemEditorWindow.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Data/Items/Editor/ItemEditorWindow.uss");
        root.styleSheets.Add(styleSheet);

        List<Item> itemList = new List<Item>(Resources.LoadAll<Item>("Data/Items/"));
        ListView listView = LoadItemListView(root, itemList);
        LoadItemView(root);

        listView.selectedIndex = 0;
    }

    private ListView LoadItemListView(VisualElement root, List<Item> itemList)
    {
        Func<VisualElement> makeItem = () => new Label();
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = itemList[i].Name;

        ListView listview = root.Query<ListView>("item-list").First();
        // listview.styleSheets.Add(styleSheet);
        listview.makeItem = makeItem;
        listview.bindItem = bindItem;
        listview.onSelectionChanged += SelectionChanged;
        listview.itemsSource = itemList;
        listview.Refresh();
        return listview;
    }

    private void LoadItemView(VisualElement root)
    {
        VisualElement itemRarityContainer = root.Query<VisualElement>("item-rarity");

        List<ItemRarity> itemRarities = new List<ItemRarity>(Resources.LoadAll<ItemRarity>("").OrderBy(i=>i.name));
        Func<ItemRarity, string> formatRarityLabel = delegate(ItemRarity itemRarity) { return itemRarity.name; };
        PopupField<ItemRarity> elItemRarity = new PopupField<ItemRarity>("Rarity", itemRarities, itemRarities[0], formatRarityLabel, formatRarityLabel );
        elItemRarity.name = "item-rarity-popup";
        itemRarityContainer.Add(elItemRarity);

        VisualElement itemAttributesContainer = root.Query<VisualElement>("item-attribs");
        AttributesVisualElement ave = new AttributesVisualElement();
        ave.name = "item-attribs-list";
        itemAttributesContainer.Add(ave);

        VisualElement itemCharacteristicsContainer = root.Query<VisualElement>("item-characteristics");
        ItemCharacteristicsVisualElement icve = new ItemCharacteristicsVisualElement();
        icve.name = "item-characteristics-list";
        itemCharacteristicsContainer.Add(icve);
    }

    private void SelectionChanged(List<object> obj)
    {
        Item item = obj[0] as Item;
        SerializedObject so = new SerializedObject(item);

        VisualElement root = rootVisualElement;
        //root.Query<TextField>("item-name").First().value = item.Name;
        root.Query<TextField>("item-name").First().BindProperty(so.FindProperty("Name"));
        root.Query<TextField>("item-description").First().BindProperty(so.FindProperty("Description"));
        root.Query<PopupField<ItemRarity>>("item-rarity-popup").First().value = item.Rarity;
        root.Query<AttributesVisualElement>("item-attribs-list").First().SetReference(item.Attributes);
        root.Query<ItemCharacteristicsVisualElement>("item-characteristics-list").First().SetReference(item.Characteristics);
    }
}