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
            bool changed = false;

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
                changed |= DrawCharacterAttributeInt(ref s.Attributes);

                if (s.Slot == EInventorySlot.Weapon)
                {
                    E.Separator();

                    E.LabelField("Item Damage Scaling", EditorStyles.boldLabel);
                    changed |= DrawCharacterAttributeFloat(ref s.DamageScaling);

                    E.Separator();
                    s.WeaponType = (EWeaponType)E.EnumPopup("Weapon Type", s.WeaponType);

                    var skills = serializedObject.FindProperty("Stats.Skills");
                    E.PropertyField(skills, new UnityEngine.GUIContent("Skills"));

                    s.WeaponColliderScaling = E.Slider("Collider Scaling", s.WeaponColliderScaling, 0f, 3f);
                }
            }

            E.Separator();
            E.LabelField("Visuals", EditorStyles.boldLabel);

            E.PropertyField(serializedObject.FindProperty("Prefab"));
            E.PropertyField(serializedObject.FindProperty("AnimationOverride"));

            if (changed)
            {
                EditorUtility.SetDirty(target);
            }
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

        private int IntCheck(int v, string name, int min, int max, ref bool changed)
        {
            int lv = v;
            v = E.IntSlider(name, v, min, max);
            changed |= lv != v;
            return v;
        }

        private float FloatCheck(string name, float v, float min, float max, ref bool changed)
        {
            float lv = v;
            v = E.Slider(name, v, min, max);
            changed |= lv != v;
            return v;
        }

        private bool DrawCharacterAttributeInt(ref CharAttributesI attrs)
        {
            bool changed = false;
            attrs.Vigor = IntCheck(attrs.Vigor, "Vigor", -100, 100, ref changed);
            attrs.Strength = IntCheck(attrs.Strength, "Strength", -100, 100, ref changed);
            attrs.Dexterity = IntCheck(attrs.Dexterity, "Dexterity", -100, 100, ref changed);
            attrs.Magic = IntCheck(attrs.Magic, "Magic", -100, 100, ref changed);
            return changed;
        }

        private bool DrawCharacterAttributeFloat(ref CharAttributesF attrs)
        {
            bool changed = false;
            attrs.Vigor = FloatCheck("Vigor", attrs.Vigor, 0f, 10f, ref changed);
            attrs.Strength = FloatCheck("Strength", attrs.Strength, 0f, 10f, ref changed);
            attrs.Dexterity = FloatCheck("Dexterity", attrs.Dexterity, 0f, 10f, ref changed);
            attrs.Magic = FloatCheck("Magic", attrs.Magic, 0f, 10f, ref changed);
            return changed;
        }
    }

}