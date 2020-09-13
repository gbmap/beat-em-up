using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catacumba.LevelGen;
using Catacumba.LevelGen.Mesh;
using Utils = Catacumba.LevelGen.Mesh.Utils;

namespace Tests
{
    public static class LevelGenerationTestHelper
    {
        public static System.Tuple<Level, Sector> CreateTestSector()
        {
            return CreateTestSector(
                new Vector2Int(128, 128), 
                new Vector2Int(50, 50), 
                new Vector2Int(20, 20)
            );
        }

        public static System.Tuple<Level, Sector> CreateTestSector(Vector2Int levelSize,
                                                             Vector2Int sectorPosition,
                                                             Vector2Int sectorSize)
                                                             
        {
            Level level = new Level(levelSize);
            Sector sec = new Sector(level, 
                                    sectorPosition, 
                                    sectorSize, 
                                    LevelGeneration.ECellCode.Room); 
            return new System.Tuple<Level, Sector>(level, sec);
        }
    }

    public class LevelGenerationUtilsTests
    {
        private System.Tuple<Level, Sector> CreateTestSector()
        {
            Level level = new Level(new Vector2Int(128, 128));
            Sector sec = new Sector(level, 
                                    new Vector2Int(50, 50), 
                                    new Vector2Int(20, 20), 
                                    LevelGeneration.ECellCode.Room); 
            return new System.Tuple<Level, Sector>(level, sec);
        }

        // A Test behaves as an ordinary method
        [Test]
        public void Test_SectorIteration_Cell()
        {
            var testLevel = CreateTestSector();

            Level level = testLevel.Item1;
            Sector sec =  testLevel.Item2;           

            System.Action<Catacumba.LevelGen.Mesh.Utils.SectorCellIteration> iterator = 
                delegate(Catacumba.LevelGen.Mesh.Utils.SectorCellIteration iteration)
            {
                Assert.AreEqual(LevelGeneration.ECellCode.Room, iteration.cell);
            };
            Catacumba.LevelGen.Mesh.Utils.IterateSector(sec, iterator);
        }

        [Test]
        public void Test_SectorIteration_LocalPosition()
        {
            var testLevel = CreateTestSector();

            Level level = testLevel.Item1;
            Sector sec =  testLevel.Item2;           

            List<Vector2Int> expectedPositions       = new List<Vector2Int>();
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    Vector2Int expectedPos =new Vector2Int(x, y); 
                    expectedPositions.Add(expectedPos);                    
                }
            }

            System.Action<Catacumba.LevelGen.Mesh.Utils.SectorCellIteration> iterator = 
                delegate(Catacumba.LevelGen.Mesh.Utils.SectorCellIteration iteration)
            {
                Assert.AreEqual(expectedPositions[iteration.iterationNumber], iteration.cellPosition);
            };
            Catacumba.LevelGen.Mesh.Utils.IterateSector(sec, iterator);
        }

        [Test]
        public void Test_SectorIteration_GlobalPosition()
        {
            var testLevel = CreateTestSector();

            Level level = testLevel.Item1;
            Sector sec =  testLevel.Item2;           

            List<Vector2Int> expectedGlobalPositions = new List<Vector2Int>();
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    Vector2Int expectedPos =new Vector2Int(x, y); 
                    expectedGlobalPositions.Add(expectedPos + sec.Pos);
                }
            }

            System.Action<Catacumba.LevelGen.Mesh.Utils.SectorCellIteration> iterator = 
                delegate(Catacumba.LevelGen.Mesh.Utils.SectorCellIteration iteration)
            {
                Assert.AreEqual(expectedGlobalPositions[iteration.iterationNumber], iteration.sector.GetAbsolutePosition(iteration.cellPosition));
            };
            Catacumba.LevelGen.Mesh.Utils.IterateSector(sec, iterator);
        }

        [Test]
        public void Test_SectorIteration_Cell_OutsideCells()
        {
            var testLevel = CreateTestSector();
            Level l = testLevel.Item1;
            Sector s = testLevel.Item2;

            l.BaseSector.FillSector(LevelGeneration.ECellCode.Hall);
            s.FillSector(LevelGeneration.ECellCode.Room);
            Assert.AreEqual(LevelGeneration.ECellCode.Room, s.GetCell(0, 0));
            Assert.AreEqual(LevelGeneration.ECellCode.Hall, l.GetCell(0,0));

            System.Func<Utils.CheckNeighborsComparerParams, bool> checkNeighbors = delegate(Utils.CheckNeighborsComparerParams p)
            {
                bool isOutside = !p.sector.IsIn(p.neighborPosition);
                var outsideCell = p.sector.Level.GetCell(p.sector.GetAbsolutePosition(p.neighborPosition));
                if (isOutside)
                    Assert.AreEqual(LevelGeneration.ECellCode.Hall, outsideCell);
                return true;
            };
             
            Utils.IterateSector(s, delegate(Utils.SectorCellIteration i)
            {
                Catacumba.LevelGen.Mesh.Utils.CheckNeighbors(i.sector, i.cellPosition, checkNeighbors);
            });

            /*
            System.Func<Utils.CheckNeighborsComparerParams, bool> checkNeighborsGlobal = delegate(Utils.CheckNeighborsComparerParams p)
            {
                bool isOutside = !p.sector.IsIn(p.neighborPosition);
                if (isOutside)
                    Assert.AreEqual(LevelGeneration.ECellCode.Hall, outsideCell);
                return true;
            };

            Utils.IterateSector(s, delegate(Utils.SectorCellIteration i)
            {
                Catacumba.LevelGen.Mesh.Utils.CheckNeighbors(i.sector, i.cellPosition, checkNeighbors);
            });
            */
        }

    }
}
