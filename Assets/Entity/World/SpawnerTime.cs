using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnerTime : MonoBehaviour
{
    public GameObject[] Prefabs;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Spawn", 10f, 5f);
    }

    void Spawn()
    {
        int count = UnityEngine.Random.Range(1, 4);
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = UnityEngine.Random.insideUnitCircle;
            var obj = Instantiate(Prefabs[UnityEngine.Random.Range(0, Prefabs.Length)], new Vector3(pos.x, 0f, pos.y) * 5f, Quaternion.identity);

            /*CharacterData data = obj.GetComponent<CharacterData>();
            data.TypeId = GetRandomType();
            data.Stats.Attributes = new CharAttributesI()
            {
                Vigor = UnityEngine.Random.Range(0, 5),
                Dexterity = UnityEngine.Random.Range(0, 5),
                Magic = UnityEngine.Random.Range(0, 5),
                Strength = UnityEngine.Random.Range(0, 5)
            };
            */
        }
    }

    ECharacterType GetRandomType()
    {
        var values = Enum.GetValues(typeof(ECharacterType));
        ECharacterType[] skip = {
                ECharacterType.AdventurePackBegin,
                ECharacterType.AdventurePackEnd,
                ECharacterType.None,
                ECharacterType.DungeonPackBegin,
                ECharacterType.DungeonPackEnd,
                ECharacterType.FantasyRivalsBegin,
                ECharacterType.FantasyRivalsEnd,
                ECharacterType.KnightsBegin,
                ECharacterType.KnightsEnd,
                ECharacterType.ModularCharacter
            };

        ECharacterType type = ECharacterType.AdventurePackBegin;
        while (skip.Contains(type))
        {
            type = (ECharacterType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        return type;
    }
}
