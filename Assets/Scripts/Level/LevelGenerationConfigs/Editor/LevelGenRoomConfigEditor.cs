using Catacumba.Data.Level;
using UnityEditor;

namespace Catacumba.LevelGen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoomConfiguration))]
    public class LevelGenBiomeConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}