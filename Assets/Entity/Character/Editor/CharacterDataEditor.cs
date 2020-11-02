using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Catacumba.Entity;
using Catacumba.Data;
using Catacumba.Data.Items;

[CustomEditor(typeof(CharacterData))]
public class CharacterDataEditor : Editor
{
    private void Awake()
    {
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CharacterData data = target as CharacterData;

        if (data.Stats != null)
        {
            EditorGUILayout.LabelField("Stats");
            EditorGUILayout.LabelField(StatsToString(data.Stats), GUILayout.Height(200f));

        }
    }

    private string StatsToString(CharacterStats stats) 
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendFormat("Strength: {0}\n", stats.Attributes.Strength);
        sb.AppendFormat("Dexterity: {0}\n", stats.Attributes.Dexterity);
        sb.AppendFormat("Magic: {0}\n", stats.Attributes.Magic);
        sb.AppendLine("-----");
        sb.AppendFormat("Stamina: {0}/{1}\n", stats.CurrentStamina, stats.Stamina);
        sb.AppendFormat("Health: {0}/{1}\n", stats.Health, stats.MaxHealth);
        sb.AppendFormat("Mana: {0}\n", stats.Mana, stats.MaxMana);
        sb.AppendLine("-----");
        if (stats.Inventory)
        {
            foreach (var slot in stats.Inventory.Slots)
            {
                Item item = slot.Item;
                string itemName = item ? item.Name : "Empty";
                sb.AppendFormat("{0}: {1}\n", slot.Part.name, itemName);
            }
        }
        return sb.ToString();
    }

    private static string FindAssetPath(CharacterData d)
    {
        string[] folders = { "Character/ ",
                             "Character/Enemies/" };
        string[] assets = AssetDatabase.FindAssets(d.gameObject.name);
        if (assets.Length > 1)
        {
            string msg = "Ambiguity when searching for asset. Multiple entries found: ";
            foreach (string asset in assets)
            {
                msg += "\n " + asset;
            }
            throw new System.Exception(msg);
        }
        if (assets.Length == 0)
        {
            throw new System.Exception("Couldn't find asset");
        }

        return AssetDatabase.GUIDToAssetPath(assets[0]);
    }
}
