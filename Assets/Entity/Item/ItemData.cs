using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : ConfigurableObject<ItemStats, int>
{
    private void OnDestroy()
    {
        // isso aqui tá dando exceção qnd fecha o jogo :(
        ItemManager.Instance.UnregisterItemInstance(gameObject.GetInstanceID());
    }

    private void Start()
    {
        Stats = ItemManager.Instance.RegisterItemInstance(this);
        ItemManager.Instance.SetupItem(gameObject, TypeId);
    }
}
