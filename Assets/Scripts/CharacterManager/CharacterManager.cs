using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : Singleton<CharacterManager>
{
    private static Dictionary<int, CharacterStats> CharacterStats = new Dictionary<int, CharacterStats>();

    public static CharacterStats RegisterCharacter(int instanceId)
    {
        return RegisterCharacter(instanceId, new CharacterStats());
    }

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

    private CharacterManagerConfig config;
    public CharacterManagerConfig Config
    {
        get { return config ?? (config = Resources.Load<CharacterManagerConfig>("Data/CharacterManagerConfig")); }
    }

    public IEnumerator SetupCharacter(GameObject instance, ECharacterType type)
    {
        yield return Config.SetupCharacter(instance, type);
    }
}
