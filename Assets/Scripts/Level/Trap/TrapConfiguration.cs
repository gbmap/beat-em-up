using UnityEngine;

namespace Catacumba.Data.Level
{
	[CreateAssetMenu(menuName="Data/Level/Trap Configuration", fileName="TrapConfiguration")]
	public class TrapConfiguration : ScriptableObject
	{
		public CharacterConfiguration Trap;
		public CellPlacementConfiguration[] PossiblePlacements;

		public static TrapConfiguration Load(string name)
		{
			return Resources.Load<TrapConfiguration>($"Data/Level/TrapConfigurations/{name}");
		}
	}
}
