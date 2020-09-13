using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catacumba.LevelGen;
using Catacumba.LevelGen.Mesh;

namespace Tests
{
    public class LevelGenerationTests
    {
        [Test]
        public void Test_SectorFunction_IsIn()
        {
            Level l = new Level(new Vector2Int(256, 256));
            Sector subSector = new Sector(l, new Vector2Int(100, 100),
                                          new Vector2Int(50, 50),
                                          LevelGeneration.ECellCode.Room);

            Assert.That(subSector.IsIn(new Vector2Int(0, 0)));
            Assert.That(subSector.IsIn(new Vector2Int(49, 49)));
            Assert.That(!subSector.IsIn(new Vector2Int(50, 50)));
        }

        [Test]
        public void Test_FillSector()
        {
            var testSectorTuple = LevelGenerationTestHelper.CreateTestSector();
            Level l = testSectorTuple.Item1;
            Sector s = testSectorTuple.Item2;

            Catacumba.LevelGen.Mesh.Utils.IterateSector(s, delegate(Catacumba.LevelGen.Mesh.Utils.SectorCellIteration iteration)
            {
                Assert.AreEqual(s.Code, iteration.cell);
            });

            s.FillSector(LevelGeneration.ECellCode.Prop);

            Catacumba.LevelGen.Mesh.Utils.IterateSector(s, delegate(Catacumba.LevelGen.Mesh.Utils.SectorCellIteration iteration)
            {
                Assert.AreEqual(LevelGeneration.ECellCode.Prop, iteration.cell);
            });
        }
    }
}
