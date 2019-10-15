using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : ConfigurableObject<ItemStats, int>
{
    private void Awake()
    {
        Stats = ItemManager.Instance.GetItem(Id);
    }

    private void Start()
    {
        ItemManager.Instance.SetupItem(gameObject, Id);
    }
}
