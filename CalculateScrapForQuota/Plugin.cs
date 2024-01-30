using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using CalculateScrapForQuota.Scripts;
using CalculateScrapForQuota.Utils;
using Dissonance;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements.UIR;

namespace CalculateScrapForQuota
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static Config MyConfig { get; set; }
        private static ManualLogSource _logger;
        
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} is loaded!");
            _logger = Logger;
            
            MyConfig = new(Config);
            MaterialSwapper.swapColor = MyConfig.highlightColor;
            
            Harmony harmony = new(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void Log(string m)
        {
            if (MyConfig.isVerbose)
                _logger.LogMessage(m);
        }
    }
}