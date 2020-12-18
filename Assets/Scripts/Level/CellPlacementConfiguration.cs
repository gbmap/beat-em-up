using System;
using System.Text;
using UnityEngine;

namespace Catacumba.Data
{
    [CreateAssetMenu(menuName="Data/Level/Cell Placement Configuration", fileName="CellPlacement")]
	public class CellPlacementConfiguration : ScriptableObject
	{
		public string Pattern       = "000000000";  // 001 011 001 = 3x3 cell matrix where 1 = cell exists
		public int    RowWidth      = 3;            // Pattern row width
		public bool   AllowRotation = true;         // Does the pattern should be checked with 4 90 degrees rotations? 

		public string RotatePattern90Degrees()
		{
			return RotatePattern90Degrees(Pattern, RowWidth);
		}

		public string RotatePattern90Degrees(string pattern, int rowWidth, bool debug=false)
		{
			int w = rowWidth;

			StringBuilder result = new StringBuilder(pattern);
			for (int i = 0; i < pattern.Length; i++)
			{
				int v = (int)Char.GetNumericValue(pattern[i]);
				int x = i % w;
				int y = Mathf.FloorToInt(i / w);

				int offsetX = (w-1) - y - x;
				int offsetY = -y + x;

				int index = ConvertOffsetToLinear(x+offsetX, y+offsetY);
				result[index] = v == 1 ? '1' : '0';

				if (debug) {
					Debug.Log($"I: {i} X: {x} Y: {y} OffsetX: {offsetX} OffsetY: {offsetY} NewIndex: {index} V: {v}");
				}
			}
			return result.ToString();
		}

		public int ConvertOffsetToLinear(int x, int y)
		{
			int w = RowWidth;
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
			Vector2Int center   = new Vector2Int(Mathf.FloorToInt(w/2), Mathf.FloorToInt(w/2));
			Vector2Int position = IndexToPosition(index, w);
			return position - center;
		}

		public static bool IsPositionValid(
				string pattern, 
			    int w, 
			    LevelGen.Level l, 
			    Vector2Int initialPosition
		) {
			for (int i = 0; i < pattern.Length; i++)
			{
				Vector2Int pos = IndexToPosition(i, w);
			}
			return true;
		}
	}

}