using UnityEngine;

namespace Catacumba.Data.Level
{
    [CreateAssetMenu(
        fileName="ObjectPlacementRef", 
        menuName="Data/Level/")]
    public class ObjectPlacementConfiguration : ScriptableObject
    {
        public GameObject[] PossiblePrefabs;
        public CellPlacementConfiguration[] PlacementConfigurations;
    }
}