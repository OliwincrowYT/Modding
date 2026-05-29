// ═══════════════════════════════════════════════════════════
// ADVANCED: Plugin Interop — Multi-Mod Communication
// Dependencies, interop, conflict resolution, public APIs
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════
// DEPENDENCY ATTRIBUTES
// ═══════════════════════════════════════════════════════

[BepInPlugin(
    guid: "com.example.interop",
    name: "Interop Demo",
    version: "1.0.0"
)]

// HARD dependency — this plugin WON'T load without it
[BepInDependency("com.example.core", BepInDependencyMode.Hard)]

// SOFT dependency — works without, enhanced with
[BepInDependency("com.example.optional", BepInDependencyMode.Soft)]

// INCOMPATIBLE — refuse to load alongside this
[BepInIncompatibility("com.example.conflicting")]

// Only load in specific games
[BepInProcess("MyGame.exe")]
[BepInProcess("UnityPlayer.dll")] // Or Unity standalone

public class InteropDemo : BaseUnityPlugin
{
    // ── Public API ────────────────────────────────────────
    // Other plugins access your features through this
    public static InteropDemo Instance { get; private set; }

    // Public events other mods can subscribe to
    public event Action<string> OnMessageSent;
    public event Action<float> OnTimeScaleChanged;

    // Public methods other mods can call
    public bool IsFeatureEnabled { get; private set; }

    // ── Dependency Tracking ───────────────────────────────
    private bool _hasCorePlugin;
    private bool _hasOptionalPlugin;
    private ManualLogSource _logger;

    private void Awake()
    {
        Instance = this;
        _logger = Logger;

        // Check which optional plugins are loaded
        _hasCorePlugin = Chainloader.PluginInfos
            .Any(p => p.Value.Metadata.GUID == "com.example.core");

        _hasOptionalPlugin = Chainloader.PluginInfos
            .Any(p => p.Value.Metadata.GUID == "com.example.optional");

        Logger.LogInfo($"Core plugin: {_hasCorePlugin}");
        Logger.LogInfo($"Optional plugin: {_hasOptionalPlugin}");

        // ── Access Another Plugin's Public API ────────────
        if (_hasOptionalPlugin)
        {
            var optionalInstance = Chainloader.PluginInfos
                .Select(p => p.Value.Instance)
                .OfType<OptionalPluginType>()
                .FirstOrDefault();

            if (optionalInstance != null)
            {
                Logger.LogInfo("Connected to optional plugin!");
                optionalInstance.RegisterCallback(OnOptionalEvent);
            }
        }

        // ── Register with Another Plugin ──────────────────
        if (_hasCorePlugin)
        {
            var coreInstance = Chainloader.PluginInfos
                .Select(p => p.Value.Instance)
                .OfType<CorePluginType>()
                .FirstOrDefault();

            coreInstance?.RegisterModule(this);
        }

        // ── Log All Loaded Plugins ────────────────────────
        Logger.LogInfo("=== Loaded Plugins ===");
        foreach (var plugin in Chainloader.PluginInfos)
        {
            var meta = plugin.Value.Metadata;
            Logger.LogInfo($"  {meta.Name} v{meta.Version} ({meta.GUID})");
        }
    }

    // ── Public API Methods ────────────────────────────────

    public void SendGameMessage(string message)
    {
        OnMessageSent?.Invoke(message);
        Logger.LogInfo($"Message: {message}");
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = Mathf.Clamp(scale, 0f, 5f);
        OnTimeScaleChanged?.Invoke(Time.timeScale);
    }

    // ── Callback from Optional Plugin ─────────────────────
    private void OnOptionalEvent(string data)
    {
        Logger.LogInfo($"Received from optional plugin: {data}");
    }

    private void OnDestroy()
    {
        // Unsubscribe from other plugins to prevent memory leaks
        // optionalInstance?.UnregisterCallback(OnOptionalEvent);
    }
}

// ═══════════════════════════════════════════════════════
// PUBLISHER: Exposing a Clean Public API
// ═══════════════════════════════════════════════════════

// This is how you design your plugin so OTHER mods can use it.
// Keep a stable public interface — other mods depend on it.

[BepInPlugin("com.example.api", "API Provider", "1.0.0")]
public class ApiProviderPlugin : BaseUnityPlugin
{
    public static ApiProviderPlugin Instance { get; private set; }

    // ── Stable Public Interface ──────────────────────────
    // Version this separately from your plugin if needed

    public event Action<PlayerData> OnPlayerSpawned;
    public event Action<PlayerData> OnPlayerDespawned;
    public event Action<BeforeAttack, AfterAttack> OnAttack;

    public bool TryGetPlayer(int playerId, out PlayerData data)
    {
        // Your implementation
        data = null;
        return false;
    }

    public bool TryModifyDamage(ref float damage, DamageModifier modifier)
    {
        // Let subscribers modify damage
        foreach (var handler in OnAttack.GetInvocationList()
            .Cast<Action<BeforeAttack, AfterAttack>>())
        {
            // ... invoke and collect modifications
        }
        return true;
    }

    private void Awake()
    {
        Instance = this;
    }
}

// Public data contracts (don't change these without bumping API version)
public struct PlayerData
{
    public int Id;
    public string Name;
    public Vector3 Position;
    public float Health;
}

public struct BeforeAttack { public int AttackerId; public float BaseDamage; }
public struct AfterAttack { public float FinalDamage; public bool Cancelled; }

// ═══════════════════════════════════════════════════════
// CONFLICT RESOLUTION: Patch Order Management
// ═══════════════════════════════════════════════════════

[BepInPlugin("com.example.conflictresolver", "Conflict Resolver", "1.0.0")]
public class ConflictResolver : BaseUnityPlugin
{
    private readonly Harmony _harmony = new("com.example.conflictresolver");

    private void Awake()
    {
        // ── Check If a Method Is Already Patched ─────────
        var targetMethod = AccessTools.Method(typeof(Player), "TakeDamage");
        var patchInfo = _harmony.GetPatchInfo(targetMethod);

        if (patchInfo.Prefixes.Count > 0 || patchInfo.Postfixes.Count > 0)
        {
            Logger.LogWarning(
                $"TakeDamage already has {patchInfo.Prefixes.Count} prefix(es) " +
                $"and {patchInfo.Postfixes.Count} postfix(es). " +
                $"Our patch will run alongside them.");

            // Log who patched it
            foreach (var prefix in patchInfo.Prefixes)
            {
                Logger.LogInfo($"  Prefix owner: {prefix.Owner}");
            }
        }

        // ── Skip Patching If Already Handled ─────────────
        bool shouldPatch = patchInfo.Prefixes
            .All(p => p.Owner != "com.example.core");

        if (shouldPatch)
        {
            _harmony.PatchAll();
            Logger.LogInfo("Patches applied (no conflict detected)");
        }
        else
        {
            Logger.LogInfo("Skipping — core plugin already handles this");
        }

        // ── Priority-Based Ordering ──────────────────────
        // Higher priority = runs first (for prefixes)
        // Lower priority = runs last (for prefixes, first for postfixes)
    }
}

// Priority examples
[HarmonyPatch(typeof(Player), "TakeDamage")]
static class PriorityExample
{
    // Runs FIRST (highest priority)
    [HarmonyPriority(100)]
    [HarmonyPrefix]
    static void EarlyBlock(Player __instance, ref float damage)
    {
        // Check invincibility first
        if (__instance.isInvincible)
        {
            damage = 0;
        }
    }

    // Runs SECOND (default priority = 0)
    [HarmonyPrefix]
    static void NormalModify(ref float damage)
    {
        // Apply armor reduction
        damage *= 0.8f;
    }

    // Runs LAST (lowest priority)
    [HarmonyPriority(-100)]
    [HarmonyPrefix]
    static void FinalAdjust(ref float damage)
    {
        // Ensure minimum damage
        if (damage < 1) damage = 1;
    }

    // Postfix runs in REVERSE priority order
    [HarmonyPriority(100)]
    [HarmonyPostfix]
    static void LateLog(float damage)
    {
        // This runs LAST among postfixes
        Debug.Log($"Final damage: {damage}");
    }
}

// ═══════════════════════════════════════════════════════
// PATCH ORDER: Before/After Other Mods
// ═══════════════════════════════════════════════════════

[HarmonyPatch(typeof(Player), "Move")]
// Our patches run BEFORE the other mod's patches
[HarmonyPatchBefore("com.example.othermod")]
static class RunBeforeOther
{
    [HarmonyPrefix]
    static void OurPrefix() { /* runs first */ }
}

[HarmonyPatch(typeof(Player), "Move")]
// Our patches run AFTER the other mod's patches
[HarmonyPatchAfter("com.example.othermod")]
static class RunAfterOther
{
    [HarmonyPrefix]
    static void OurPrefix() { /* runs after their prefix */ }
}
