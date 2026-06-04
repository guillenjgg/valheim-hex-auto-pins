using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy))]
    internal static class PatchWearNTearDestroy
    {
        private static void Prefix(WearNTear __instance)
        {
            if (__instance == null)
            {
                return;
            }

            var portal = __instance.GetComponent<TeleportWorld>();

            if(portal == null)
            {
                return;
            }

            PinManager.RemovePortalPin(portal);
        }
    }
}
