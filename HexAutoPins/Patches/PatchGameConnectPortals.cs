using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(Game), nameof(Game.ConnectPortals))]
    internal static class PatchGameConnectPortals
    {
        private static void Postfix()
        {
            PortalManager.RefreshPortals();

            if (Minimap.instance == null)
            {
                return;
            }

            if (!PinManager.IsReady)
            {
                return;
            }

            PinManager.SyncPortalPins();
        }
    }
}