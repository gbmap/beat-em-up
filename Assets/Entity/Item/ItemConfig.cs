using UnityEditor;
using UnityEngine;

public class ItemConfig : ScriptableObject
{
    [MenuItem("Assets/Create/Item/ItemConfig")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<ItemConfig>();
    }

    public string Name;
    public string Description;
    public ItemStats Stats;
    public GameObject Prefab;
}
