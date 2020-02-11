﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : ConfigurableSingleton<CharacterManager, CharacterManagerConfig>
{
    protected override string Path => "Data/CharacterManagerConfig";

    private static Dictionary<int, CharacterStats> CharacterStats = new Dictionary<int, CharacterStats>();

    public static CharacterStats RegisterCharacter(int instanceId, CharacterStats stats)
    {
        CharacterStats[instanceId] = stats;
        return stats;
    }

    public static void UnregisterCharacter(int instanceId)
    {
        CharacterStats.Remove(instanceId);
    }

    public static CharacterStats GetCharacterStats(int instanceId)
    {
        return CharacterStats.ContainsKey(instanceId) ? CharacterStats[instanceId] : null;
    }


    public IEnumerator SetupCharacter(GameObject instance, ECharacterType type)
    {
        yield return Config.SetupCharacter(instance, type);
    }

    public IEnumerator SetupCharacter(CharacterData data)
    {
        yield return Config.SetupCharacter(data.gameObject, data.TypeId);
    }

    public bool Interact(CharacterData character, ItemStats item)
    {
        character.Stats.Inventory[item.Slot] = item;
        return true;
    }

    public bool UnEquip(CharacterData character, EInventorySlot slot)
    {
        CharacterStats stats = CharacterStats[character.gameObject.GetInstanceID()];
        if (!stats.Inventory.HasEquip(slot))
        {
            return false;
        }

        stats.Inventory.UnEquip(slot);

        return true;
    }
}
