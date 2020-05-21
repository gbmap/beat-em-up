using UnityEngine;
using System.Linq;

public class CharacterStartingItems : MonoBehaviour
{
    [System.Serializable]
    public class StartingItemsProbability
    {
        public ItemConfig Item;
    }

    public StartingItemsProbability[] Items;

    [Range(0f, 1f)]
    public float Probability;

    private CharacterData data;

    void Awake()
    {
        if (Probability < Random.value) return;
        data = GetComponent<CharacterData>();
        var item = Items[Random.Range(0, Items.Length)].Item;
        data.StartingItems = data.StartingItems.Append(item).ToArray();

        Destroy(this);
    }
}
