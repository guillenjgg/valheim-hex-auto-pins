using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
    internal static class PatchTeleportWorldAwake
    {
        private static void Postfix(TeleportWorld __instance)
        {
            Plugin.Instance?.DelayPortalSync(__instance);
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.RPC_SetTag))]
    internal static class PatchTeleportWorldRpcSetTag
    {
        private static void Postfix(TeleportWorld __instance, string tag)
        {
            Plugin.Instance?.DelayPortalSync(__instance);
        }
    }
}