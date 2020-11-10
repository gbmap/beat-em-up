using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data;
using Catacumba.Data.Items;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AttributesVisualElement : VisualElement
{
    private List<AttributeValueI> _target;

    private VisualElement _fieldsContainer;
    private List<AttributeData> _attributes;

    public AttributesVisualElement()
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Data/Items/Editor/AttributesVisualElement.uss");
        styleSheets.Add(styleSheet);

        _fieldsContainer = new VisualElement();
        _fieldsContainer.name = "fields";
        _attributes = new List<AttributeData>(Resources.LoadAll<AttributeData>(""));

        Add(new Label("Attributes"));
        Add(_fieldsContainer);

        Button btnCreate = new Button(Cb_OnAddClick);
        btnCreate.AddToClassList("btn-add");
        btnCreate.Add(new Label("Add"));
        Add(btnCreate);
    }

    public void SetReference(List<AttributeValueI> attributes)
    {
        _target = attributes;
        _fieldsContainer.Clear();

        for (int i = 0; i < attributes.Count; i++)
        {
            AttributeValueI attr = attributes[i];
            _fieldsContainer.Add(CreateListItem(attr, i));
        }
    }

    private VisualElement CreateListItem(AttributeValueI attr, int index)
    {
        VisualElement listItem = new VisualElement();
        listItem.style.flexDirection = FlexDirection.Row;
        listItem.name = $"item-{index}";

        listItem.Add(CreatePopupField(attr, index));

        IntegerField intf = new IntegerField();
        intf.value = attr.Value;
        intf.AddToClassList("attr_value");
        intf.RegisterValueChangedCallback(delegate (ChangeEvent<int> ce) { Cb_OnIntFieldValueChanged(ce, index); });
        listItem.Add(intf);
        
        Button btnRemove = new Button(() => Cb_OnRemoveClick(index));
        btnRemove.AddToClassList("attr_btn_remove");
        listItem.Add(btnRemove);
        return listItem;
    }

    private PopupField<AttributeData> CreatePopupField(AttributeValueI attr, int index)
    {
        Func<AttributeData, string> formatter = (attrData) => attrData.name;
        AttributeData defaultValue = _attributes.FirstOrDefault(a=>a==attr.Attribute) ?? _attributes[0];
        PopupField<AttributeData> pp = new PopupField<AttributeData>(_attributes, defaultValue, formatter, formatter);
        pp.AddToClassList("attr_popup");
        pp.RegisterValueChangedCallback(delegate (ChangeEvent<AttributeData> ce) { Cb_OnPopupItemValueChanged(ce, index);  });
        return pp;
    }
    
    private void Cb_OnIntFieldValueChanged(ChangeEvent<int> changeEvent, int index)
    {
        _target[index].Value = changeEvent.newValue;
    }

    private void Cb_OnPopupItemValueChanged(ChangeEvent<AttributeData> changeEvent, int index)
    {
        _target[index].Attribute = changeEvent.newValue;
    }

    private void Cb_OnRemoveClick(int index)
    {
        if (_target == null) return;
        _target.RemoveAt(index);
        _fieldsContainer.RemoveAt(index);
    }

    private void Cb_OnAddClick()
    {
        if (_target == null) return;
        AttributeValueI attribute = new AttributeValueI();
        attribute.Attribute = _attributes[0];
        _fieldsContainer.Add(CreateListItem(attribute, _target.Count));
        _target.Add(attribute);
    }

    public new class UxmlFactory : UxmlFactory<AttributesVisualElement, UxmlTraits>
    {
        public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        {
            return base.Create(bag, cc);
        }
    }
}


public class ItemCharacteristicsVisualElement : VisualElement
{
    private List<ItemCharacteristic> _target;

    private VisualElement _fieldsContainer;
    private List<ItemCharacteristic> _characteristics;

    public ItemCharacteristicsVisualElement()
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Data/Items/Editor/AttributesVisualElement.uss");
        styleSheets.Add(styleSheet);

        _fieldsContainer = new VisualElement();
        _fieldsContainer.name = "fields";
        _characteristics = new List<ItemCharacteristic>(Resources.LoadAll<ItemCharacteristic>(""));

        Add(new Label("Characteristics"));
        Add(_fieldsContainer);

        Button btnCreate = new Button(Cb_OnAddClick);
        btnCreate.AddToClassList("btn-add");
        btnCreate.Add(new Label("Add"));
        Add(btnCreate);
    }


    private VisualElement CreateListItem(ItemCharacteristic characteristic, int index)
    {
        VisualElement listItem = new VisualElement();
        listItem.style.flexDirection = FlexDirection.Row;
        listItem.name = $"item-{index}";

        listItem.Add(CreatePopupField(characteristic, index));

        Button btnRemove = new Button(() => Cb_OnRemoveClick(index));
        btnRemove.AddToClassList("attr_btn_remove");
        listItem.Add(btnRemove);
        return listItem;
    }

    private PopupField<ItemCharacteristic> CreatePopupField(ItemCharacteristic characteristic, int index)
    {
        Func<ItemCharacteristic, string> formatter = (attrData) => attrData.name;
        ItemCharacteristic defaultValue = _characteristics.FirstOrDefault(a=>a==characteristic) ?? _characteristics[0];
        PopupField<ItemCharacteristic> pp = new PopupField<ItemCharacteristic>(_characteristics, defaultValue, formatter, formatter);
        pp.AddToClassList("attr_popup");
        pp.RegisterValueChangedCallback(delegate (ChangeEvent<ItemCharacteristic> ce) { Cb_OnPopupItemValueChanged(ce, index);  });
        return pp;
    }

    public void SetReference(List<ItemCharacteristic> characteristics)
    {
        _target = characteristics;
        _fieldsContainer.Clear();

        for (int i = 0; i < characteristics.Count; i++)
        {
            ItemCharacteristic characteristic = characteristics[i];
            _fieldsContainer.Add(CreateListItem(characteristic, i));
        }
    }

    private void Cb_OnAddClick()
    {
        if (_target == null) return;
        ItemCharacteristic characteristic = _characteristics[0];
        _fieldsContainer.Add(CreateListItem(characteristic, _target.Count));
        _target.Add(characteristic);
    }

    private void Cb_OnRemoveClick(int index)
    {
        if (_target == null) return;
        _target.RemoveAt(index);
        _fieldsContainer.RemoveAt(index);
    }

    private void Cb_OnPopupItemValueChanged(ChangeEvent<ItemCharacteristic> changeEvent, int index)
    {
        _target[index] = changeEvent.newValue;
    }
}