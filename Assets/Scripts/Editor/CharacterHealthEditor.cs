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

            CharacterData cd = health.GetComponent<CharacterData>();
            AttackRequest ar = new AttackRequest(cd, cd, EAttackType.Weak);

            CombatManager.AttackCharacter(ar);
            //health.TakeDamage(new CharacterAttackData(ar) { Damage = 100 });

            //health.TakeDamage();
            //health.TakeDamage(new CharacterAttackData(new AttackRequest(health)))
        }
    }
}
