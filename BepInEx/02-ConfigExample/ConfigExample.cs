// ═══════════════════════════════════════════════════════════
// BEGINNER: Configuration — User-Editable Settings
// Generates BepInEx/config/com.example.configexample.cfg
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

[BepInPlugin(
    guid: "com.example.configexample",
    name: "Config Example",
    version: "1.0.0"
)]
public class ConfigExamplePlugin : BaseUnityPlugin
{
    // ── Config Entries ──────────────────────────────────────
    // Config.Bind(section, key, defaultValue, description)
    // BepInEx auto-generates & persists the .cfg file.

    // Boolean toggle
    private ConfigEntry<bool> _enableFeature;

    // Integer with acceptable range
    private ConfigEntry<int> _damageMultiplier;

    // Float value
    private ConfigEntry<float> _playerSpeed;

    // String value
    private ConfigEntry<string> _greetingMessage;

    // Key binding
    private ConfigEntry<KeyCode> _toggleKey;

    // Enum with acceptable values
    private ConfigEntry<Difficulty> _difficulty;

    // ── Lifecycle ───────────────────────────────────────────

    private void Awake()
    {
        // ── Bind Settings ──────────────────────────────────

        // Section "General"
        _enableFeature = Config.Bind(
            section: "General",
            key: "Enable Feature",
            defaultValue: true,
            description: "Master toggle for the mod's main feature."
        );

        _greetingMessage = Config.Bind(
            section: "General",
            key: "Greeting",
            defaultValue: "Hello, Player!",
            description: "Message shown when the game starts."
        );

        // Section "Combat"
        _damageMultiplier = Config.Bind(
            section: "Combat",
            key: "Damage Multiplier",
            defaultValue: 100,
            descriptionTree: new AcceptableValueRange<int>(50, 500),
            description: "Damage percentage. 100 = normal, 200 = double."
        );

        // Section "Movement"
        _playerSpeed = Config.Bind(
            section: "Movement",
            key: "Speed Multiplier",
            defaultValue: 1.0f,
            description: "Player movement speed multiplier."
        );

        // Section "Controls"
        _toggleKey = Config.Bind(
            section: "Controls",
            key: "Toggle Key",
            defaultValue: KeyCode.RightControl,
            description: "Key to toggle the feature on/off."
        );

        // Section "Gameplay"
        _difficulty = Config.Bind(
            section: "Gameplay",
            key: "Difficulty",
            defaultValue: Difficulty.Normal,
            acceptableValues: new AcceptableValueList<Difficulty>(
                Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Lunatic
            ),
            description: "Mod difficulty preset."
        );

        // ── React to Config Changes ────────────────────────
        _enableFeature.SettingChanged += (oldVal, newVal) =>
        {
            Logger.LogInfo($"Feature is now {(newVal ? "ENABLED" : "DISABLED")}");
        };

        // ── Read Values ────────────────────────────────────
        Logger.LogInfo(_greetingMessage.Value);
        Logger.LogInfo($"Damage: {_damageMultiplier.Value}%, Speed: {_playerSpeed.Value}x");
        Logger.LogInfo($"Difficulty: {_difficulty.Value}");
    }

    private void Update()
    {
        // Example: react to key press from config
        if (Input.GetKeyDown(_toggleKey.Value))
        {
            _enableFeature.Value = !_enableFeature.Value; // Toggle (auto-saves to .cfg)
            Logger.LogInfo($"Feature toggled to {_enableFeature.Value}");
        }
    }

    // ── Enum ────────────────────────────────────────────────
    public enum Difficulty { Easy, Normal, Hard, Lunatic }
}
