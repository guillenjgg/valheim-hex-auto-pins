using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HexAutoPins.Managers
{
    internal static class PinManager
    {
        private const float PortalPinReconnectDistance = 5f;
        private const Minimap.PinType PortalPinType = Minimap.PinType.Icon4;

        private static readonly FieldInfo MinimapPinsField = typeof(Minimap).GetField(
            "m_pins",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
        );

        private static readonly Dictionary<ZDOID, Minimap.PinData> PortalPins =
            new Dictionary<ZDOID, Minimap.PinData>();

        internal static void SyncPortalPin(TeleportWorld portal)
        {
            if (portal == null || Minimap.instance == null)
            {
                return;
            }

            var nview = portal.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
            {
                return;
            }

            var zdo = nview.GetZDO();

            if (zdo == null || zdo.m_uid == ZDOID.None)
            {
                return;
            }

            GameObject portalObject = portal.gameObject;

            if (portalObject == null)
            {
                return;
            }

            ZDOID portalId = zdo.m_uid;
            Vector3 portalPosition = portalObject.transform.position;
            string pinName = GetPortalPinName(portal);

            if (PortalPins.TryGetValue(portalId, out Minimap.PinData existingPin))
            {
                UpdatePortalPin(existingPin, portalPosition, pinName);

                Plugin.Log?.LogInfo($"Updated portal pin. ID: {portalId}, Name: {pinName}");
                return;
            }

            Minimap.PinData savedPin = FindExistingPortalPin(pinName, portalPosition);

            if(savedPin != null)
            {
                PortalPins[portalId] = savedPin;
                UpdatePortalPin(savedPin, portalPosition, pinName);

                Plugin.Log?.LogInfo($"Reconnected portal pin. ID: {portalId}, Name: {pinName}");
                return;
            }

            // 0L matches vanilla local/unowned pins. Shared map logic assings owner IDs when needed
            var ownerId = 0L;

            Minimap.PinData newPin = Minimap.instance.AddPin(
                portalPosition,
                PortalPinType,
                pinName,
                true,
                false,
                ownerId
            );

            PortalPins.Add(portalId, newPin);
            Plugin.Log?.LogInfo($"Created portal pin. ID: {portalId}, Name: {pinName}");
        }

        private static void UpdatePortalPin(Minimap.PinData pin, Vector3 position, string name)
        {
            if(pin == null)
            {
                return;
            }

            pin.m_pos = position;
            pin.m_name = name;
            pin.m_type = PortalPinType;
        }

        private static Minimap.PinData FindExistingPortalPin(string pinName, Vector3 position)
        {
            var pins = MinimapPinsField.GetValue(Minimap.instance) as List<Minimap.PinData>;

            if(pins == null)
            {
                return null;
            }

            foreach (var pin in pins)
            {
                if (!pin.m_save || pin.m_type != PortalPinType || pin.m_name != pinName)
                {
                    continue;
                }

                if (Vector3.Distance(pin.m_pos, position) > PortalPinReconnectDistance)
                {
                    continue;
                }

                return pin;
            }

            return null;
        }

        private static string GetPortalPinName(TeleportWorld portal)
        {
            string tag = portal.GetText();

            if (string.IsNullOrWhiteSpace(tag))
            {
                return "Portal";
            }

            return tag;
        }
    }
}