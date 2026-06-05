using System.Reflection;
using UnityEngine;

namespace HexAutoPins.Managers
{
    internal static class AssetManager
    {
        private const string BundleResourceName = "HexAutoPins.Assets.Embedded.hexautopins";
        private const string ActivePortalSpriteAssetPath = "assets/hexautopins/texture/icons/hex_autopins_portal_active.png";

        internal static Sprite ActivePortalSprite { get; private set; }

        internal static void LoadAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(BundleResourceName))
            {
                if (stream == null)
                {
                    Plugin.Log.LogError($"Failed to load asset bundle from resource: {BundleResourceName}");
                    return;
                }

                var bundleBytes = new byte[stream.Length];
                stream.Read(bundleBytes, 0, bundleBytes.Length);

                var bundle = AssetBundle.LoadFromMemory(bundleBytes);

                foreach (string assetName in bundle.GetAllAssetNames())
                {
                    Plugin.Log?.LogInfo($"Bundle asset: {assetName}");
                }

                if (bundle == null)
                {
                    Plugin.Log.LogError("Failed to load asset bundle from memory.");
                    return;
                }

                ActivePortalSprite = bundle.LoadAsset<Sprite>(ActivePortalSpriteAssetPath);

                if (ActivePortalSprite == null)
                {
                    Plugin.Log?.LogError("Failed to load active portal sprite.");
                }
                else
                {
                    Plugin.Log?.LogInfo($"Loaded active portal sprite: {ActivePortalSprite.name}");
                }

                bundle.Unload(false);
            }
        }
    }
}
