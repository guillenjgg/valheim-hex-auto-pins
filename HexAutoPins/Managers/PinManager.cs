using System;
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

        private static readonly MethodInfo RemovePinMethod = typeof(Minimap).GetMethod(
            "RemovePin",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(Minimap.PinData) },
            null
        );

        private static readonly MethodInfo HaveTargetMethod = typeof(TeleportWorld).GetMethod(
            "HaveTarget",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
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
            bool isConnected = false;
            
            if(HaveTargetMethod != null)
            {
                isConnected = (bool)HaveTargetMethod.Invoke(portal, null);
            }

            Plugin.Log?.LogInfo(
                $"Portal sync. Name: {pinName}, Position: {portalPosition}, Connected: {isConnected}, ID: {portalId}"
            );

            if (PortalPins.TryGetValue(portalId, out Minimap.PinData existingPin))
            {
                UpdatePortalPin(existingPin, portalPosition, pinName);

                return;
            }

            Minimap.PinData savedPin = FindExistingPortalPin(pinName, portalPosition);

            if(savedPin != null)
            {
                PortalPins[portalId] = savedPin;
                UpdatePortalPin(savedPin, portalPosition, pinName);

                return;
            }

            // 0L matches vanilla local/unowned pins. Shared map logic assigns owner IDs when needed
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

        internal static void RemovePortalPin(TeleportWorld portal)
        {
            if(portal == null || Minimap.instance == null)
            {
                return;
            }

            var nview = portal.GetComponent<ZNetView>();

            if(nview == null || !nview.IsValid())
            {
                return;
            }

            var zdo = nview.GetZDO();

            if(zdo == null || zdo.m_uid == ZDOID.None)
            {
                return;
            }

            ZDOID portalId = zdo.m_uid;
            Vector3 portalPosition = portal.gameObject.transform.position;
            string pinName = GetPortalPinName(portal);

            Minimap.PinData pin = null;

            if(PortalPins.TryGetValue(portalId, out Minimap.PinData trackedPin))
            {
                pin = trackedPin;
            }
            else
            {
                pin = FindExistingPortalPin(pinName, portalPosition);
            }

            if(pin == null)
            {
                return;
            }

            RemovePinMethod?.Invoke(Minimap.instance, new object[] { pin });
            PortalPins.Remove(portalId);

            Plugin.Log?.LogInfo($"Removed portal pin. ID: {portalId}, Name: {pinName}");
        }

        internal static void MarkPortalAndPairDisconnected(TeleportWorld portal)
        {
            if (portal == null)
            {
                return;
            }

            ZDOID portalId = GetPortalId(portal);
            ZDOID pairedPortalId = GetConnectedPortalId(portal);
            
            Plugin.Log?.LogInfo($"Portal rename prefix. Portal ID: {portalId}, Paired Portal ID: {pairedPortalId}");

            MarkPortalDisconnected(portalId);
            MarkPortalDisconnected(pairedPortalId);
        }

        private static void MarkPortalDisconnected(ZDOID portalId)
        {
            if(portalId == ZDOID.None)
            {
                return;
            }

            if(!PortalPins.TryGetValue(portalId, out Minimap.PinData pin) || pin == null)
            {
                Plugin.Log?.LogWarning($"No tracked pin to mark disconnected. ID: {portalId}");
                return;
            }

            Plugin.Log?.LogInfo($"Marking portal pin as disconnected. ID: {portalId}, Name: {pin.m_name}");
        }

        private static ZDOID GetPortalId(TeleportWorld portal)
        {
            if(portal == null)
            {
                return ZDOID.None;
            }

            var nview = portal.GetComponent<ZNetView>();

            if(nview == null || !nview.IsValid())
            {
                return ZDOID.None;
            }

            var zdo = nview.GetZDO();

            if(zdo == null)
            {
                return ZDOID.None;
            }

            return zdo.m_uid;
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
            if(MinimapPinsField == null)
            {
                return null;
            }

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
                return string.Empty;
            }

            return tag;
        }

        private static ZDOID GetConnectedPortalId(TeleportWorld portal)
        {
            if (portal == null)
            {
                return ZDOID.None;
            }

            var nview = portal.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
            {
                return ZDOID.None;
            }

            var zdo = nview.GetZDO();

            if (zdo == null)
            {
                return ZDOID.None;
            }

            return zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
        }
    }
}