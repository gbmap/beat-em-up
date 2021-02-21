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
        Spawns props on ECellCode.Prop cells.
    */
    public class LevelGenerationMeshStepProps : ILevelGenerationMeshStep
    {
        private class PropPlacementBufferItem
        {
            public List<GameObject> PropInstances = new List<GameObject>();
            public GameObject FloorObject;
            public EDirectionBitmask Directions;
        }

        public LevelGenerationMeshStepProps()
        {
        }

        public IEnumerator Run(BiomeConfiguration cfg, Level level, GameObject root)
        {
            List<PropPlacementBufferItem> bufferItems = new List<PropPlacementBufferItem>();

            Mesh.Utils.IterateSector(level.BaseSector, (it) =>
            {
                if (it.cell != LevelGeneration.ECellCode.Prop) return;

                RoomConfiguration roomCfg = cfg.GetRoomConfig(level.GetCell(it.cellPosition, ELevelLayer.Hall | ELevelLayer.Rooms));
                
                GameObject floorObject = GameObject.Find($"F_{it.cellPosition.x}_{it.cellPosition.y}");
                if (floorObject == null) return;

                EDirectionBitmask differentNeighbors = GetDifferentNeighbors(it.sector, it.cellPosition);
                List<GameObject>  propInstances      = new List<GameObject>();
                for (int i = 0; i < 3; i++)
                {
                    CharacterPoolItem propCfg = roomCfg.PropPool.GetRandom();
                    propInstances.Add(CharacterManager.SpawnProp(propCfg.Config, Vector2Int.zero));
                }

                bufferItems.Add(new PropPlacementBufferItem
                {
                    PropInstances = propInstances,
                    FloorObject   = floorObject,
                    Directions    = differentNeighbors
                });
            }, ELevelLayer.Props);
            yield return new WaitForSeconds(0.25f);

            foreach (PropPlacementBufferItem item in bufferItems)
                PropPlacement.OrganizeProps(item.FloorObject, item.Directions, item.PropInstances.ToArray());
        }

        private EDirectionBitmask GetDifferentNeighbors(Sector sector, Vector2Int cellPosition)
        {
            ECellCode cell = sector.GetCell(cellPosition);
            EDirectionBitmask differentCells = EDirectionBitmask.None;
            System.Func<Utils.CheckNeighborsComparerParams, bool> checker = delegate(Utils.CheckNeighborsComparerParams arg) 
            { 
                return CheckNeighbors(arg, ref differentCells);
            };
            Utils.CheckNeighbors(sector, cellPosition, checker, ELevelLayer.Hall | ELevelLayer.Rooms); 
            return differentCells;
        }

        private bool CheckNeighbors(Utils.CheckNeighborsComparerParams arg, ref EDirectionBitmask directions)
        {
            bool isDoor = arg.sector.GetCell(arg.neighborPosition, ELevelLayer.Doors) > ECellCode.Empty;
            if (arg.neighborCell != arg.originalCell && !isDoor)
                DirectionHelper.Set(ref directions, arg.direction);
            return true;
        }
    }
}