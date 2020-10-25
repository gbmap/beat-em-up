using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data;

public class ItemManager : ConfigurableSingleton<ItemManager, ItemManagerConfig>
{
    protected override string Path => "Data/ItemManagerConfig";

    public ItemStats RegisterItemInstance(ItemData item)
    {
        //UIManager.Instance.CreateItemLabel(item);
        //return GetItem(item.TypeId);
        return null;
    }

    public void UnregisterItemInstance(int instanceId)
    {
        //UIManager.Instance.DestroyItemLabel(instanceId);
    }

    public ItemConfig GetItemConfig(int id)
    {
        return Config.GetItemConfig(id);
    }

    public ItemStats GetItem(int id)
    {
        return Config.GetItemStats(id);
    }

    internal void SetupItem(GameObject gameObject, int id)
    {
        var config = GetItemConfig(id);
        SetupItem(gameObject, config);
    }

    internal void SetupItem(GameObject gameObject, ItemConfig config)
    {
        var modelRoot = gameObject.transform.Find("ModelRoot");
        if (modelRoot.transform.childCount > 0)
        {
            for (int i = 0; i < modelRoot.childCount; i++)
            {
                Destroy(modelRoot.transform.GetChild(i).gameObject);
            }
        }

        var instance = Instantiate(config.Prefab, modelRoot);
        instance.transform.localPosition = Vector3.zero;

        gameObject.transform.Find("Highlight").GetComponent<MeshRenderer>().material = Config.GetRarityColor(config.Stats.Rarity);
    }

    public GameObject SpawnItem(Vector3 position, int typeId)
    {
        ItemConfig cfg = Config.GetItemConfig(typeId);
        var instance = GameObject.Instantiate(Config.ItemPrefab, position, Quaternion.identity);
        instance.GetComponent<ItemData>().itemConfig = cfg;
        return instance;
    }
}
