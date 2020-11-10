using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptableObjectPopupField : PopupField<ScriptableObject>
{
    public new class UxmlFactory : UxmlFactory<ScriptableObjectPopupField, UxmlTraits> 
    {
        public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        {
            return base.Create(bag, cc);
        }  

    }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        //UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
        UxmlStringAttributeDescription _type = new UxmlStringAttributeDescription { name = "type" };
        
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            ScriptableObjectPopupField target = (ScriptableObjectPopupField)ve;
            LogChildren(target.Children());


            string label =  "";
            bag.TryGetAttributeValue("label", out label);
            target.label = label; 

            string type = _type.GetValueFromBag(bag, cc);
            Type objType = Type.GetType(type + ", Assembly-CSharp");

            var resources = Resources.LoadAll(string.Empty, objType);
            foreach (var obj in resources)
            {
                Debug.Log(obj);
            }
        }

        private void LogChildren(IEnumerable<VisualElement> elements)
        {
            foreach (var child in elements)
            {
                Debug.Log(child.name + " " + child);
                LogChildren(child.Children());
            }
        }
    }
}
