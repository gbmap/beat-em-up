using NUnit.Framework;
using Catacumba.Data;
using UnityEngine;

namespace Tests
{
    public class CellPlacementConfigurationTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void Test_Rotation()
        {
			CellPlacementConfiguration cfg = new CellPlacementConfiguration()
			{
				Pattern = "001011001",
				RowWidth = 3,
				AllowRotation = true
			};

			string nPattern = cfg.RotatePattern90Degrees(cfg.Pattern, cfg.RowWidth); 
			Assert.AreEqual(nPattern, "000010111");

			nPattern = cfg.RotatePattern90Degrees(nPattern, cfg.RowWidth); 
			Assert.AreEqual(nPattern, "100110100");
        }

		[Test]
		public void Test_IndexToOffset()
		{
			string p     = "001011001";
			int w        = 3;
			Vector2Int[] offsets = new Vector2Int[]
			{
				new Vector2Int(-1, -1),
				new Vector2Int(0, -1),
				new Vector2Int(1, -1),
				new Vector2Int(-1, 0),
				new Vector2Int(0, 0),
				new Vector2Int(1, 0),
				new Vector2Int(-1, 1),
				new Vector2Int(0, 1),
				new Vector2Int(1, 1)
			};

			for (int i = 0; i < offsets.Length; i++)
			{
				Vector2Int offset = CellPlacementConfiguration.IndexToOffset(p, w, i);
				Assert.AreEqual(offset, offsets[i]);
			}
		}

		[Test]
		public void Test_IndexToPosition()
		{
			int w = 3;
			Vector2Int[] positions = new Vector2Int[]
			{
				new Vector2Int(0, 0),
				new Vector2Int(1, 0),
				new Vector2Int(2, 0),
				new Vector2Int(0, 1),
				new Vector2Int(1, 1),
				new Vector2Int(2, 1),
				new Vector2Int(0, 2),
				new Vector2Int(1, 2),
				new Vector2Int(2, 2),
			};

			for (int i = 0; i < positions.Length; i++)
            {
                Vector2Int position = CellPlacementConfiguration.IndexToPosition(i, w);
				Assert.AreEqual(positions[i], position);
			}
		}
    }
}
