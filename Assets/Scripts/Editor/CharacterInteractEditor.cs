using System.Collections;
using System.Collections.Generic;
using Catacumba.Entity;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterInteract))]
public class CharacterInteractEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Interact"))
        {
            CharacterInteract interact = target as CharacterInteract;
            interact.Interact();
        }
    }
}
