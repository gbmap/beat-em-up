using UnityEngine;
using System;
using Catacumba.Data;
using System.Collections.Generic;

namespace Catacumba.LevelGen
{
    public class Room
    {
        public abstract class RoomRequirementBase
        {

        }


        public Sector Sector { get; private set; }
        public Vector2Int Position { get { return Sector.Pos; } }
        public Vector2Int Size { get { return Sector.Size; } }
        public LevelGeneration.ECellCode Type { get { return Sector.Code; } }

        public List<GameObject> Props;
        public List<GameObject> Characters;
        public RoomRequirementBase RequirementToEnter;
        public int NumberOfDoors;

        public Room(Sector sec)
        {
            this.Sector = sec;
        }
    }

}