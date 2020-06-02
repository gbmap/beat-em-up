﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager : SimpleSingleton<AIManager>
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
        TargetInfo info;
        if (mapTargets.TryGetValue(target, out info))
        {
            return info.Attackers;
        }
        else
        {
            return -1;
        }
    }

    public void IncreaseAttackers(GameObject target)
    {
        mapTargets[target].Attackers = mapTargets[target].Attackers+1;
    }

    public void DecreaseAttackers(GameObject target)
    {
        TargetInfo t;
        if (!mapTargets.TryGetValue(target, out t)) return;
        mapTargets[target].Attackers--;
    }
}