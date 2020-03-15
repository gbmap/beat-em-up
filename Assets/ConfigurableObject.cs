using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ConfigurableObject<StatsData, TypeEnum> : MonoBehaviour
{
    public TypeEnum TypeId;
    public StatsData Stats { get; protected set; }
}
