using UnityEngine;

namespace Catacumba.Data 
{
	[CreateAssetMenu(menuName="Data/Level/Trap Configuration", fileName="TrapConfiguration")]
	public class TrapConfiguration : ScriptableObject
	{
		public GameObject Prefab;
		public CellPlacementConfiguration[] PossibleSpots;
	}
}
