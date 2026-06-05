using HarmonyLib;
using HexAutoPins.Managers;

namespace HexAutoPins.Patches
{
    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
    internal static class PatchTeleportWorldAwake
    {
        private static void Postfix(TeleportWorld __instance)
        {
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

            Plugin.Log.LogInfo($"Portal Awake: {zdo.m_uid}");

            PortalManager.RegisterPortal(zdo);
            PinManager.SyncPortalPins();
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.RPC_SetTag))]
    internal static class PatchTeleportWorldRpcSetTag
    {
        private static void Postfix()
        {
            Plugin.Log.LogInfo("RPC_SetTag fired.");

            PortalManager.RefreshPortals();

            Plugin.Log.LogInfo($"Portal dictionary contains {PortalManager.Portals.Count} portals after refresh.");

            PinManager.SyncPortalPins();
        }
    }
}