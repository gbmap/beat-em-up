using UnityEngine;

public class LevelGenBiomeConfig : ScriptableObject
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

    public Vector3 CellSize()
    {
        return Floors[0].GetComponent<Renderer>().bounds.size;
    }
}