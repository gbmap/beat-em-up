using System.Collections.Generic;
using UnityEngine;


public class UIManager : Singleton<UIManager> // TODO: trocar isso aqui por um ConfigurableSingleton
{
    public GameObject UILabel;

    private Dictionary<int, GameObject> labels = new Dictionary<int, GameObject>();

    public void CreateItemLabel(ItemData item)
    {
        var instance = Instantiate(UILabel, transform);
        instance.GetComponent<UIItemLabel>().SetItemData(ItemManager.Instance.GetItemConfig(item.TypeId));
        instance.GetComponent<UIWorldToScreen>().Target = item.transform;
        instance.SetActive(false);
        labels.Add(item.GetInstanceID(), instance);
    }

    public void DestroyItemLabel(ItemData item)
    {
        DestroyItemLabel(item.TypeId);
    }

    public void DestroyItemLabel(int id)
    {
        GameObject labelInstance = labels[id];
        labels.Remove(id);
        Destroy(labelInstance);
    }

    public void SetItemLabelVisibility(ItemData item, bool v)
    {
        labels[item.GetInstanceID()].SetActive(v);
    }
}
