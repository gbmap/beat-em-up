using System.Collections;
using UnityEngine;
using Catacumba.LevelGen.Mesh;
using System.Collections.Generic;
using System;

namespace Catacumba.LevelGen
{
    class LevelGenAlgoAddDoors : ILevelGenAlgo
    {
        private class Door
        {
            public Vector2Int From;
            public Vector2Int To;

            public int FromId;
            public int ToId;
        } 

        private class SectorConnection
        {
            public int FromId;
            public int ToId;
        }

        private class DirectionDoorPositionMap : Dictionary<EDirectionBitmask, List<Door>> { }
        private class SectorDoors : Dictionary<int, DirectionDoorPositionMap> {}

        private SectorDoors _sectorDoors;
        private List<SectorConnection> _sectorConnections = new List<SectorConnection>();

        private bool _repeatConnections;

        public LevelGenAlgoAddDoors(bool repeatConnectionsOnDifferentSides = true)
        {
            _repeatConnections = repeatConnectionsOnDifferentSides;
        }

        public IEnumerator Run(Level level, System.Action<Level> updateVis = null) 
        {
            foreach (var sec in level.BaseSector.Children)
            {
                _sectorDoors = new SectorDoors();

                Mesh.Utils.IterateSector(
                    sec, new System.Action<Mesh.Utils.SectorCellIteration>[] { SearchBorderCells }
                );

                AddDoors(level, sec, _sectorDoors);

                updateVis?.Invoke(level);
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }

        private void SearchBorderCells(Mesh.Utils.SectorCellIteration cellIteration)
        {
            Vector2Int sectorPos  = cellIteration.sector.Pos;
            Vector2Int sectorSz   = cellIteration.sector.Size;
            Sector     rootSector = cellIteration.sector.Level.BaseSector;

            if (cellIteration.cellPosition.x == 0 || 
                cellIteration.cellPosition.x == sectorSz.x - 1 ||
                cellIteration.cellPosition.y == 0 || 
                cellIteration.cellPosition.y == sectorSz.y - 1)
            {
                EDirectionBitmask directions =  Mesh.Utils.CheckNeighbors(cellIteration.sector, 
                                                                          cellIteration.cellPosition, 
                                                                          SearchOutsideCells,
                                                                          ELevelLayer.Rooms | ELevelLayer.Hall);

                foreach (EDirectionBitmask dir in DirectionHelper.GetValues())
                {
                    if (!DirectionHelper.IsSet(directions, dir))
                        continue;

                    Vector2Int outsideDoorPosition = cellIteration.sector.GetAbsolutePosition(
                        cellIteration.cellPosition + DirectionHelper.ToOffset(dir)
                    );
                }
            }
        }

        private bool SearchOutsideCells(Mesh.Utils.CheckNeighborsComparerParams p)
        {
            Vector2Int absPosition = p.sector.GetAbsolutePosition(p.neighborPosition);
            LevelGeneration.ECellCode absoluteNeighborCell = p.sector.Level.BaseSector.GetCell(absPosition, p.layer);
            Sector s = p.sector.Level.GetSectorAt(absPosition);

            bool isInSector      = p.sector.IsIn(p.neighborPosition);
            bool isNotEmpty      = absoluteNeighborCell > LevelGeneration.ECellCode.Empty;
            bool isNotHall       = absoluteNeighborCell != LevelGeneration.ECellCode.Hall;
            bool shouldPlaceDoor = !isInSector && isNotEmpty && !HasConnection(p.sector.Id, s.Id);

            EDirectionBitmask dir = p.direction; // GetDirection(p);
            if (dir == EDirectionBitmask.None)
                return false;

            if (!shouldPlaceDoor)
                return false;

            //Sector s = p.sector.Level.GetSectorAt(absPosition);
            if (!_sectorDoors.ContainsKey(s.Id))
                _sectorDoors[s.Id] = new DirectionDoorPositionMap();

            var sectorDoors = _sectorDoors[s.Id];

            if (!sectorDoors.ContainsKey(dir))
                sectorDoors.Add(dir, new List<Door>());

            Door d = new Door();
            d.From = p.sector.GetAbsolutePosition(p.originalPosition);
            d.To = absPosition;
            d.FromId = p.sector.Id;
            d.ToId = s.Id;

            sectorDoors[dir].Add(d);
            return true;
        }

        private EDirectionBitmask GetDirection(Utils.CheckNeighborsComparerParams p)
        {
            if (p.originalPosition.x == 0) return EDirectionBitmask.Left;
            if (p.originalPosition.x == p.sector.Size.x-1) return EDirectionBitmask.Right;
            if (p.originalPosition.y == 0) return EDirectionBitmask.Down;
            if (p.originalPosition.y == p.sector.Size.y-1) return EDirectionBitmask.Up;
            return EDirectionBitmask.None;
        }

        private void AddDoors(Level level, Sector sector, SectorDoors doors)
        {
            foreach (var sectorId in doors.Keys)
            {
                foreach (var direction in doors[sectorId].Keys)
                {
                    List<Door> d = doors[sectorId][direction];
                    var door = d[UnityEngine.Random.Range(0, d.Count)];

                    SectorConnection sc = new SectorConnection();
                    sc.FromId = door.FromId;
                    sc.ToId = door.ToId;
                    
                    if (!_repeatConnections)
                    {
                        if (HasConnection(sc.FromId, sc.ToId))
                            continue;
                    }

                    _sectorConnections.Add(sc);

                    level.SetCell(door.From, LevelGeneration.ECellCode.Door);
                    level.SetCell(door.To, LevelGeneration.ECellCode.Door);
                }
            }
        }

        private bool HasConnection(int from, int to)
        {
            foreach(var sectorConnection in _sectorConnections)
            {
                if (sectorConnection.FromId == from && sectorConnection.ToId == to ||
                    sectorConnection.ToId == from && sectorConnection.FromId == to)
                    return true;
            }
            return false;
        }
    }
}