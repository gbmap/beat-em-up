using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : Singleton<AIManager>
{
    private class TargetInfo
    {
        public int EnemiesTargetting;
        public int Attackers;
    }

    private Dictionary<GameObject, TargetInfo> mapTargets = new Dictionary<GameObject, TargetInfo>();

    private void Awake()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Array.ForEach(players, p => mapTargets.Add(p, new TargetInfo()));
    }

    public GameObject GetTarget(GameObject enemy)
    {
        var target = mapTargets.OrderBy(kvp => kvp.Value.EnemiesTargetting).FirstOrDefault();
        if (target.Value != null)
        {
            mapTargets[target.Key].EnemiesTargetting++;
        }
        return target.Key;
    }
    
    public void ClearTarget(GameObject target)
    {
        mapTargets[target].EnemiesTargetting--;
    }

    public int GetMaxAttackers(GameObject target)
    {
        if (target == null) return int.MaxValue;

        CharacterData characterData = target.GetComponent<CharacterData>();
        if (characterData == null) return 0;
        return Mathf.CeilToInt(characterData.Stats.Level*0.1f);
    }

    public int GetNumberOfAttackers(GameObject target)
    {
        try
        {
            return mapTargets[target].Attackers;
        }
        catch (KeyNotFoundException ex)
        {
            Debug.LogError(ex.Message);
            throw ex;
        }
    }

    public void IncreaseAttackers(GameObject target)
    {
        mapTargets[target].Attackers = mapTargets[target].Attackers+1;
    }

    public void DecreaseAttackers(GameObject target)
    {
        mapTargets[target].Attackers--;
    }
}
