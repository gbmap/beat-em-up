using System;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.Exploration
{
    public class ScenarioManager : Singleton<ScenarioManager>
    {
        [SerializeField] private List<GameObject> _maps;
        private int _currentActiveMap;

        private void Awake()
        {
            _maps = new List<GameObject>();
            
            // Get child maps
            foreach (Transform map in transform)
            {
                _maps.Add(map.gameObject);
            }
        }
        
        public void TransitionToMap(int mapIndex)
        {
            // Hide current map
            if (mapIndex >= 0 && _maps.Count > mapIndex)
            {
                _maps[_currentActiveMap].SetActive(false);
            }
            
            // Show new map
            _maps[_currentActiveMap = mapIndex].SetActive(true);
        }
    }
}