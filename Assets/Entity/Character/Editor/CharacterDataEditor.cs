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
      

        lastType = d.TypeId;

        if (!GUILayout.Button("Test Models"))
            return;

        d.StartCoroutine(d.Test_AllCharacters());
    }

    private static string FindAssetPath(CharacterData d)
    {
        string[] folders = { "Character/ ",
                             "Character/Enemies/" };
        string[] assets = AssetDatabase.FindAssets(d.gameObject.name);
        if (assets.Length > 1)
        {
            string msg = "Ambiguity when searching for asset. Multiple entries found: ";
            foreach (string asset in assets)
            {
                msg += "\n " + asset;
            }
            throw new System.Exception(msg);
        }
        if (assets.Length == 0)
        {
            throw new System.Exception("Couldn't find asset");
        }

        return AssetDatabase.GUIDToAssetPath(assets[0]);
    }
}
