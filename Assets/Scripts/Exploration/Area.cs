using UnityEngine;

namespace Catacumba.Exploration
{
    public class Area : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;

        public Transform PlayerSpawnPoint => playerSpawnPoint;
    }
}