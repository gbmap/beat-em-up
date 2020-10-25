using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ConfigurableObject<StatsData>: MonoBehaviour
{
    public StatsData Stats { get; protected set; }
}
