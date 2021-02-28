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
        Spawns the level's base geometry: Floors, Walls and Doors.
    */
    public class LevelGenerationMeshStepGeometry : ILevelGenerationMeshStep
    {
        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            Action<Utils.SectorCellIteration> Iterator = delegate (Utils.SectorCellIteration it)
            {
                var roomType = level.GetCell(it.cellPosition, ELevelLayer.Rooms);
                var roomCfg = cfg.GetRoomConfig(roomType);

                bool floor = it.cell != LevelGeneration.ECellCode.Empty;
                if (floor)
                {
                    Utils.PutFloor(new Utils.PutFloorParams
                    {
                        sector = it.sector,
                        floorPrefab = roomCfg.Floors[Random.Range(0, roomCfg.Floors.Length)],
                        floorMaterial = roomCfg.EnvironmentMaterial,
                        cellSize = roomCfg.CellSize(),
                        floorRoot = root,
                        position = it.cellPosition,
                        rotation = new Vector3(0f, Random.Range(0, 4)*90, 0f)
                    });
                }

                Func<Utils.CheckNeighborsComparerParams, bool> WallCheck = delegate(Utils.CheckNeighborsComparerParams p)
                {
                    bool differentSectors = level.GetSectorAt(p.originalPosition) != level.GetSectorAt(p.neighborPosition);
                    bool differentTypes = p.originalCell != p.neighborCell;

                    bool isOriginDoor = p.sector.GetCell(p.originalPosition, ELevelLayer.Doors) == LevelGeneration.ECellCode.Door;
                    bool isNeighborDoor = p.sector.GetCell(p.neighborPosition, ELevelLayer.Doors) == LevelGeneration.ECellCode.Door;

                    bool isDoor = isOriginDoor && isNeighborDoor && differentSectors;

                    if ((differentTypes || differentSectors) && !isDoor)
                    {
                        /*
                        Utils.PutWall(new Utils.PutWallParams
                        {
                            sector          = it.sector,
                            root            = root,
                            cfg             = roomCfg,
                            directions      = p.direction,
                            prefab          = roomCfg.Walls[0],
                            cellSize        = roomCfg.CellSize(),
                            namePreffix     = "W",
                            position        = it.cellPosition,
                            material        = roomCfg.EnvironmentMaterial,
                            shouldCollide   = true
                        });
                        */
                    }
                    else if (isDoor)
                    {
                        Utils.PutWall(new Utils.PutWallParams
                        {
                            sector          = it.sector,
                            root            = root,
                            cfg             = roomCfg,
                            directions      = p.direction,
                            prefab          = roomCfg.Doors[0],
                            cellSize        = roomCfg.CellSize(),
                            namePreffix     = "D",
                            position        = it.cellPosition,
                            material        = roomCfg.EnvironmentMaterial,
                            shouldCollide   = true,
                            addNavMeshObstacle = false
                        });
                    }

                    return (differentTypes || differentSectors) && !isDoor;
                };
                Utils.CheckNeighbors(it.sector, 
                                     it.cellPosition, 
                                     WallCheck, 
                                     ELevelLayer.Hall | ELevelLayer.Rooms);
            };

            Utils.IterateSector(level.BaseSector, Iterator, ELevelLayer.All);
            yield return null;
        }

    }
}