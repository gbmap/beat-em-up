using System.Collections.Generic;

public static class CharacterManager
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
}
