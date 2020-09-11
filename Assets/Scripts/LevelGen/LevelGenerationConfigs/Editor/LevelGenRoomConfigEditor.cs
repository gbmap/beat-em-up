using UnityEditor;

namespace Catacumba.LevelGen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LevelGenRoomConfig))]
    public class LevelGenBiomeConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}