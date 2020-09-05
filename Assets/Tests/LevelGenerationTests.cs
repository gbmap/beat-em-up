using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catacumba.LevelGen;

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
                                          l.BaseSector, 
                                          LevelGeneration.ECellCode.Room);

            Assert.That(subSector.IsIn(new Vector2Int(0, 0)));
            Assert.That(subSector.IsIn(new Vector2Int(49, 49)));
            Assert.That(!subSector.IsIn(new Vector2Int(50, 50)));
        }
    }
}
