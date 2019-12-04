using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Exploration
{
    public class Area : MonoBehaviour
    {
        [SerializeField] Transform playerSpawnPoint;
        [SerializeField] Vector3 cameraPathOffset;
        [SerializeField] Transform firstArea;
        [SerializeField] Transform lastArea;
        
        CinemachineVirtualCamera virtualCamera;
        CinemachinePath cinemachinePath;
        NavMeshSurface navMeshSurface;

        public Transform PlayerSpawnPoint => playerSpawnPoint;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;

        public void SetCharacter(GameObject character)
        {
            virtualCamera.Follow = character.transform;
        }

        void Awake()
        {
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
            cinemachinePath = GetComponentInChildren<CinemachinePath>();
            navMeshSurface = GetComponent<NavMeshSurface>();
            
//            virtualCamera.gameObject.SetActive(false);

            // Build navmesh at runtime
//            navMeshSurface.BuildNavMesh();

            GenerateCameraPath();
        }

        void GenerateCameraPath()
        {
            var firstWaypoint = new CinemachinePath.Waypoint();
            var lastWaypoint = new CinemachinePath.Waypoint();

            firstWaypoint.position = firstArea.position + cameraPathOffset;
            lastWaypoint.position = lastArea.position + cameraPathOffset;
            
            cinemachinePath.m_Waypoints = new [] {firstWaypoint, lastWaypoint};
        }
    }
}