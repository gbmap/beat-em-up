using UnityEngine;

namespace Catacumba.LevelGen {
    public class  LevelGenBiomeConfig : ScriptableObject
    {
        // código burro pq eu sou burro
        public LevelGenRoomConfig Hall;
        public LevelGenRoomConfig RoomItem;
        public LevelGenRoomConfig RoomPrison;
        public LevelGenRoomConfig RoomEnemies;
        public LevelGenRoomConfig RoomDice;
        public LevelGenRoomConfig RoomBloodOath;
        public LevelGenRoomConfig RoomKillChallenge;
        public LevelGenRoomConfig RoomChase; 

        public LevelGenRoomConfig GetRoomConfig(LevelGeneration.ECellCode cellCode)
        {
            switch (cellCode)
            {
                case LevelGeneration.ECellCode.Hall: return Hall;
                case LevelGeneration.ECellCode.RoomItem:    return RoomItem;
                case LevelGeneration.ECellCode.RoomPrison:  return RoomPrison;
                case LevelGeneration.ECellCode.RoomEnemies: return RoomEnemies;
                case LevelGeneration.ECellCode.RoomDice: return RoomDice;
                case LevelGeneration.ECellCode.RoomBloodOath: return RoomBloodOath;
                case LevelGeneration.ECellCode.RoomKillChallenge: return RoomKillChallenge;
                case LevelGeneration.ECellCode.RoomChase: return RoomChase;
                default: return Hall;
            }
        }

        public Vector3 CellSize()
        {
            return Hall.Floors[0].GetComponent<Renderer>().bounds.size;
        }
    }
}