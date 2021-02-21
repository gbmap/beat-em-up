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
        Generates meshes for areas between rooms. Iterates over ECellCode.Hall
        and instantiates floors and walls accordingly.
    */
    public class LevelGenerationMeshStepHall : ILevelGenerationMeshStep
    {
        private GameObject floorRoot;
        private GameObject wallRoot;

        public LevelGenerationMeshStepHall(GameObject floorRoot, GameObject wallRoot)
        {
            this.floorRoot = floorRoot;
            this.wallRoot = wallRoot;
        }

        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            GenerateHall(level, cfg, this.floorRoot, this.wallRoot);
            yield return null;
        }

        private static void GenerateHall(Level l,
                                         BiomeConfiguration cfg,
                                         GameObject floorRoot,
                                         GameObject wallRoot)
        {
            var hallCfg = cfg.GetRoomConfig(LevelGeneration.ECellCode.Hall);
            Vector3 cellSize = hallCfg.Floors[0].GetComponent<Renderer>().bounds.size;

            Func<Utils.CheckNeighborsComparerParams, bool> comparer = delegate(Utils.CheckNeighborsComparerParams p)
            {
                bool put = p.neighborCell > LevelGeneration.ECellCode.Empty &&
                           p.neighborCell != LevelGeneration.ECellCode.Hall;
                return put;
            };

            Action<Utils.SectorCellIteration> hallStep = delegate(Utils.SectorCellIteration param)
            {
                if (param.cell == LevelGeneration.ECellCode.Hall ||
                    param.cell == LevelGeneration.ECellCode.Prop ||
                    param.cell == LevelGeneration.ECellCode.Enemy)
                {

                    Utils.PutFloor(new Utils.PutFloorParams
                    {
                        sector        = param.sector,
                        position      = param.cellPosition,
                        cellSize      = cfg.CellSize(),
                        floorPrefab   = hallCfg.Floors[0],
                        floorMaterial = hallCfg.EnvironmentMaterial,
                        floorRoot     = floorRoot
                    });

                    Utils.CheckOneSidedWalls(param.sector.Level.BaseSector, 
                                             hallCfg, 
                                             cellSize, 
                                             wallRoot, 
                                             param.cellPosition, 
                                             hallCfg.EnvironmentMaterial);
                    
                    
                    Utils.CheckTwoSidedWalls(param.sector.Level.BaseSector, 
                                             hallCfg, 
                                             cellSize, 
                                             wallRoot, 
                                             param.cellPosition, 
                                             ELevelLayer.Hall | ELevelLayer.Rooms,
                                             //param.layer,
                                             comparer, hallCfg.EnvironmentMaterial);
                }
            };

            Utils.IterateSector(l.BaseSector, new Action<Utils.SectorCellIteration>[] { hallStep }, ELevelLayer.All &~ ELevelLayer.Doors);
        }
    }
}