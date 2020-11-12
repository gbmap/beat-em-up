using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Catacumba.LevelGen
{
    using CellCode = LevelGeneration.ECellCode;

    [Flags]
    public enum ELevelLayer
    {
        Hall = 1,
        Rooms = 1 << 1,
        Doors = 1 << 2,
        Props = 1 << 3,
        Enemies = 1 << 4,

        All = Hall | Rooms | Doors | Props | Enemies
    }

    public abstract class LayeredBitmap<TLayer, TValue>
    {
        private Dictionary<TLayer, TValue[,]> Map;

        public Vector2Int Size { get; private set; }
        public abstract TLayer AllLayers { get; }

        public abstract TValue CellEmpty { get; }
        public abstract TValue CellError { get; }

        public LayeredBitmap(Vector2Int size)
        {
            this.Map = new Dictionary<TLayer, TValue[,]>();
            this.Size = size;

            TLayer[] layers = GetLayerValues();
            foreach (var layer in layers)
                this.Map[layer] = new TValue[size.x, size.y];
        }

        public void SetCell(int x, int y, TValue value, TLayer layer, bool overwrite=false)
        {
            if (layer.Equals(AllLayers))
                layer = GetLayerFromValue(value);

            TValue[,] layerMap = this.Map[layer];
            if (overwrite)
                layerMap[x, y] = value;
            else
                layerMap[x,y] = this.Max(layerMap[x, y], value);
        }

        public TValue GetCell(int x, int y)
        {
            return GetCell(x, y, AllLayers);
        }

        public TValue GetCell(int x, int y, TLayer layerBitmask)
        {
            try {
                // var values = Enum.GetValues(typeof(ELevelLayer)).Cast<ELevelLayer>().Reverse().Skip(1);
                var layers = GetLayerValues().Reverse().Skip(1);

                foreach (var layer in layers)
                {
                    if (!IsLayerSet(layerBitmask, layer))
                    //if (!BitmaskHelper.IsSet<TLayer>(layerBitmask, layer))
                        continue;

                    var cell = this.Map[layer][x,y];
                    if (IsGreater(cell, CellEmpty))
                        return cell;
                }
                return CellEmpty;
            }

            catch (IndexOutOfRangeException)
            {
                return CellError;
            }
        }

        public abstract TLayer GetLayerFromValue(TValue value);
        public abstract TLayer[] GetLayerValues();
        public abstract TValue Max(TValue a, TValue b);
        public abstract bool IsGreater(TValue a, TValue b);
        public abstract bool IsLayerSet(TLayer layerBitmask, TLayer layer);
    }

    public class LevelBitmap : LayeredBitmap<ELevelLayer, CellCode>
    {
        public LevelBitmap(Vector2Int size) : base(size) {}

        public override ELevelLayer AllLayers => ELevelLayer.All;

        public override CellCode CellEmpty => LevelGeneration.ECellCode.Empty;

        public override CellCode CellError => LevelGeneration.ECellCode.Error;

        private Dictionary<LevelGeneration.ECellCode, ELevelLayer> dictCellToLayer = new Dictionary<CellCode, ELevelLayer>
        {
            { LevelGeneration.ECellCode.Error,             ELevelLayer.Hall    },
            { LevelGeneration.ECellCode.Empty,             ELevelLayer.Hall    },
            { LevelGeneration.ECellCode.Hall,              ELevelLayer.Hall    },
            { LevelGeneration.ECellCode.Room,              ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.BossRoom,          ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.PlayerSpawn,       ELevelLayer.Hall    },
            { LevelGeneration.ECellCode.Enemy,             ELevelLayer.Enemies },
            { LevelGeneration.ECellCode.Prop,              ELevelLayer.Props   },
            { LevelGeneration.ECellCode.RoomBloodOath,     ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.RoomChase,         ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.RoomDice,          ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.RoomEnemies,       ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.RoomItem,          ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.RoomKillChallenge, ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.RoomPrison,        ELevelLayer.Rooms   },
            { LevelGeneration.ECellCode.Door,              ELevelLayer.Doors   }
        };

        public override ELevelLayer GetLayerFromValue(CellCode value)
        {
            return dictCellToLayer[value];
        }

        public override ELevelLayer[] GetLayerValues()
        {
            return Enum.GetValues(typeof(ELevelLayer)).Cast<ELevelLayer>().ToArray();
        }

        public override bool IsGreater(CellCode a, CellCode b)
        {
            return a > b;
        }

        public override bool IsLayerSet(ELevelLayer layerBitmask, ELevelLayer layer)
        {
            return BitmaskHelper.IsSet<ELevelLayer>(layerBitmask, layer);
        }

        public override CellCode Max(CellCode a, CellCode b)
        {
            return (CellCode)Mathf.Max((int)a, (int)b);
        }
    }
    /*
        // TODO: Abstract class LayeredBitmap<layer, value>
        public class LevelBitmap
        {
            private Dictionary<ELevelLayer, LevelGeneration.ECellCode[,]> Map;
            public Vector2Int Size {get; private set;}

            private Dictionary<LevelGeneration.ECellCode, ELevelLayer> dictCellToLayer = new Dictionary<CellCode, ELevelLayer>
            {
                { LevelGeneration.ECellCode.Error,             ELevelLayer.Hall    },
                { LevelGeneration.ECellCode.Empty,             ELevelLayer.Hall    },
                { LevelGeneration.ECellCode.Hall,              ELevelLayer.Hall    },
                { LevelGeneration.ECellCode.Room,              ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.BossRoom,          ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.PlayerSpawn,       ELevelLayer.Hall    },
                { LevelGeneration.ECellCode.Enemy,             ELevelLayer.Enemies },
                { LevelGeneration.ECellCode.Prop,              ELevelLayer.Props   },
                { LevelGeneration.ECellCode.RoomBloodOath,     ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.RoomChase,         ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.RoomDice,          ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.RoomEnemies,       ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.RoomItem,          ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.RoomKillChallenge, ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.RoomPrison,        ELevelLayer.Rooms   },
                { LevelGeneration.ECellCode.Door,              ELevelLayer.Doors   }
            };

            // DONE
            public LevelBitmap(Vector2Int size)
            {
                this.Map = new Dictionary<ELevelLayer, CellCode[,]>();
                this.Size = size;

                var values = Enum.GetValues(typeof(ELevelLayer)).Cast<ELevelLayer>();
                foreach (var v in values)
                    this.Map[v] = new CellCode[size.x, size.y];
            }

            public void SetCell(int x, int y,
                                LevelGeneration.ECellCode value,
                                ELevelLayer layer, 
                                bool overwrite = false)
            {
                SetCell(new Vector2Int(x, y), value, layer, overwrite);
            }

            public void SetCell(Vector2Int p, 
                                LevelGeneration.ECellCode value, 
                                ELevelLayer layer, 
                                bool overwrite = false)
            {
                if (layer == ELevelLayer.All)
                    layer = this.dictCellToLayer[value];

                CellCode[,] layerMap = Map[layer];
                if (overwrite)
                    layerMap[p.x, p.y] = value; 
                else 
                    layerMap[p.x, p.y] = (CellCode)Mathf.Max((int)value, (int)layerMap[p.x,p.y]); 
            }

           public LevelGeneration.ECellCode GetCell(Vector2Int p, ELevelLayer layer = ELevelLayer.All)
            {
                return GetCellFromLayerBitmask(p.x, p.y, layer);
            }

            public LevelGeneration.ECellCode GetCell(int x, int y, ELevelLayer layer = ELevelLayer.All)
            {
                return GetCellFromLayerBitmask(x, y, layer);
            }

            public LevelGeneration.ECellCode GetCellFromLayerBitmask(int x, int y, ELevelLayer bitmask)
            {
                try {
                    var values = Enum.GetValues(typeof(ELevelLayer)).Cast<ELevelLayer>().Reverse().Skip(1);

                    foreach (var v in values)
                    {
                        if (!BitmaskHelper.IsSet<ELevelLayer>(bitmask, v))
                            continue;

                        var cell = this.Map[v][x,y];
                        if (cell > LevelGeneration.ECellCode.Empty)
                            return cell;
                    }
                    return LevelGeneration.ECellCode.Empty;
                }
                catch (IndexOutOfRangeException ex)
                {
                    return LevelGeneration.ECellCode.Error;
                }
            }

            private static ELevelLayer GetAllLayers()
            {
                ELevelLayer layer = 0;
                var values = Enum.GetValues(typeof(ELevelLayer)).Cast<ELevelLayer>();
                foreach (var v in values)
                {
                    BitmaskHelper.Set<ELevelLayer>(ref layer, v);
                }
                return layer;
            }
        }

        */

    public class Level
    {
        public Vector2Int Size
        {
            get
            {
                return this.map.Size;
                //return new Vector2Int(Map.GetLength(0), Map.GetLength(1));
            }
        }
        //private LevelGeneration.ECellCode[,] Map;

        private LevelBitmap map;

        public Vector2Int SpawnPoint;
        public Sector SpawnSector;

        public Sector BaseSector { get; private set; }

        public Level(Vector2Int size)
        {
            // Map = new LevelGeneration.ECellCode[size.x, size.y];
            this.map = new LevelBitmap(size);
            BaseSector = new Sector(this, Vector2Int.zero, size, LevelGeneration.CODE_EMPTY);
        }

        public void SetCell(Vector2Int p, LevelGeneration.ECellCode v, ELevelLayer layer = ELevelLayer.All, bool overwrite = false)
        {
            map.SetCell(p.x, p.y, v, layer, overwrite);
        }

        public void SetCell(int x, int y, LevelGeneration.ECellCode v, ELevelLayer layer = ELevelLayer.All, bool overwrite = false)
        {
            map.SetCell(x, y, v, layer, overwrite);
        }

        public LevelGeneration.ECellCode GetCell(Vector2Int p, ELevelLayer layer = ELevelLayer.All)
        {
            return map.GetCell(p.x, p.y, layer);
        }

        public LevelGeneration.ECellCode GetCell(int x, int y, ELevelLayer layer = ELevelLayer.All)
        {
            return map.GetCell(x, y, layer);
        }

        public Sector GetSectorAt(Vector2Int position)
        {
            foreach (var sec in BaseSector.Children)
            {
                if (sec.IsInFromGlobal(position))
                    return sec;
            }
            return BaseSector;
        }

        public Vector2Int WorldPositionToLevelPosition(Vector3 position, Vector3 cellSize) {
            return new Vector2Int(Mathf.CeilToInt((position.x)/cellSize.x), Mathf.FloorToInt(position.z/cellSize.z));
        }
    }

}