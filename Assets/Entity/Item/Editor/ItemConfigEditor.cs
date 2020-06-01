using UnityEditor;

namespace Catacumba
{
    using E = EditorGUILayout;

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
            E.LabelField("UI Config");
            itemConfig.Name = E.TextField("Name", itemConfig.Name);
            itemConfig.Description = E.TextField("Description", itemConfig.Description);

            E.Separator();
            E.LabelField("Stats", EditorStyles.boldLabel);

            var s = itemConfig.Stats;
            s.Id = E.IntField("Id", s.Id);
            s.ItemType = (EItemType)E.EnumPopup("Item Type", s.ItemType);
            s.Rarity = (EItemRarity)E.EnumPopup("Item Rarity", s.Rarity);

            if (s.ItemType == EItemType.Equip)
            {
                s.Slot = (EInventorySlot)E.EnumPopup("Slot", s.Slot);

                E.LabelField("Item Attributes", EditorStyles.boldLabel);
                DrawCharacterAttributeInt(ref s.Attributes);

                if (s.Slot == EInventorySlot.Weapon)
                {
                    E.Separator();

                    E.LabelField("Item Damage Scaling", EditorStyles.boldLabel);
                    DrawCharacterAttributeFloat(ref s.DamageScaling);

                    E.Separator();
                    s.WeaponType = (EWeaponType)E.EnumPopup("Weapon Type", s.WeaponType);

                    var skills = serializedObject.FindProperty("Stats.Skills");
                    E.PropertyField(skills, new UnityEngine.GUIContent("Skills"));
                }
            }

            E.Separator();
            E.LabelField("Visuals", EditorStyles.boldLabel);

            E.PropertyField(serializedObject.FindProperty("Prefab"));
            E.PropertyField(serializedObject.FindProperty("AnimationOverride"));

            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }

        /*
        private void DrawCharacterAttribute(ref TCharAttributes<int> a)
        {
            DrawCharacterAttributeGeneric(ref a, E.IntField);
        }

        private void DrawCharacterAttribute(ref CharAttributesF a)
        {
            DrawCharacterAttributeGeneric(ref a, E.FloatField);
        }

        private void DrawCharacterAttributeGeneric(ref TCharAttributes<T> attrs, System.Func<string, T, UnityEngine.GUILayoutOption[], T> drawFunc)
        {
            attrs.Vigor = drawFunc("Vigor", attrs.Vigor, null);
            attrs.Strength = drawFunc("Strength", attrs.Strength, null);
            attrs.Dexterity = drawFunc("Dexterity", attrs.Dexterity, null);
            attrs.Magic = drawFunc("Magic", attrs.Magic, null);
        }
        */

        private void DrawCharacterAttributeInt(ref CharAttributesI attrs)
        {
            attrs.Vigor = E.IntSlider("Vigor", attrs.Vigor, -100, 100);
            attrs.Strength = E.IntSlider("Strength", attrs.Strength, -100, 100);
            attrs.Dexterity = E.IntSlider("Dexterity", attrs.Dexterity, -100, 100);
            attrs.Magic = E.IntSlider("Magic", attrs.Magic, -100, 100);
        }

        private void DrawCharacterAttributeFloat(ref CharAttributesF attrs)
        {
            attrs.Vigor = E.Slider("Vigor", attrs.Vigor, 0f, 10f);
            attrs.Strength = E.Slider("Strength", attrs.Strength, 0f, 10f);
            attrs.Dexterity = E.Slider("Dexterity", attrs.Dexterity, 0f, 10f);
            attrs.Magic = E.Slider("Magic", attrs.Magic, 0f, 10f);
        }
    }

}