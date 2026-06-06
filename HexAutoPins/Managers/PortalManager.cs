using HexAutoPins.Models;
using System.Collections.Generic;

namespace HexAutoPins.Managers
{
    internal static class PortalManager
    {
        private static readonly Dictionary<ZDOID, PortalInfo> _portals = new Dictionary<ZDOID, PortalInfo>();

        internal static IReadOnlyDictionary<ZDOID, PortalInfo> Portals => _portals;

        internal static void RegisterPortal(ZDO portal)
        {
            if (portal == null || portal.m_uid == ZDOID.None)
            {
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
        }

        internal static void RefreshPortals()
        {
            _portals.Clear();

            if (ZDOMan.instance == null)
            {
                return;
            }

            var portals = ZDOMan.instance.GetPortals();

            foreach (var zdo in portals)
            {
                RegisterPortal(zdo);
            }
        }

        internal static void RemovePortal(ZDOID portalId)
        {
            if(portalId == ZDOID.None)
            {
                return;
            }

            _portals.Remove(portalId);
        }
    }
}
