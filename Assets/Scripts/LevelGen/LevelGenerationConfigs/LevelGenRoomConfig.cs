using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.LevelGen {
    public class LevelGenRoomConfig : ScriptableObject
    {
        public Material EnvironmentMaterial;

        [Header("Floors")]
        public GameObject[] Floors;

        [Header("Walls")]
        public GameObject[] Walls;

        [Header("Doors")]
        public GameObject[] DoorWalls;
        public GameObject[] DoorFrame;
        public GameObject[] Door;
        public RuntimeAnimatorController DoorAnimator;

        [Header("Props")]
        public GameObject[] Props;

        public Vector3 CellSize()
        {
            return Floors[0].GetComponent<Renderer>().bounds.size;
        }
    }
}