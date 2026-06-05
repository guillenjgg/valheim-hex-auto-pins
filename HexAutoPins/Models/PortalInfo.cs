using UnityEngine;

namespace HexAutoPins.Models
{
    internal sealed class PortalInfo
    {
        internal ZDOID PortalId { get; set; }
        internal ZDOID ConnectedPortalId { get; set; }
        internal Vector3 Position { get; set; }
        internal string Tag { get; set; }
        internal bool IsConnected { get; set; }
    }
}
