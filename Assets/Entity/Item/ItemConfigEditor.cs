using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/*
[CustomEditor(typeof(ItemConfig))]
public class ItemConfigEditor : Editor
{
    private ItemConfig itemConfig;
    private bool foldout;

    private void OnEnable()
    {
        itemConfig = (ItemConfig)target;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        itemConfig.Name = EditorGUILayout.TextField("Name", itemConfig.Name);
        itemConfig.Description = EditorGUILayout.TextField("Description", itemConfig.Description);
        itemConfig.Prefab = EditorGUILayout.ObjectField("Prefab", itemConfig.Prefab, typeof(GameObject), false) as GameObject;

        foldout = EditorGUILayout.Foldout(foldout, "Stats");
        if (foldout)
        {
            itemConfig.Stats.Id = EditorGUILayout.IntField(itemConfig.Stats.Id, "Id");
            itemConfig.Stats.ItemType = (EItemType)EditorGUILayout.EnumPopup("Item Type", itemConfig.Stats.ItemType);
            if (itemConfig.Stats.ItemType == EItemType.Equip)
            {
                itemConfig.Stats.Slot = (EInventorySlot)EditorGUILayout.EnumPopup("Inventory Slot", itemConfig.Stats.Slot);
                if (itemConfig.Stats.Slot == EInventorySlot.Weapon)
                {
                    itemConfig.Stats.WeaponType = (EWeaponType)EditorGUILayout.EnumPopup("Weapon Type", itemConfig.Stats.WeaponType);
                }

                CharAttributesI attributes = itemConfig.Stats.Attributes;
                attributes.Vigor = EditorGUILayout.IntField(attributes.Vigor, "Vigor");
                attributes. = EditorGUILayout.IntField(attributes.Vigor, "Vigor");
                attributes.Vigor = EditorGUILayout.IntField(attributes.Vigor, "Vigor");
                attributes.Vigor = EditorGUILayout.IntField(attributes.Vigor, "Vigor");



                public CharAttributesF DamageScaling;
                public Skill Skill;
            }
        }

}
*/