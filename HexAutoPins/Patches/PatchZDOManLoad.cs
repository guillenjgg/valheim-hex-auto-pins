using HarmonyLib;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Load))]
    internal static class PatchZDOManLoad
    {
        private static void Postfix()
        {
            Plugin.Log.LogInfo("ZDOMan.Load postfix called. Refreshing portals.");
            Managers.PortalManager.RefreshPortalsOnce();
        }
    }
}
