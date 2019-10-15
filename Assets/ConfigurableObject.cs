using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConfigurableObject<StatsData, TypeEnum> : MonoBehaviour
{
    public TypeEnum Id;
    public StatsData Stats { get; protected set; }

    [Header("Data Initialization")]
    public bool InitData = false;
    public StatsData DataInit;

    public bool Randomize = false;
}
