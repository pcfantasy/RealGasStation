using HarmonyLib;

namespace RealGasStation.Util
{
    internal static class HarmonyDetours
    {
        public const string ID = "pcfantasy.realgasstation";
        public static void Apply()
        {
            var harmony = new Harmony(HarmonyDetours.ID);
            harmony.PatchAll();
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            var harmony = new Harmony(HarmonyDetours.ID);
            harmony.UnpatchAll(ID);
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}
