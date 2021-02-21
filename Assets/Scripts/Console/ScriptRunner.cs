﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using System.Threading.Tasks;
using System.Linq;

namespace Catacumba
{
    [CommandPrefix("script.")]
    public class ScriptRunner : MonoBehaviour
    {
        public string StartingCommand = "";

        void Start() 
        {
            if (string.IsNullOrEmpty(StartingCommand))
                return;

            QuantumConsole.Instance.InvokeCommand(StartingCommand);
        }

        [Command("run_commands")]
        public static async Task Run(string[] commands)
        {
            commands = commands.Where(c => c.Length > 2)
                               .Where(c => c.Substring(0, 2) != "//")
                               .ToArray();
            await QuantumConsole.Instance.InvokeCommandsAsync(commands);
        }

        [Command("run")]
        public static async Task Run(string file)
        {
            TextAsset script = Resources.Load<TextAsset>(file);
            if (script == null) 
            {
                Debug.LogError($"Couldn't load script: {file}.");
                return;
            }

            string[] lines = script.text.Split('\n');
            await Run(lines);
        }
    }
}
