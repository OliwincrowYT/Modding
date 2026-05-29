// ============================================================
// Example 08: Asset Bundles (Advanced)
// ============================================================
// Demonstrates:
// - Loading custom assets (models, textures, prefabs)
// - Creating and using AssetBundles
// - Instantiating custom GameObjects
// - Resource management and cleanup
// - Hot-reloading assets
//
// AssetBundles are Unity's format for packaged assets
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MelonLoader;

namespace AssetBundlesExample
{
    [assembly: MelonInfo_name("Asset Bundles Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates loading custom assets via AssetBundles")]

    public class AssetBundlesExample : MelonMod
    {
        // Asset loading state
        private Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        private Dictionary<string, GameObject> _instantiatedObjects = new Dictionary<string, GameObject>();

        // Path to assets folder
        private static readonly string _assetsPath;

        static AssetBundlesExample()
        {
            // Assets stored alongside the mod
            _assetsPath = Path.Combine(MelonModManager.ModsDir, "AssetBundlesExample", "assets");
        }

        override public void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing AssetBundle loader...");

            // Create assets directory if it doesn't exist
            if (!Directory.Exists(_assetsPath))
            {
                Directory.CreateDirectory(_assetsPath);
                MelonLogger.Msg($"Created assets directory: {_assetsPath}");
                MelonLogger.Msg("Place your .assetbundle files in the assets folder");
            }

            // Load all asset bundles in the directory
            LoadAssetBundles();

            // Example: Spawn a custom object
            SpawnCustomObject();
        }

        override public void OnApplicationQuit()
        {
            // Clean up loaded assets
            UnloadAllAssets();
            MelonLogger.Msg("AssetBundles cleaned up");
        }

        /// <summary>
        /// Load all .assetbundle files from assets directory
        /// </summary>
        private void LoadAssetBundles()
        {
            var bundleFiles = Directory.GetFiles(_assetsPath, "*.assetbundle");

            foreach (var filePath in bundleFiles)
            {
                string bundleName = Path.GetFileNameWithoutExtension(filePath);

                try
                {
                    // Load the asset bundle
                    var bundle = AssetBundle.LoadFromFile(filePath);

                    if (bundle != null)
                    {
                        _loadedBundles[bundleName] = bundle;
                        MelonLogger.Msg($"Loaded bundle: {bundleName} ({filePath})");

                        // Log contents
                        LogBundleContents(bundleName, bundle);
                    }
                    else
                    {
                        MelonLogger.Warning($"Failed to load bundle: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error loading bundle {filePath}: {ex.Message}");
                }
            }

            MelonLogger.Msg($"Loaded {_loadedBundles.Count} asset bundles");
        }

        /// <summary>
        /// Log the contents of an asset bundle
        /// </summary>
        private void LogBundleContents(string bundleName, AssetBundle bundle)
        {
            var assetNames = bundle.GetAllAssetNames();

            MelonLogger.Msg($"Bundle '{bundleName}' contains:");
            foreach (var name in assetNames)
            {
                MelonLogger.Msg($"  - {name}");
            }
        }

        /// <summary>
        /// Example: Spawn a custom GameObject from asset bundle
        /// </summary>
        private void SpawnCustomObject()
        {
            // Try to load and instantiate a prefab from a bundle
            if (!_loadedBundles.ContainsKey("custom_objects")) return;

            var bundle = _loadedBundles["custom_objects"];

            // Load a specific prefab
            var prefab = bundle.LoadAsset<GameObject>("CustomCube");

            if (prefab != null)
            {
                // Instantiate in the scene
                var instance = GameObject.Instantiate(prefab);
                instance.name = "ModdedCube";
                instance.transform.position = new Vector3(0, 10, 0);

                _instantiatedObjects["CustomCube"] = instance;
                MelonLogger.Msg("Spawned CustomCube at (0, 10, 0)");
            }
        }

        /// <summary>
        /// Load a specific asset from a bundle
        /// </summary>
        public T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            if (!_loadedBundles.ContainsKey(bundleName))
            {
                MelonLogger.Warning($"Bundle not found: {bundleName}");
                return null;
            }

            var bundle = _loadedBundles[bundleName];
            var asset = bundle.LoadAsset<T>(assetName);

            if (asset == null)
            {
                MelonLogger.Warning($"Asset not found in bundle: {bundleName}/{assetName}");
            }

            return asset;
        }

        /// <summary>
        /// Load a texture from an asset bundle
        /// </summary>
        public Texture2D LoadTexture(string bundleName, string textureName)
        {
            return LoadAsset<Texture2D>(bundleName, textureName);
        }

        /// <summary>
        /// Load a material from an asset bundle
        /// </summary>
        public Material LoadMaterial(string bundleName, string materialName)
        {
            return LoadAsset<Material>(bundleName, materialName);
        }

        /// <summary>
        /// Apply a custom texture to an existing GameObject
        /// </summary>
        public void ApplyCustomTexture(GameObject target, string bundleName, string textureName)
        {
            var texture = LoadTexture(bundleName, textureName);

            if (texture != null && target != null)
            {
                var renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.mainTexture = texture;
                    MelonLogger.Msg($"Applied texture {textureName} to {target.name}");
                }
            }
        }

        /// <summary>
        /// Unload all loaded assets and clean up
        /// </summary>
        private void UnloadAllAssets()
        {
            // Destroy instantiated objects
            foreach (var obj in _instantiatedObjects.Values)
            {
                if (obj != null)
                {
                    GameObject.Destroy(obj);
                }
            }
            _instantiatedObjects.Clear();

            // Unload asset bundles
            foreach (var bundle in _loadedBundles.Values)
            {
                if (bundle != null)
                {
                    bundle.Unload(false); // false = keep instantiated objects
                }
            }
            _loadedBundles.Clear();
        }

        override public void OnUpdate()
        {
            // Press F12 to reload assets
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ReloadAssets();
            }
        }

        /// <summary>
        /// Hot-reload all asset bundles
        /// </summary>
        private void ReloadAssets()
        {
            MelonLogger.Msg("Reloading assets...");

            // Unload current bundles
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle?.Unload(true); // true = force unload
            }
            _loadedBundles.Clear();

            // Reload
            LoadAssetBundles();
            MelonLogger.Msg("Assets reloaded");
        }
    }
}
