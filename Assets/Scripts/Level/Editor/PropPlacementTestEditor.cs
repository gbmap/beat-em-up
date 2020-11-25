using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PropPlacementTest))]
public class PropPlacementTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Organize"))
            (target as PropPlacementTest).OrganizeProps();
    }

}
