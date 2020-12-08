﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using System.Threading.Tasks;

namespace Catacumba
{
    [CommandPrefix("script.")]
    public static class ScriptRunner
    {
        [Command("run_commands")]
        public static async Task Run(string[] commands)
        {
            await QuantumConsole.Instance.InvokeCommandsAsync(commands);
        }

        [Command("run")]
        public static async Task Run(string file)
        {
            TextAsset script = Resources.Load<TextAsset>(file);
            string[] lines = script.text.Split('\n');
            await Run(lines);
        }
    }
}