// ============================================================
// Example 02: Config Mod (Beginner)
// ============================================================
// Demonstrates:
// - MelonPreferences for persistent settings
// - Creating categorized config sections
// - Reading/writing different data types
//
// Config is saved to: MelonLoader/config/<AssemblyName>.json
// ============================================================

using System;
using MelonLoader;

namespace ConfigMod
{
    [assembly: MelonInfo_name("Config Example Mod")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates MelonPreferences configuration system")]

    public class ConfigMod : MelonMod
    {
        // =========================================================
        // Define your config section name
        // =========================================================
        private const string MainCategory = "General";
        private const string AdvancedCategory = "Advanced";

        // =========================================================
        // Define your settings with defaults
        // MelonPreferences persists these automatically
        // =========================================================

        // Boolean toggle
        private static MelonPreferences_Category _mainCategory;
        private static MelonPreferences_Entry<bool> _enableMod;

        // Integer slider
        private static MelonPreferences_Entry<int> _damageMultiplier;

        // Float value
        private static MelonPreferences_Entry<float> _speedBoost;

        // String text
        private static MelonPreferences_Entry<string> _greetingMessage;

        // Boolean with keyboard shortcut binding
        private static MelonPreferences_Entry<bool> _showDebugInfo;

        override public void OnApplicationStart()
        {
            // Create config categories
            _mainCategory = MelonPreferences.CreateCategory("ConfigMod", "Config Example Settings");

            // Create entries with: name, defaultValue, description, category, keyboard shortcut (optional)
            _enableMod = _mainCategory.CreateEntry(
                "EnableMod",
                true,
                "Enable/disable the mod functionality");

            _damageMultiplier = _mainCategory.CreateEntry(
                "DamageMultiplier",
                1,
                1, 10, // min, max for slider
                "Damage multiplier (1-10)");

            _speedBoost = _mainCategory.CreateEntry(
                "SpeedBoost",
                1.0f,
                0.1f, 5.0f,
                "Movement speed multiplier");

            _greetingMessage = _mainCategory.CreateEntry(
                "GreetingMessage",
                "Hello, player!",
                "Custom greeting message");

            _showDebugInfo = _mainCategory.CreateEntry(
                "ShowDebugInfo",
                false,
                "Display debug information in console");

            // Log loaded values
            LogCurrentSettings();
        }

        override public void OnUpdate()
        {
            // Read config values
            bool isEnabled = _enableMod.Value;
            int damageMult = _damageMultiplier.Value;
            float speed = _speedBoost.Value;
            string greeting = _greetingMessage.Value;

            // Use config in your mod logic
            if (!isEnabled) return;

            // Press F6 to log current settings
            if (Input.GetKeyDown(KeyCode.F6))
            {
                LogCurrentSettings();
            }
        }

        private void LogCurrentSettings()
        {
            MelonLogger.Msg("=== Config Mod Settings ===");
            MelonLogger.Msg($"Enable Mod: {_enableMod.Value}");
            MelonLogger.Msg($"Damage Multiplier: {_damageMultiplier.Value}");
            MelonLogger.Msg($"Speed Boost: {_speedBoost.Value}");
            MelonLogger.Msg($"Greeting: {_greetingMessage.Value}");
            MelonLogger.Msg($"Debug Info: {_showDebugInfo.Value}");
            MelonLogger.Msg("================================");
        }

        /// <summary>
        /// Example: Programmatically change a setting
        /// </summary>
        public static void SetDamageMultiplier(int value)
        {
            _damageMultiplier.Value = Mathf.Clamp(value, 1, 10);
            MelonLogger.Msg($"Damage multiplier set to {_damageMultiplier.Value}");
        }

        /// <summary>
        /// Example: Reset all settings to defaults
        /// </summary>
        public static void ResetToDefaults()
        {
            _mainCategory.ResetToDefaults();
            MelonLogger.Msg("All settings reset to defaults");
        }
    }
}
