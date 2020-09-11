using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catacumba.LevelGen;

namespace Tests
{
    public class LevelBitmapTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void Test_Creation()
        {
            // Use the Assert class to test conditions
            LevelBitmap level = new LevelBitmap(new Vector2Int(128, 128));
            for (int x =0 ; x < 128; x ++)
            {
                for (int y = 0; y < 128; y++)
                {
                    Assert.True(level.GetCell(x, y) == LevelGeneration.ECellCode.Empty);
                }
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [Test]
        public void Test_SetCell()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            LevelBitmap level = new LevelBitmap(new Vector2Int(128, 128));
            level.SetCell(50, 50, LevelGeneration.ECellCode.Prop, ELevelLayer.Hall);
            Assert.AreEqual(LevelGeneration.ECellCode.Prop, level.GetCell(50, 50));

            level.SetCell(50, 50, LevelGeneration.ECellCode.Enemy, ELevelLayer.Enemies);
            Assert.AreEqual(LevelGeneration.ECellCode.Enemy, level.GetCell(50, 50));

            level.SetCell(50, 50, LevelGeneration.ECellCode.Enemy, ELevelLayer.Enemies);
        }
    }
}
