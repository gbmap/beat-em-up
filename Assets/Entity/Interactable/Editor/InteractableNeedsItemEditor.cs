using UnityEditor;
using UnityEngine;

namespace Catacumba
{
    [CustomEditor(typeof(InteractableNeedsItem))]
    public class InteractableNeedsItemEditor : Editor
    {
        InteractableNeedsItem i;

        private void OnEnable()
        {
            i = target as InteractableNeedsItem;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            i.InteractionType = (EInteractType)EditorGUILayout.EnumPopup("Check Type", i.InteractionType);

            bool ic = i.InteractionType != EInteractType.None;

            EditorGUILayout.Separator();

            if (ic)
            {
                EditorGUILayout.LabelField("Check Config", EditorStyles.boldLabel);
                EditorGUILayout.Separator();

                switch (i.InteractionType)
                {
                    case EInteractType.ItemCheck:
                        EditorGUILayout.LabelField("Has Item:", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Item"));
                        break;
                    case EInteractType.AttributeCheck:
                        EditorGUILayout.LabelField("Attribute Check", EditorStyles.boldLabel);
                        i.AttrToCheck = (EAttribute)EditorGUILayout.EnumPopup("Attribute to Compare", i.AttrToCheck);
                        i.AttrOperation = (EAttributeOperation)EditorGUILayout.EnumPopup("Operation", i.AttrOperation);
                        i.AttrValue = EditorGUILayout.IntField("Target Value", i.AttrValue);
                        break;
                    case EInteractType.ObjectsDestroyed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetObjects"));
                        break;
                }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Popup Config", EditorStyles.boldLabel);
            i.HasItemMessage = EditorGUILayout.TextField(ic ? "Success Message" : "Message", i.HasItemMessage);
            if (ic)
                i.NoItemMessage = EditorGUILayout.TextField("Failure Message", i.NoItemMessage);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("EventHasItem"), new GUIContent(ic ? "Success Event" : "On Interact"));
            if (ic)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EventNoItem"), new GUIContent("Failure Event"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}