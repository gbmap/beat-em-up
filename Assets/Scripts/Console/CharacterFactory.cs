using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using UnityEngine.AI;
using Catacumba.Data;
using Catacumba.Data.Controllers;
using Catacumba.Data.Items;

namespace Catacumba.Entity
{
    public static class CharacterFactory
    {
        [Command("spawn_enemy")]
        public static void Command_CreateEnemy(string characterConfiguration)
        {
            CharacterData data = CreateEntityInstance(characterConfiguration, "Enemy", "Entities");
            ControllerComponent component = data.gameObject.AddComponent<ControllerComponent>();
            component.Controller = Resources.Load<ControllerAI>("Data/Controllers/ControllerAI");
        }

        [Command("spawn_player")]
        public static void Command_CreatePlayer(string characterConfiguration)
        {
            CharacterData data = CreateEntityInstance(characterConfiguration, "Player", "Player");
            ControllerComponent component = data.gameObject.AddComponent<ControllerComponent>();
            component.Controller = Resources.Load<ControllerInput>("Data/Controllers/ControllerInputPlayer1");
        }

        private static CharacterData CreateEntityInstance(string characterConfiguration, string tag, string layer)
        {
            string path = $"{CharacterConfiguration.DEFAULT_FOLDER}/{characterConfiguration}";
            CharacterConfiguration configuration = Resources.Load<CharacterConfiguration>(path);
            if (!configuration)
            { 
                QuantumConsole.Instance.LogToConsole($"Couldn't load character: {characterConfiguration}");
                return null;
            }

            GameObject instance = new GameObject(characterConfiguration);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.layer                   = LayerMask.NameToLayer(layer);
            instance.tag                     = tag;

            CharacterData data = instance.AddComponent<CharacterData>();
            configuration.Configure(data);

            return data;
        }
    }
}