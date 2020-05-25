using UnityEditor;

[CustomEditor(typeof(ItemConfig))]
public class ItemConfigEditor : Editor
{
    ItemConfig itemConfig;

    private void OnEnable()
    {
        itemConfig = (ItemConfig)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //base.OnInspectorGUI();
        EditorGUILayout.LabelField("UI Config");
        itemConfig.Name = EditorGUILayout.TextField("Name", itemConfig.Name);
        itemConfig.Description = EditorGUILayout.TextField("Description", itemConfig.Description);
        
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

        var s = itemConfig.Stats;
        s.Id = EditorGUILayout.IntField("Id", s.Id);
        s.ItemType = (EItemType)EditorGUILayout.EnumPopup("Item Type", s.ItemType);
        s.Rarity = (EItemRarity)EditorGUILayout.EnumPopup("Item Rarity", s.Rarity);

        if (s.ItemType == EItemType.Equip)
        {
            s.Slot = (EInventorySlot)EditorGUILayout.EnumPopup("Slot", s.Slot);

            EditorGUILayout.LabelField("Item Attributes", EditorStyles.boldLabel);
            DrawCharacterAttribute(s.Attributes);

            if (s.Slot == EInventorySlot.Weapon)
            {
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Item Damage Scaling", EditorStyles.boldLabel);
                DrawCharacterAttribute(s.DamageScaling);

                EditorGUILayout.Separator();
                s.WeaponType = (EWeaponType)EditorGUILayout.EnumPopup("Weapon Type", s.WeaponType);

                var skills = serializedObject.FindProperty("Stats.Skills");
                EditorGUILayout.PropertyField(skills, new UnityEngine.GUIContent("Skills"));
            }
        }
                
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Prefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimationOverride"));

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCharacterAttribute(CharAttributesI a)
    {
        DrawCharacterAttribute(a, EditorGUILayout.IntField);
    }

    private void DrawCharacterAttribute(CharAttributesF a)
    {
        DrawCharacterAttribute(a, EditorGUILayout.FloatField);
    }

    private void DrawCharacterAttribute<T>(TCharAttributes<T> attrs, System.Func<string, T, UnityEngine.GUILayoutOption[], T> drawFunc)
    {
        attrs.Vigor = drawFunc("Vigor", attrs.Vigor, null);
        attrs.Strength = drawFunc("Strength", attrs.Strength, null);
        attrs.Dexterity = drawFunc("Dexterity", attrs.Dexterity, null);
        attrs.Magic = drawFunc("Magic", attrs.Magic, null);
    }

}
