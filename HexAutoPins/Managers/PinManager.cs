using HexAutoPins.Models;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HexAutoPins.Managers
{
    internal static class PinManager
    {
        private const float PortalPinReconnectDistance = 1f;
        private const Minimap.PinType PortalPinType = Minimap.PinType.Icon4;

        private static Sprite VanillaPortalSprite;

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

        private static readonly MethodInfo CreateMapNamePinMethod = typeof(Minimap).GetMethod(
            "CreateMapNamePin",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(Minimap.PinData), typeof(RectTransform) },
            null
        );

        private static readonly FieldInfo PinNameRootLargeField = typeof(Minimap).GetField(
            "m_pinNameRootLarge",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
        );

        private static readonly Dictionary<ZDOID, Minimap.PinData> _portalPins =
            new Dictionary<ZDOID, Minimap.PinData>();

        internal static void SyncPortalPins()
        {
            foreach (var portal in PortalManager.Portals.Values)
            {
                if (portal == null)
                {
                    continue;
                }

                SyncPortalPin(portal);
            }
        }

        internal static void SyncPortalPin(PortalInfo portal)
        {
            if (portal == null || Minimap.instance == null)
            {
                return;
            }

            if(portal.PortalId == ZDOID.None)
            {
                return;
            }

            Minimap.PinData pin;

            if (!_portalPins.TryGetValue(portal.PortalId, out pin))
            {
                pin = FindExistingPortalPin(portal.Tag, portal.Position);

                if (pin == null)
                {
                    // The 0L is vanillas default owner ID for player created pins
                    pin = Minimap.instance.AddPin(
                        portal.Position,
                        PortalPinType,
                        portal.Tag,
                        true,
                        false,
                        0L);

                }
                
                _portalPins[portal.PortalId] = pin;
            }

            UpdatePortalPin(pin, portal.Position, portal.Tag, portal.IsConnected);
        }

        internal static void RemovePortalPin(ZDOID portalId)
        {
            if(Minimap.instance == null)
            {
                return;
            }

            var isPortalCached = PortalManager.Portals.TryGetValue(portalId, out PortalInfo portal);

            if(!isPortalCached)
            {
                Plugin.Log.LogWarning($"Portal {portalId} was not found in PortalManager.");

                return;
            }

            Minimap.PinData pin;

            if (!_portalPins.TryGetValue(portalId, out pin))
            {
                pin = FindExistingPortalPin(portal.Tag, portal.Position);
            }

            if (pin == null)
            {
                return;
            }

            RemovePinMethod?.Invoke(Minimap.instance, new object[] { pin });
            _portalPins.Remove(portalId);

            Plugin.Log.LogInfo($"Removed portal pin for portal {portalId}.");
        }

        internal static void ClearTrackedPortalPins()
        {
            _portalPins.Clear();
        }

        private static void UpdatePortalPin(
            Minimap.PinData pin,
            Vector3 position,
            string name,
            bool isConnected)
        {
            if (pin == null)
            {
                return;
            }

            CacheVanillaPortalSprite(pin);

            string oldName = pin.m_name;

            pin.m_pos = position;
            pin.m_name = name;
            pin.m_type = PortalPinType;

            if (oldName != name)
            {
                EnsurePortalPinName(pin);
            }

            SetPortalPinIcon(pin, isConnected);
        }

        private static void SetPortalPinIcon(Minimap.PinData pin, bool isConnected)
        {
            if (pin == null)
            {
                return;
            }

            Sprite desiredSprite = null;

            if (isConnected && AssetManager.ActivePortalSprite != null)
            {
                desiredSprite = AssetManager.ActivePortalSprite;
            }
            else if (VanillaPortalSprite != null)
            {
                desiredSprite = VanillaPortalSprite;
            }

            if (desiredSprite == null)
            {
                return;
            }

            pin.m_icon = desiredSprite;

            if (pin.m_iconElement != null)
            {
                pin.m_iconElement.sprite = desiredSprite;
            }
        }

        private static void CacheVanillaPortalSprite(Minimap.PinData pin)
        {
            if (VanillaPortalSprite != null || pin == null || pin.m_icon == null)
            {
                return;
            }

            if (pin.m_icon == AssetManager.ActivePortalSprite)
            {
                return;
            }

            VanillaPortalSprite = pin.m_icon;
        }

        private static Minimap.PinData FindExistingPortalPin(string pinName, Vector3 position)
        {
            if (MinimapPinsField == null || Minimap.instance == null)
            {
                return null;
            }

            List<Minimap.PinData> pins =
                MinimapPinsField.GetValue(Minimap.instance) as List<Minimap.PinData>;

            if (pins == null)
            {
                return null;
            }

            foreach (Minimap.PinData pin in pins)
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

        private static void EnsurePortalPinName(Minimap.PinData pin)
        {
            if (pin == null || Minimap.instance == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(pin.m_name))
            {
                return;
            }

            if (pin.m_NamePinData == null)
            {
                pin.m_NamePinData = new Minimap.PinNameData(pin);
            }

            if (pin.m_NamePinData.PinNameGameObject != null)
            {
                Object.Destroy(pin.m_NamePinData.PinNameGameObject);
            }

            pin.m_NamePinData = new Minimap.PinNameData(pin);

            RectTransform root = PinNameRootLargeField?.GetValue(Minimap.instance) as RectTransform;

            if (root == null)
            {
                return;
            }

            CreateMapNamePinMethod?.Invoke(Minimap.instance, new object[] { pin, root });
        }

        private static bool _isReady;

        internal static bool IsReady => _isReady;

        internal static void SetReady()
        {
            _isReady = true;
        }
    }
}