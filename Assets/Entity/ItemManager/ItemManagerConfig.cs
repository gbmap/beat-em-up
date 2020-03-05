using System;
using System.Collections.Generic;
using UnityEngine;


public class ItemManagerConfig : ScriptableObject
{
    /*[MenuItem("Assets/Create/Item/ItemManagerConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<ItemManagerConfig>();
    }*/

    public GameObject ItemPrefab;
    public Material[] RarityMaterials;
    private Dictionary<int, ItemConfig> items;
    private Dictionary<int, ItemConfig> Items
    {
        get
        {
            if (items == null)
            {
                items = new Dictionary<int, ItemConfig>();
                Array.ForEach(Resources.LoadAll<ItemConfig>("Data/Items"), item => items[item.Stats.Id] = item);
            }
            return items;
        }
    }

    public Material GetRarityColor(EItemRarity rarity)
    {
        try
        {
            return RarityMaterials[(int)rarity];
        }
        catch
        {
            return RarityMaterials[0];
        }
    }

    internal ItemConfig GetItemConfig(int id)
    {
        ItemConfig item = null;
        if (Items.TryGetValue(id, out item))
        {
            return item;
        }
        return null;
    }

    public ItemStats GetItemStats(int id)
    {
        ItemConfig item = null;
        if (Items.TryGetValue(id, out item))
        {
            return item.Stats;
        }
        return null;
    }
}
