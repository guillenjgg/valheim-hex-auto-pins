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

            ZDO zdo = nview.GetZDO();

            if (zdo == null || zdo.m_uid == ZDOID.None)
            {
                return;
            }

            ZDOID portalId = zdo.m_uid;
            Vector3 position = zdo.GetPosition();
            string pinName = GetPortalPinNameFromZdo(zdo);
            bool isConnected = IsPortalConnected(zdo);

            Minimap.PinData pin;

            if (!PortalPins.TryGetValue(portalId, out pin) || pin == null)
            {
                pin = FindExistingPortalPin(pinName, position);

                if (pin == null)
                {
                    pin = Minimap.instance.AddPin(
                        position,
                        PortalPinType,
                        pinName,
                        true,
                        false,
                        0L
                    );
                }

                PortalPins[portalId] = pin;
            }

            UpdatePortalPin(pin, position, pinName, isConnected);
        }

        internal static void RemovePortalPin(TeleportWorld portal)
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

            ZDO zdo = nview.GetZDO();

            if (zdo == null || zdo.m_uid == ZDOID.None)
            {
                return;
            }

            ZDOID portalId = zdo.m_uid;
            Vector3 position = zdo.GetPosition();
            string pinName = GetPortalPinNameFromZdo(zdo);

            Minimap.PinData pin;

            if (!PortalPins.TryGetValue(portalId, out pin))
            {
                pin = FindExistingPortalPin(pinName, position);
            }

            if (pin == null)
            {
                return;
            }

            RemovePinMethod?.Invoke(Minimap.instance, new object[] { pin });
            PortalPins.Remove(portalId);
        }

        internal static void ClearTrackedPortalPins()
        {
            PortalPins.Clear();
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

        private static bool IsPortalConnected(ZDO zdo)
        {
            if (zdo == null)
            {
                return false;
            }

            return zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None;
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

        private static string GetPortalPinNameFromZdo(ZDO zdo)
        {
            if (zdo == null)
            {
                return string.Empty;
            }

            string tag = zdo.GetString(ZDOVars.s_tag, string.Empty);

            if (string.IsNullOrWhiteSpace(tag))
            {
                return string.Empty;
            }

            return tag;
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
    }
}