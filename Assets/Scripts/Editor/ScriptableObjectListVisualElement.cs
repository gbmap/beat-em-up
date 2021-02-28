using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UIElements;
using System;
using System.Linq;
using UnityEditor;

public abstract class ScriptableObjectListVisualElement<T> : VisualElement where T : ScriptableObject
{
    protected List<T> availableItems;
    private List<T> _target;
    protected VisualElement _fieldsContainer;

    protected abstract string Title { get; }

    public ScriptableObjectListVisualElement(List<T> target)
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/ScriptableObjectListVisualElement.uss");
        styleSheets.Add(styleSheet);

        _target = target;
        availableItems = new List<T>(Resources.LoadAll<T>(""));

        CreateView();
    }

    private void CreateView()
    {
        Add(new Label(Title));
        _fieldsContainer = new VisualElement();
        _fieldsContainer.name = "fields";
        _fieldsContainer.Clear();

        for (int i = 0; i < _target.Count; i++)
        {
            T item = _target[i];
            _fieldsContainer.Add(CreateListItem(item, i));
        }
        Add(_fieldsContainer);

        Button btnCreate = new Button(Cb_OnAddClick);
        btnCreate.AddToClassList("btn-add");
        btnCreate.Add(new Label("Add"));
        Add(btnCreate);
    }


    protected virtual T CreateItem()
    {
        return availableItems[0];
    }

    protected virtual VisualElement CreateListItem(T item, int index)
    {

        VisualElement listItem = new VisualElement();
        listItem.style.flexDirection = FlexDirection.Row;
        listItem.name = $"item-{index}";

        FillListItem(listItem, item, index);
        
        Button btnRemove = new Button(() => Cb_OnRemoveClick(index));
        btnRemove.AddToClassList("btn-remove");
        listItem.Add(btnRemove);
        return listItem;
    }

    protected virtual void FillListItem(VisualElement listItem, T item, int index)
    {
        listItem.Add(CreatePopupField(item, index));
    }

    private PopupField<T> CreatePopupField(T item, int index)
    {
        Func<T, string> formatter = (i) => i.name;
        T defaultValue = _target.FirstOrDefault(a=>a.name == item.name) ?? availableItems[0];
        PopupField<T> pp = new PopupField<T>(availableItems, defaultValue, formatter, formatter);
        pp.AddToClassList("popup");
        pp.RegisterValueChangedCallback(delegate (ChangeEvent<T> ce) { Cb_OnPopupItemValueChanged(ce, index);  });
        return pp;
    }

    ////////////////////////////// 
    //   EVENTS

    private void Cb_OnAddClick()
    {
        if (_target == null) return;
        T item = CreateItem();
        _fieldsContainer.Add(CreateListItem(item, _target.Count));
        _target.Add(item);
    }

    private void Cb_OnRemoveClick(int index)
    {
        if (_target == null) return;
        _target.RemoveAt(index);
        _fieldsContainer.RemoveAt(index);
    }

    /*
        Called when a new object is assigned.
    */
    private void Cb_OnPopupItemValueChanged(ChangeEvent<T> ce, int index)
    {
        _target[index] = ce.newValue;
    }

}
