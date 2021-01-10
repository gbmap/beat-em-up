using NUnit.Framework;
using UnityEngine;
using Catacumba.Data;
using Catacumba.LevelGen;
using Catacumba.Data.Level;
using static Catacumba.LevelGen.LevelGeneration;

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

			string nPattern = CellPlacementConfiguration.RotatePattern90Degrees(cfg.Pattern, cfg.RowWidth); 
			Assert.AreEqual(nPattern, "000010111");

			nPattern = CellPlacementConfiguration.RotatePattern90Degrees(nPattern, cfg.RowWidth); 
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

		[Test]
		public void Test_IsPositionValid()
		{
			CellPlacementConfiguration cfg = new CellPlacementConfiguration();
			cfg.Pattern 	  = "010101010";
			cfg.AllowRotation = false;
			cfg.RowWidth      = 3;

			Level l = new Level(new Vector2Int(3,3));
			l.SetCell(1, 0, LevelGeneration.ECellCode.Hall);
			l.SetCell(0, 1, LevelGeneration.ECellCode.Hall);
			l.SetCell(2, 1, LevelGeneration.ECellCode.Hall);
			l.SetCell(1, 2, LevelGeneration.ECellCode.Hall);

			bool isValid = cfg.IsPosValid(l, new Vector2Int(1,1), ELevelLayer.All, ECellCode.Hall);
			Assert.IsTrue(isValid);
		}

		[Test]
		public void Test_IsPositionValid_Invalid()
		{
			CellPlacementConfiguration cfg = new CellPlacementConfiguration();
			cfg.Pattern 	  = "000000000";
			cfg.AllowRotation = false;
			cfg.RowWidth      = 3;

			Level l = new Level(new Vector2Int(3,3));
			l.SetCell(1, 0, LevelGeneration.ECellCode.Hall);
			l.SetCell(0, 1, LevelGeneration.ECellCode.Hall);
			l.SetCell(2, 1, LevelGeneration.ECellCode.Hall);
			l.SetCell(1, 2, LevelGeneration.ECellCode.Hall);

			bool isValid = cfg.IsPosValid(l, new Vector2Int(1,1), ELevelLayer.All, ECellCode.Hall);
			Assert.IsFalse(isValid);
		}

		[Test]
		public void Test_IsPositionValidRotation()
		{
			CellPlacementConfiguration cfg = new CellPlacementConfiguration();
			cfg.Pattern 	  = "111000000";
			cfg.AllowRotation = true;
			cfg.RowWidth      = 3;

			Level l = new Level(new Vector2Int(3,3));
			l.SetCell(0, 2, LevelGeneration.ECellCode.Hall);
			l.SetCell(1, 2, LevelGeneration.ECellCode.Hall);
			l.SetCell(2, 2, LevelGeneration.ECellCode.Hall);

			bool isValid = cfg.IsPosValid(l, new Vector2Int(1,1), ELevelLayer.All, ECellCode.Hall);
			Assert.IsTrue(isValid);
		}
    }
}
