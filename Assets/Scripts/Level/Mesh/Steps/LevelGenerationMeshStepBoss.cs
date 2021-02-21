using UnityEngine;
using System;
using System.Linq;
using Catacumba.Entity;
using Catacumba.Data.Level;
using Catacumba.Data;
using System.Collections.Generic;
using System.Collections;
using static Catacumba.LevelGen.LevelGeneration;
using static Catacumba.LevelGen.Mesh.Utils;
using Random = UnityEngine.Random;

namespace Catacumba.LevelGen.Mesh
{
    /*
        Spawns the provided boss in the boss room.
    */
    public class LevelGenerationMeshStepBoss : ILevelGenerationMeshStep
    {
        private CharacterConfiguration _bossConfig;

        public LevelGenerationMeshStepBoss(CharacterConfiguration bossConfiguration)
        {
            _bossConfig = bossConfiguration;
        }

        public LevelGenerationMeshStepBoss(CharacterPool possibleBosses)
        {
            _bossConfig = possibleBosses.GetRandom().Config;
        }

        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            Sector bossSector = null;
            foreach (Sector sector in level.BaseSector.Children)
            {
                if (sector.Code != ECellCode.BossRoom)
                    continue;

                bossSector = sector;
                break;
            }

            if (bossSector == null)
                yield break;

            Vector2Int center = bossSector.GetAbsolutePosition(bossSector.Size/2);
            Entity.CharacterManager.SpawnEnemy(_bossConfig, center.x, center.y);
        }
    }
}