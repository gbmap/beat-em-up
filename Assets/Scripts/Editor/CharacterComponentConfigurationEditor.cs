using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Catacumba.Data;
using Catacumba.Entity;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

public class CharacterComponentListVisualElement : VisualElement
{
    private CharacterComponentConfiguration _configuration;
    private List<string> _target;

    private VisualElement _fieldsContainer;
    private List<string> _components;

    public CharacterComponentListVisualElement()
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/CharacterComponentConfigurationEditor.uss");
        styleSheets.Add(styleSheet);

        _fieldsContainer = new VisualElement();
        _fieldsContainer.name = "fields";
        _components = new List<string>(GetComponents());

        Add(new Label("Components"));
        Add(_fieldsContainer);

        Button btnCreate = new Button(Cb_OnAddClick);
        btnCreate.AddToClassList("btn-add");
        btnCreate.Add(new Label("Add"));
        Add(btnCreate);
    }


    private VisualElement CreateListItem(string characteristic, int index)
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

    private PopupField<string> CreatePopupField(string type, int index)
    {
        Func<string, string> formatter = (strType) => strType;
        string defaultValue = _components.FirstOrDefault(a=>a==type) ?? _components[0];
        PopupField<string> pp = new PopupField<string>(_components, defaultValue, formatter, formatter);
        pp.AddToClassList("attr_popup");
        pp.RegisterValueChangedCallback(delegate (ChangeEvent<string> ce) { Cb_OnPopupItemValueChanged(ce, index);  });
        return pp;
    }

    public void SetReference(CharacterComponentConfiguration configuration)
    {
        _configuration = configuration;
        _target = configuration.ComponentFullNames;
        _fieldsContainer.Clear();

        for (int i = 0; i < _target.Count; i++)
        {
            string component = _target[i];
            _fieldsContainer.Add(CreateListItem(component, i));
        }

    }

    private void Cb_OnAddClick()
    {
        if (_target == null) return;
        string characteristic = _components[0];
        _fieldsContainer.Add(CreateListItem(characteristic, _target.Count));
        _target.Add(characteristic);

        _configuration.ComponentFullNames = _target;
        EditorUtility.SetDirty(_configuration);
    }

    private void Cb_OnRemoveClick(int index)
    {
        if (_target == null) return;
        _target.RemoveAt(index);
        _fieldsContainer.RemoveAt(index);

        _configuration.ComponentFullNames = _target;
        EditorUtility.SetDirty(_configuration);
    }

    private void Cb_OnPopupItemValueChanged(ChangeEvent<string> changeEvent, int index)
    {
        _target[index] = changeEvent.newValue;
        _configuration.ComponentFullNames = _target;
        EditorUtility.SetDirty(_configuration);
    }

    private IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
    {
        return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
    }

    private IEnumerable<string> GetComponents()
    {
        Type type = typeof(CharacterComponentBase);
        return FindDerivedTypes(type.Assembly, type).Select(t => t.FullName);
    }
}

[CustomEditor(typeof(CharacterComponentConfiguration))]
public class CharacterComponentConfigurationEditor : Editor
{
    VisualElement rootElement;
    public void OnEnable()
    {
        rootElement = new VisualElement();

        // Load in UXML template and USS styles, then apply them to the root element.
        string uxmlTemplate = "Assets/Scripts/Editor/CharacterComponentConfigurationEditor.uxml";
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlTemplate);
        visualTree.CloneTree(rootElement);

        CharacterComponentListVisualElement cclve = new CharacterComponentListVisualElement();
        cclve.SetReference((target as CharacterComponentConfiguration));
        rootElement.Add(cclve);

        
    }

    public override VisualElement CreateInspectorGUI()
    {
        //return base.CreateInspectorGUI();
        return rootElement;
    }

    /*
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Type baseCharacterComponent = typeof(CharacterComponentBase);
        var components = FindDerivedTypes(baseCharacterComponent.Assembly, baseCharacterComponent);
        foreach (var componentType in components)
        {
            EditorGUILayout.LabelField(componentType.Name);
        }

        rootElement = new VisualElement();

        // Load in UXML template and USS styles, then apply them to the root element.
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Star System Editor/StarSystemEditor.uxml");
        visualTree.CloneTree(rootElement);
    }
    */

}
