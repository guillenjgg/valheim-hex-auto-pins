using HexAutoPins.Models;
using System.Collections.Generic;

namespace HexAutoPins.Managers
{
    internal static class PortalManager
    {
        private static readonly Dictionary<ZDOID, PortalInfo> _portals = new Dictionary<ZDOID, PortalInfo>();
        private static bool HasLoadedPortals;

        internal static IReadOnlyDictionary<ZDOID, PortalInfo> Portals => _portals;

        internal static void RegisterPortal(ZDO portal)
        {
            if (portal == null || portal.m_uid == ZDOID.None)
            {
                Plugin.Log.LogWarning("ZDOMan.instance is null.");
                return;
            }

            var connectedPortalId = portal.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);

            var portalInfo = new PortalInfo
            {
                PortalId = portal.m_uid,
                ConnectedPortalId = connectedPortalId,
                Position = portal.GetPosition(),
                Tag = portal.GetString("tag", ""),
                IsConnected = connectedPortalId != ZDOID.None
            };

            _portals[portalInfo.PortalId] = portalInfo;

            Plugin.Log.LogInfo(
                $"Registered Portal | " +
                $"ID: {portalInfo.PortalId} | " +
                $"Tag: '{portalInfo.Tag}' | " +
                $"Connected: {portalInfo.IsConnected} | " +
                $"Connected ID: {portalInfo.ConnectedPortalId}"
            );
        }

        internal static void RefreshPortals()
        {
            _portals.Clear();

            if (ZDOMan.instance == null)
            {
                Plugin.Log.LogWarning("ZDOMan.instance is null.");
                return;
            }

            var portals = ZDOMan.instance.GetPortals();

            Plugin.Log.LogInfo($"Refreshing portals. Found {portals.Count} portal ZDOs.");

            foreach (var zdo in portals)
            {
                RegisterPortal(zdo);
            }

            Plugin.Log.LogInfo($"Portal dictionary contains {_portals.Count} portals.");
        }

        internal static void RefreshPortalsOnce()
        {
            if (HasLoadedPortals)
            {
                return;
            }

            RefreshPortals();

            HasLoadedPortals = true;
        }
    }
}
