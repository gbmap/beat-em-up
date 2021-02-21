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
        Instantiates the level's doors.
    */
    public class LevelGenerationMeshStepDoors : ILevelGenerationMeshStep
    {
        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            System.Collections.Generic.List<GameObject> doorsSpawned = new System.Collections.Generic.List<GameObject>();

            Action<Utils.SectorCellIteration> CheckDoors = delegate(Utils.SectorCellIteration iteration)
            {
                if (iteration.cell != LevelGeneration.ECellCode.Door)
                    return;

                var cell    = iteration.sector.Level.GetSectorAt(iteration.cellPosition).Code;
                var roomCfg = cfg.GetRoomConfig(cell);

                EDirectionBitmask directions = Utils.CheckNeighbors(iteration.sector,
                                                                    iteration.cellPosition, 
                                                                    SelectDoors, 
                                                                    iteration.layer);

                Utils.PutWallParams p = new Utils.PutWallParams()
                {
                    sector         = iteration.sector,
                    root           = root,
                    cfg            = roomCfg,
                    directions     = directions,
                    prefab         = roomCfg.Doors[0],
                    cellSize       = roomCfg.Floors[0].GetComponent<Renderer>().bounds.size,
                    namePreffix    = "D",
                    position       = iteration.cellPosition,
                    removeExisting = true,
                    material       = roomCfg.EnvironmentMaterial
                };
                NeighborObjects doors = Utils.PutWall(p);

                foreach (var value in doors.Values) {
                    doorsSpawned.Add(value);
                }
            };

            Utils.IterateSector(level.BaseSector, CheckDoors, ELevelLayer.Doors);

            foreach (GameObject door in doorsSpawned) {
                Vector3 pos = door.GetComponentInChildren<Renderer>().bounds.center;
                Collider[] collisions = Physics.OverlapSphere(pos, 0.1f,  1<< LayerMask.NameToLayer("Level"));

                foreach (var collider in collisions) {
                    if (collider.gameObject.name[0] != 'D') {
                        GameObject.Destroy(collider.gameObject);
                    }
                }
            }

            yield return null;
        }

        bool SelectDoors(Utils.CheckNeighborsComparerParams param)
        {
            var level = param.sector.Level;
            
            Vector2Int globalOriginalPos = param.sector.GetAbsolutePosition(param.originalPosition);
            Vector2Int globalNeighborPos = param.sector.GetAbsolutePosition(param.neighborPosition);

            bool bothDoors = param.originalCell == LevelGeneration.ECellCode.Door && 
                             param.neighborCell == LevelGeneration.ECellCode.Door;

            bool differentSectors = level.GetSectorAt(globalOriginalPos).Id != level.GetSectorAt(globalNeighborPos).Id;

            return bothDoors && differentSectors;
        }
    }
}