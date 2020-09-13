using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.LevelGen
{
    class LevelGenAlgoTest : ILevelGenAlgo
    {
        public IEnumerator Run(Level level, System.Action<Level> updateVis = null)
        {
            Sector s = new Sector(
                level, 
                new Vector2Int(10, 10), 
                new Vector2Int(10, 10), 
                LevelGeneration.ECellCode.Room
            );

            for (int i = 11; i <= 19; i++)
                level.SetCell(new Vector2Int(i, 20), LevelGeneration.ECellCode.Hall);

            for (int i = 20; i < 24; i++)
                level.SetCell(new Vector2Int(15, i), LevelGeneration.ECellCode.Hall);

            Sector s2 = new Sector(
                level, 
                new Vector2Int(20, 15), 
                new Vector2Int(10, 10), 
                LevelGeneration.ECellCode.RoomChase
            );

            Sector s3 = new Sector(
                level,
                new Vector2Int(13, 24),
                new Vector2Int(5, 5),
                LevelGeneration.ECellCode.RoomDice
            );

            yield return null;
        }

    }
}