using Catacumba.LevelGen;
using QFSW.QC;
using UnityEngine;

namespace Catacumba.Data.Level {

    public class QCBiomeParser : BasicQcParser<BiomeConfiguration>
    {
        public override BiomeConfiguration Parse(string value)
        {
            return LevelGenerationManager.LoadBiome(value);
        }
    }

    public class  BiomeConfiguration : ScriptableObject
    {
        // código burro pq eu sou burro
        public RoomConfiguration Hall;
        public RoomConfiguration RoomItem;
        public RoomConfiguration RoomPrison;
        public RoomConfiguration RoomEnemies;
        public RoomConfiguration RoomDice;
        public RoomConfiguration RoomBloodOath;
        public RoomConfiguration RoomKillChallenge;
        public RoomConfiguration RoomChase; 

        public BiomeTrapConfiguration TrapConfiguration;

        public RoomConfiguration GetRoomConfig(LevelGeneration.ECellCode cellCode)
        {
            switch (cellCode)
            {
                case LevelGeneration.ECellCode.Hall:              return Hall;
                case LevelGeneration.ECellCode.RoomItem:          return RoomItem;
                case LevelGeneration.ECellCode.RoomPrison:        return RoomPrison;
                case LevelGeneration.ECellCode.RoomEnemies:       return RoomEnemies;
                case LevelGeneration.ECellCode.RoomDice:          return RoomDice;
                case LevelGeneration.ECellCode.RoomBloodOath:     return RoomBloodOath;
                case LevelGeneration.ECellCode.RoomKillChallenge: return RoomKillChallenge;
                case LevelGeneration.ECellCode.RoomChase:         return RoomChase;
                default:                                          return Hall;
            }
        }

        public Vector3 CellSize()
        {
            return Hall.Floors[0].GetComponent<Renderer>().bounds.size;
        }
    }
}