using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
    internal static class PatchTeleportWorldAwake
    {
        private static void Postfix(TeleportWorld __instance)
        {
            if(__instance == null)
            {
                return;
            }

            var nview = __instance.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
            {
                return;
            }

            var zdo = nview.GetZDO();

            if (zdo == null)
            {
                return;
            }

            PortalManager.RegisterPortal(zdo);
            PinManager.SyncPortalPins();
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.RPC_SetTag))]
    internal static class PatchTeleportWorldRpcSetTag
    {
        private static void Postfix()
        {
            PortalManager.RefreshPortals();
            PinManager.SyncPortalPins();
        }
    }
}