using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : ConfigurableSingleton<CharacterManager, CharacterManagerConfig>
{
    protected override string Path => "Data/CharacterManagerConfig";

    public IEnumerator SetupCharacter(GameObject instance, ECharacterType type)
    {
        yield return Config.SetupCharacter(instance, type);
    }

    public IEnumerator SetupCharacter(CharacterData data)
    {
        yield return Config.SetupCharacter(data.gameObject, data.TypeId);
    }

    public IEnumerator SetupCharacter(GameObject instance, GameObject model)
    {
        yield return CharacterManagerConfig.SetupCharacter(instance, model);
    }

    public bool Interact(CharacterData character, ItemStats item)
    {
        character.Stats.Inventory[item.Slot] = item;
        return true;
    }

    public bool UnEquip(CharacterData character, EInventorySlot slot)
    {
        CharacterStats stats = character.Stats;
        if (!stats.Inventory.HasEquip(slot))
        {
            return false;
        }

        stats.Inventory.UnEquip(slot);

        return true;
    }
}
