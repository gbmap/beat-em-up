using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.Data.Level {
    public class RoomConfiguration : ScriptableObject
    {
        public Material EnvironmentMaterial;

        [Header("Floors")]
        public GameObject[] Floors;

        [Header("Walls")]
        public GameObject[] Walls;

        [Header("Doors")]
        public GameObject[] Doors;

        [Header("Prop Pool")]
        public CharacterPool PropPool;

        public Vector3 CellSize()
        {
            return Floors[0].GetComponent<Renderer>().bounds.size;
        }
    }
}