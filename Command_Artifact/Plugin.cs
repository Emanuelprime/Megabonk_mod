using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
// using HarmonyLib;

namespace Command_Artifact
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Plugin : BasePlugin
    {
        public const string
            MODNAME = "Command_Artifact",
            AUTHOR = "Prime_Purpura",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "0.1.0";

        public Plugin()
        {
            log = Log;
        }

        public override void Load()
        {
            log.LogInfo($"Loading to testando {MODNAME} v{VERSION} by {AUTHOR}");
        }

        public static ManualLogSource log;
    }
}