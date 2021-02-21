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
        Verifies every cell in the level and, given a BiomeTrapConfiguration,
        checks if that cell is eligible for placement of any trap.

        If it is, rolls a dice and decides whether to spawn it or not.
    */
    public class LevelGenerationMeshStepTraps : ILevelGenerationMeshStep
    {
        BiomeTrapConfiguration _config;
        float _trapChance;

        public LevelGenerationMeshStepTraps(
            BiomeTrapConfiguration config, 
            float trapChance = 1.0f
        ) {
            _config = config;
            _trapChance = trapChance;
        }

        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            Utils.IterateSector(level.BaseSector, delegate(SectorCellIteration it)
            {
                TrapConfiguration[] traps = _config.GetAvailableTrapsAt(level, it.cellPosition, ECellCode.Hall);
                if (traps.Length > 0 && Random.value < _trapChance)
                {
                    TrapConfiguration t = traps[Random.Range(0, traps.Length)];
                    CharacterManager.SpawnTrap(t.Trap, it.cellPosition);
                }
            });

            yield return null;
        }
    }
}