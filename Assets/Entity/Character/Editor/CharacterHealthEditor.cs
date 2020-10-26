using UnityEngine;
using UnityEditor;
using Catacumba.Entity;

[CustomEditor(typeof(CharacterHealth))]
public class CharacterHealthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CharacterHealth health = target as CharacterHealth;

        if (GUILayout.Button("Take Damage"))
        {
            if (!Application.isPlaying) return;
            health.TakeDamage(new CharacterAttackData());
        }
    }
}
