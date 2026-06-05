using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(Game), nameof(Game.ConnectPortals))]
    internal static class PatchGameConnectPortals
    {
        private static void Postfix()
        {
            Plugin.Log.LogInfo("Game.ConnectPortals fired.");

            PortalManager.RefreshPortals();

            if (Minimap.instance == null)
            {
                Plugin.Log.LogInfo("Minimap.instance is null. Skipping pin sync.");
                return;
            }

            if (!PinManager.IsReady)
            {
                Plugin.Log.LogInfo("PinManager is not ready. Skipping pin sync.");
                return;
            }

            PinManager.SyncPortalPins();
        }
    }
}