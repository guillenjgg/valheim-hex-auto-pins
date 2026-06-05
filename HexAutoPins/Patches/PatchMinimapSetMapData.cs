using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapData))]
    internal static class PatchMinimapSetMapData
    {
        private static void Postfix()
        {
            PinManager.SetReady();
            PinManager.ClearTrackedPortalPins();
            PinManager.SyncPortalPins();
        }
    }
}
