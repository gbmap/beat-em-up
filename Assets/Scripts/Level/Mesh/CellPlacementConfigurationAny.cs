using Catacumba.LevelGen;
using UnityEngine;

namespace Catacumba.Data.Level
{
    [CreateAssetMenu(menuName="Data/Level/Cell Placement/Any", fileName="CellPlacementConfigurationAny")]
    public class CellPlacementConfigurationAny : CellPlacementConfiguration
    {
        public override bool IsPosValid(
            LevelGen.Level l, 
            Vector2Int pos, 
            ELevelLayer layer, 
            LevelGeneration.ECellCode targetCell
        ) {
            return true;
        }
    }
}