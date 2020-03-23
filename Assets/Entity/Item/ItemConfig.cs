﻿using UnityEngine;

public class ItemConfig : ScriptableObject
{
    /*[MenuItem("Assets/Create/Item/ItemConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<ItemConfig>();
    }*/

    public string Name;
    public string Description;
    public GameObject Prefab;

    public ItemStats Stats;
    public AnimatorOverrideController AnimationOverride;

    public bool CustomSlashColors = false;
    public Gradient SlashColors;
}
