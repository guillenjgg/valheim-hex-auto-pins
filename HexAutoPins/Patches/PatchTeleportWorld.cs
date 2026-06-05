using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(TeleportWorld), "UpdatePortal")]
    internal static class PatchTeleportWorldUpdatePortal
    {
        private static void Postfix(TeleportWorld __instance)
        {
            PinManager.SyncPortalPin(__instance);
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.RPC_SetTag))]
    internal static class PatchTeleportWorldRpcSetTag
    {
        private static void Postfix(TeleportWorld __instance, string tag, string authorId)
        {
            PinManager.SyncPortalPin(__instance);
        }
    }
}