using System.Linq;
using Catacumba.LevelGen;
using QFSW.QC;
using UnityEngine;
using static Catacumba.LevelGen.LevelGeneration;

namespace Catacumba.Data.Level
{
    public class QCBiomeTrapConfigurationParser : BasicQcParser<BiomeTrapConfiguration>
    {
        public override BiomeTrapConfiguration Parse(string value)
        {
            return BiomeTrapConfiguration.Load(value);
        }
    }

    [CreateAssetMenu(menuName="Data/Level/Biome Trap Configuration", fileName="BiomeTrapConfiguration")]
    public class BiomeTrapConfiguration : ScriptableObject
    {
        public TrapConfiguration[] Traps;

        public TrapConfiguration[] GetAvailableTrapsAt(
            LevelGen.Level l,
            Vector2Int pos,
            ECellCode cell
        ) {
            ELevelLayer layer = LevelGen.ELevelLayer.Hall 
                              | LevelGen.ELevelLayer.Rooms; 

            return Traps.Where(
                t => t.PossiblePlacements.Any(p => p.IsPosValid(l,pos, layer, cell))
            ).ToArray();
        }

		public static BiomeTrapConfiguration Load(string name)
		{
			return Resources.Load<BiomeTrapConfiguration>(
                $"Data/Level/TrapConfigurations/{name}"
            );
		}
    }
}