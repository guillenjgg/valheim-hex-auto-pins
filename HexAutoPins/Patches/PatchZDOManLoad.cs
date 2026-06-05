using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Load))]
    internal static class PatchZDOManLoad
    {
        private static void Postfix()
        {
            PortalManager.RefreshPortals();
        }
    }
}
