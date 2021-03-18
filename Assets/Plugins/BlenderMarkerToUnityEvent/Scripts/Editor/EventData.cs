using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EventInfo
{
	public float time = 0.0f;
	public string value;
}

[System.Serializable]
public class ActionInfo
{
	public string name;
	public List<EventInfo> eventList = new List<EventInfo>();
}

public class EventData : ScriptableObject
{
	public List<ActionInfo> actionlist = new List<ActionInfo>();
}