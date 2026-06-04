using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HexAutoPins.Managers;
using System.Collections;
using UnityEngine;

namespace HexAutoPins
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.autopins";
        private const string PluginName = "HexAutoPins";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll();

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            Log.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Instance = null;
            Log = null;
        }

        // TeleportWorld Awake fires very early, so portal data may not be fully available
        // Rather than trying to figure out Valheims internal timing, I decided to add a short delay before syncing
        internal void DelayPortalSync(TeleportWorld portal)
        {
            StartCoroutine(DelayPortalSyncCoroutine(portal));
        }

        private IEnumerator DelayPortalSyncCoroutine(TeleportWorld portal)
        {
            yield return new WaitForSeconds(1f);

            if (portal == null)
            {
                yield break;
            }

            PinManager.SyncPortalPin(portal);
        }
    }
}
