using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;
using Frictionless;
using Catacumba.Character.AI;
using System;

public class CheckpointData
{
    public int Index;
    public string TargetCameraName;
    public int[] Items;
    public ItemConfig[] KeyItems;
    public Vector3 Position;
}

public class CheckpointManager : SimpleSingleton<CheckpointManager>
{
    public static bool Retry
    {
        get { return checkpointData != null; }
    }

    public static int CheckpointIndex = -1;
    private static CheckpointData checkpointData;

    public static GameObject CheckpointCamera
    {
        get
        {
            if (checkpointData == null) return null;
            return Instance.transform.GetChild(checkpointData.Index).GetComponent<Checkpoint>().TargetCamera;
        }
    }

    private void Awake()
    {
        if (!Retry)
            return;

        SetupPlayer();
    }

    private void OnEnable()
    {
        ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<MsgOnBossDied>(CB_OnBossDied);
    }

    private void OnDisable()
    {
        ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<MsgOnBossDied>(CB_OnBossDied);
    }

    private void CB_OnBossDied(MsgOnBossDied obj)
    {
        checkpointData = null;
        CheckpointIndex = -1;
    }

    private static void SetupPlayer()
    {
        CharacterPlayerInput input = FindObjectOfType<CharacterPlayerInput>();
        CharacterData data = input.GetComponent<CharacterData>();

        // Append starting items to player's
        ItemConfig[] configs = new ItemConfig[checkpointData.Items.Length];
        for (int i = 0; i < configs.Length; i++)
        {
            configs[i] = ItemManager.Instance.GetItemConfig(checkpointData.Items[i]);
        }

        List<ItemConfig> items = new List<ItemConfig>(configs);
        items.AddRange(data.StartingItems);
        data.StartingItems = items.ToArray();

        // Create new inventory with keys also
        data.Stats.Inventory = new Inventory(checkpointData.KeyItems);

        // Set player's checkpoint position
        input.GetComponent<NavMeshAgent>().Warp(checkpointData.Position);
    }

    public static void OnCheckpoint(Checkpoint c)
    {
        var input = FindObjectOfType<CharacterPlayerInput>();
        var inventory = input.GetComponent<CharacterData>().Stats.Inventory;

        CheckpointIndex = c.transform.GetSiblingIndex();
        checkpointData = new CheckpointData()
        {
            Index = c.Index,
            Items = (int[])inventory.ItemIds.Clone(),
            KeyItems = inventory.KeyItems.ToArray(),
            TargetCameraName = c.TargetCamera.name,
            Position = c.transform.position
        };
    }

    public static T DeepCopy<T>(T other)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, other);
            ms.Position = 0;
            return (T)formatter.Deserialize(ms);
        }
    }

}
