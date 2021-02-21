using System;
using System.Text;
using Catacumba.LevelGen;
using UnityEngine;
using static Catacumba.LevelGen.LevelGeneration;
using Random = UnityEngine.Random;

namespace Catacumba.Data.Level
{
    //[CreateAssetMenu(menuName="Data/Level/Cell Placement Configuration", fileName="CellPlacement")]
	public abstract class CellPlacementConfiguration : ScriptableObject
	{
		public enum ERotationType
		{
			Fixed,
			RandomDiscreteInterval,
			RandomContinuousInterval
		}

		public ERotationType RotationType;
		[Header("Fixed")]
		public float FixedRotation = 0f;

		//
		// Divides 360 by DiscreteInterval,
		// Generates random number between 0-int(Result)
		// Multiplies random number by DiscreteInterval.
		[Header("Random Discrete Interval")]
		public float DiscreteInterval = 0f;

		// Simply rotates the object by a random number
		// between 0-ContinuousInterval.
		[Header("Random Continuous Interval")]
		public float ContinuousInterval = 0f;

        public abstract bool IsPosValid(LevelGen.Level l, 
                               Vector2Int pos, 
                               ELevelLayer layer,
                               ECellCode targetCell
        ); 	

		public void RotateObject(Transform obj)
		{
			Quaternion rotation = Quaternion.identity;
			switch (RotationType)
			{
				case ERotationType.Fixed: break;
				case ERotationType.RandomDiscreteInterval:
					int n = Mathf.RoundToInt(360f/DiscreteInterval);
					int i = Random.Range(0, n);
					rotation = Quaternion.Euler(0f, i*DiscreteInterval, 0f);
					break;
				case ERotationType.RandomContinuousInterval:
					rotation = Quaternion.Euler(0f, Random.Range(0,360f), 0f);
					break;
			}

			obj.localRotation = rotation;
		}
	}

}
