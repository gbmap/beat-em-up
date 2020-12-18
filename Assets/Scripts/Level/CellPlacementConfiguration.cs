using System;
using System.Text;
using Catacumba.LevelGen;
using UnityEngine;
using static Catacumba.LevelGen.LevelGeneration;

namespace Catacumba.Data.Level
{
    [CreateAssetMenu(menuName="Data/Level/Cell Placement Configuration", fileName="CellPlacement")]
	public class CellPlacementConfiguration : ScriptableObject
	{
		public string Pattern       = "000000000";  // 001 011 001 = 3x3 cell matrix where 1 = cell exists
		public int    RowWidth      = 3;            // Pattern row width
		public bool   AllowRotation = true;         // Does the pattern should be checked with 4 90 degrees rotations? 

		public string Rotate90Degrees()
		{
			return CellPlacementConfiguration.RotatePattern90Degrees(Pattern, RowWidth);
		}

        public bool IsPosValid(LevelGen.Level l, 
                               Vector2Int pos, 
                               ELevelLayer layer,
                               ECellCode targetCell
        ) {
            return IsPositionValid(this, l, pos, layer, targetCell);
        }

		//////////////////////////////	
		///	STATIC METHODS 
		///

		public static string RotatePattern90Degrees(
				string pattern, 
				int rowWidth, 
				bool debug=false
		) {
			int w = rowWidth;

			StringBuilder result = new StringBuilder(pattern);
			for (int i = 0; i < pattern.Length; i++)
			{
				int v = (int)Char.GetNumericValue(pattern[i]);
				int x = i % w;
				int y = Mathf.FloorToInt(i / w);

				int offsetX = (w-1) - y - x;
				int offsetY = -y + x;

				int index = ConvertOffsetToLinear(x+offsetX, y+offsetY, rowWidth);
				result[index] = v == 1 ? '1' : '0';

				if (debug) {
					Debug.Log($"I: {i} X: {x} Y: {y} OffsetX: {offsetX} OffsetY: {offsetY} NewIndex: {index} V: {v}");
				}
			}
			return result.ToString();
		}
	
		public static int ConvertOffsetToLinear(int x, int y, int w)
		{
			return x + w * y;
		}

		public static Vector2Int IndexToPosition(int index, int w)
		{
			return new Vector2Int(index % w, Mathf.FloorToInt(index/w));
		}

        // Returns an offset from the center of the specified grid based on an index.
		public static Vector2Int IndexToOffset(
				string pattern,
				int w, 
				int index
		) {
			int x = Mathf.FloorToInt(w/2); 
			Vector2Int center   = new Vector2Int(x,x);
			Vector2Int position = IndexToPosition(index, w);
			return position - center;
		}

		public static bool IsPositionValid(
			CellPlacementConfiguration cfg, 
			LevelGen.Level l,
			Vector2Int initialPosition,
			ELevelLayer layer,
            ECellCode cell
		) {
			return IsPositionValid(
					cfg.Pattern, 
					cfg.RowWidth, 
					cfg.AllowRotation,
					l, 
					initialPosition, 
					layer,
                    cell
			); 
		}

		public static bool IsPositionValid(
			string pattern, 
            int w, 
			bool allowRotation,
            LevelGen.Level l, 
            Vector2Int initialPosition,
			ELevelLayer layer,
            ECellCode cell
		) {

			bool result = TestPosition(pattern, w, l, initialPosition, layer, cell);
			if (!allowRotation)
				return result;

			for (int i = 0; i < 3; i++)
			{
				pattern = RotatePattern90Degrees(pattern, w);	
				result = TestPosition(pattern, w, l, initialPosition, layer, cell);
				if (result)
					return true;
			}

			return false;
		}

		private static bool TestPosition(
			string pattern, 
            int w, 
            LevelGen.Level l, 
            Vector2Int initialPosition,
			ELevelLayer layer,
            ECellCode cell
        ) {
			for (int i = 0; i < pattern.Length; i++)
			{
				Vector2Int offset = IndexToOffset(pattern, w, i);
				Vector2Int cellPos = initialPosition + offset;
				var lcell = l.BaseSector.GetCell(cellPos, layer);

				int targetValue  = (int)Char.GetNumericValue(pattern[i]);
                bool isEqualCell = lcell == cell;

                bool v = ((targetValue == 1 && isEqualCell) ||
                          (targetValue == 0 && lcell <= ECellCode.Empty));
				if (!v)
					return false;
			}
			return true;
		}
	}
}
