using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterData))]
public class CharacterDataEditor : Editor
{
    ECharacterType lastType;

    CharacterManagerConfig cfg;

    private void Awake()
    {
        cfg = Resources.Load<CharacterManagerConfig>("Data/CharacterManagerConfig");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CharacterData d = (CharacterData)target;

        if (d.TypeId != lastType)
        {
            var prefab = cfg.GetPrefab(d.TypeId);
            if (prefab != null)
            {
                EditorGUIUtility.PingObject(prefab.prefab);
            }
        }

        lastType = d.TypeId;
    }
}
