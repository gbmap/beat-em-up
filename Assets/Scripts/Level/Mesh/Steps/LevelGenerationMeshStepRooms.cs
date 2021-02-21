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
        Instantiates the meshes for every room in the level provided,
        using a specific BiomeConfiguration.
    */
    public class LevelGenerationMeshStepRooms : ILevelGenerationMeshStep
    {
        IEnumerator ILevelGenerationMeshStep.Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            foreach (Sector sec in level.BaseSector.Children)
            {
                GenerateRoom(sec, cfg.GetRoomConfig(sec.Code), root);
            }
            yield return null;
        }

        void GenerateRoom(Sector sec, RoomConfiguration cfg, GameObject root)
        {
            string log = "GENERATING ROOM:\n";
            log += "Position: " + sec.Pos + "\n";
            log += "Size: " + sec.Size + "\n";
            Debug.Log(log);

            Vector3 cellSize = cfg.Floors[0].GetComponent<Renderer>().bounds.size;
            cellSize.y = 0f;

            GameObject roomObject = new GameObject("Room");
            roomObject.transform.SetParent(root.transform);
            roomObject.transform.localPosition = Vector3.zero;
            roomObject.transform.localPosition = Utils.LevelToWorldPos(sec.Pos, cellSize);

            Material roomMaterial = new Material(cfg.EnvironmentMaterial);

            Action<Utils.SectorCellIteration> ac = delegate(Utils.SectorCellIteration param) {
                int x = param.cellPosition.x;
                int y = param.cellPosition.y;
                Vector2Int p = param.cellPosition;
                Vector2Int pos = param.sector.Pos;
                Vector2Int sz = param.sector.Size;

                var obj = Utils.PutFloor(new Utils.PutFloorParams
                {
                    sector        = sec,
                    cellSize      = cellSize,
                    floorMaterial = roomMaterial,
                    floorPrefab   = cfg.Floors[0],
                    floorRoot     = roomObject,
                    position      = p
                });

                // Borders
                if (x == 0 || x == sz.x-1 ||
                    y == 0 || y == sz.y-1)
                {
                    Utils.CheckOneSidedWalls(sec, cfg, cellSize, roomObject, p, roomMaterial);
                    Utils.CheckTwoSidedWalls(sec, cfg, cellSize, roomObject, p, material: roomMaterial);
                }
            };

            Utils.IterateSector(sec, new Action<Utils.SectorCellIteration>[] { ac });
        }
    }
}