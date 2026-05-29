// ============================================================
// Example 09: Inter-Mod Communication (Expert)
// ============================================================
// Demonstrates:
// - Detecting other loaded mods
// - Calling methods in other mods
// - Shared events and callbacks
// - Mod dependencies
// - API exposure patterns
//
// Enable mods to work together seamlessly
// ============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using MelonLoader.Utils;

namespace InterModCommsExample
{
    [assembly: MelonInfo_name("Inter-Mod Comms Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates inter-mod communication patterns")]

    public class InterModCommsExample : MelonMod
    {
        // Track discovered mods
        private Dictionary<string, MelonMod> _discoveredMods = new Dictionary<string, MelonMod>();

        // Event delegates for mod communication
        public static event Action<string> OnModMessage;
        public static event Action<string, object> OnDataShared;

        override public void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing inter-mod communication...");

            // Discover loaded mods
            DiscoverMods();

            // Try to communicate with known mods
            CommunicateWithMods();

            // Register our API for other mods
            RegisterAPI();

            MelonLogger.Msg("Inter-mod communication ready");
        }

        /// <summary>
        /// Discover all loaded mods
        /// </summary>
        private void DiscoverMods()
        {
            MelonLogger.Msg("=== Loaded Mods ===");

            foreach (var mod in MelonModManager.Mods)
            {
                if (mod == this) continue; // Skip ourselves

                _discoveredMods[mod.Info.Name] = mod;
                MelonLogger.Msg($"Found: {mod.Info.Name} v{mod.Info.Version}");
                MelonLogger.Msg($"  Author: {mod.Info.Author}");
                MelonLogger.Msg($"  Type: {mod.GetType().FullName}");
            }

            MelonLogger.Msg($"Total mods discovered: {_discoveredMods.Count}");
        }

        /// <summary>
        /// Try to communicate with known mods
        /// </summary>
        private void CommunicateWithMods()
        {
            // Example 1: Check if a specific mod is loaded
            if (IsModLoaded("HelloWorldMod"))
            {
                MelonLogger.Msg("HelloWorldMod is loaded!");
            }

            // Example 2: Call a public method in another mod
            CallModMethod("ConfigMod", "SetDamageMultiplier", 5);

            // Example 3: Access a public field/property
            var version = GetModProperty<string>("SomeMod", "Version");
            if (version != null)
            {
                MelonLogger.Msg($"SomeMod version: {version}");
            }

            // Example 4: Fire our own events
            OnModMessage?.Invoke("Hello from InterModCommsExample!");
        }

        /// <summary>
        /// Check if a specific mod is loaded
        /// </summary>
        public bool IsModLoaded(string modName)
        {
            foreach (var mod in MelonModManager.Mods)
            {
                if (mod.Info.Name == modName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a mod's instance by name
        /// </summary>
        public MelonMod GetModByName(string modName)
        {
            return _discoveredMods.GetValueOrDefault(modName);
        }

        /// <summary>
        /// Call a public method in another mod via reflection
        /// </summary>
        public bool CallModMethod(string modName, string methodName, params object[] args)
        {
            var mod = GetModByName(modName);
            if (mod == null)
            {
                MelonLogger.Warning($"Mod not found: {modName}");
                return false;
            }

            try
            {
                var method = mod.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                if (method == null)
                {
                    MelonLogger.Warning($"Method not found: {modName}.{methodName}");
                    return false;
                }

                method.Invoke(mod, args);
                MelonLogger.Msg($"Called {modName}.{methodName}");
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to call {modName}.{methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get a public property from another mod
        /// </summary>
        public T GetModProperty<T>(string modName, string propertyName)
        {
            var mod = GetModByName(modName);
            if (mod == null) return default;

            try
            {
                var property = mod.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                if (property == null)
                {
                    MelonLogger.Warning($"Property not found: {modName}.{propertyName}");
                    return default;
                }

                return (T)property.GetValue(mod);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to get property {modName}.{propertyName}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Register our API for other mods to use
        /// </summary>
        private void RegisterAPI()
        {
            // Other mods can access our public methods directly:
            // - IsModLoaded(string)
            // - GetModByName(string)
            // - CallModMethod(string, string, params object[])
            // - GetModProperty<T>(string, string)

            // Or subscribe to our events:
            // - OnModMessage
            // - OnDataShared

            MelonLogger.Msg("API registered. Other mods can now interact with us.");
        }

        /// <summary>
        /// Example: Broadcast a message to all listening mods
        /// </summary>
        public void BroadcastMessage(string message)
        {
            MelonLogger.Msg($"[Broadcast] {message}");
            OnModMessage?.Invoke(message);
        }

        /// <summary>
        /// Example: Share data with other mods
        /// </summary>
        public void ShareData(string key, object data)
        {
            MelonLogger.Msg($"[Data Share] {key} = {data}");
            OnDataShared?.Invoke(key, data);
        }

        override public void OnUpdate()
        {
            // Press Home to list all mods
            if (Input.GetKeyDown(KeyCode.Home))
            {
                ListAllMods();
            }
        }

        /// <summary>
        /// Log all loaded mods with details
        /// </summary>
        private void ListAllMods()
        {
            MelonLogger.Msg("=== All Loaded Mods ===");

            foreach (var mod in MelonModManager.Mods)
            {
                MelonLogger.Msg($"[{mod.Info.Name}]");
                MelonLogger.Msg($"  Version: {mod.Info.Version}");
                MelonLogger.Msg($"  Author: {mod.Info.Author}");
                MelonLogger.Msg($"  Description: {mod.Info.Description}");
                MelonLogger.Msg($"  Type: {mod.GetType().Assembly.GetName().Name}");

                // List public methods
                var methods = mod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_"))
                    {
                        MelonLogger.Msg($"  Method: {method.Name}");
                    }
                }
            }
        }
    }

    // =========================================================
    // Example: API Interface Pattern
    // =========================================================
    // Define a shared interface that mods can implement
    // to enable standardized communication

    /// <summary>
    /// Optional interface for mods that want to support
    /// standardized inter-mod communication
    /// </summary>
    public interface IModAPI
    {
        string ModName { get; }
        Version Version { get; }
        bool IsInitialized { get; }

        void SendMessage(string message);
        object CallMethod(string methodName, params object[] args);
    }

}
