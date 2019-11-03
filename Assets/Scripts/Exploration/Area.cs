using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace Catacumba.Exploration
{
    public class Area : MonoBehaviour
    {
        [SerializeField] Transform playerSpawnPoint;
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
            
        }
    }
}