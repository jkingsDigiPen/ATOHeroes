using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using static Obeliskial_Essentials.Essentials;

namespace Mesmer
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, "Mesmer", PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.stiffmeds.obeliskialessentials")]
    [BepInDependency("com.stiffmeds.obeliskialcontent")]
    [BepInProcess("AcrossTheObelisk.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal int ModDate = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;

        public static ConfigEntry<bool> EnableDebugging { get; set; }

        public static string characterName = "Mesmer";
        public static string heroName = characterName;

        public static string subclassName = "Riftkeeper"; // needs caps

        public static string subclassname = subclassName.ToLower();
        public static string itemStem = subclassname;
        public static string debugBase = "Ilyendur - Testing " + characterName + " ";

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");

            EnableDebugging = Config.Bind(new ConfigDefinition(subclassName, "Enable Debugging"), true, 
                new ConfigDescription("Enables debugging logs."));

            // register with Obeliskial Essentials
            RegisterMod(
                _name: characterName,
                _author: "ilyendur",
                _description: characterName,
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://github.com/jkingsDigiPen",
                _contentFolder: characterName,
                _type: new string[3] { "content", "hero", "trait" }
            );

            // apply patches
            harmony.PatchAll();
        }

        internal static void LogDebug(string msg)
        {
            if (EnableDebugging.Value)
            {
                Log.LogDebug(debugBase + msg);
            }

        }
        internal static void LogInfo(string msg)
        {
            Log.LogInfo(debugBase + msg);
        }
        internal static void LogError(string msg)
        {
            Log.LogError(debugBase + msg);
        }
    }
}
