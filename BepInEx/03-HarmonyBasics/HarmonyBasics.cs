// ═══════════════════════════════════════════════════════════
// INTERMEDIATE: Harmony Patching — The Core of Modding
// Learn Prefix, Postfix, and how to skip/modify originals
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

[BepInPlugin(
    guid: "com.example.harmonbasics",
    name: "Harmony Basics",
    version: "1.0.0"
)]
public class HarmonyBasicsPlugin : BaseUnityPlugin
{
    private static ManualLogSource _logger;
    private readonly Harmony _harmony = new("com.example.harmonbasics");

    private void Awake()
    {
        _logger = Logger;

        // PatchAll() finds all [HarmonyPatch] methods in this assembly
        // and applies them. This is the simplest approach.
        _harmony.PatchAll(typeof(HarmonyBasicsPlugin));

        Logger.LogInfo("Harmony patches applied.");
    }

    private void OnDestroy()
    {
        // Always clean up patches when the plugin unloads
        _harmony.UnpatchSelf();
        Logger.LogInfo("Harmony patches removed.");
    }

    // ═══════════════════════════════════════════════════════
    // EXAMPLE 1: Simple Postfix — Run After the Original
    // ═══════════════════════════════════════════════════════

    // [HarmonyPatch(typeof(TargetClass), "TargetMethod")]
    // tells Harmony which method to patch.
    //
    // Postfix runs AFTER the original method completes.
    // Use it for: logging, reacting to results, side effects.

    [HarmonyPatch(typeof(Application), nameof(Application.Quit))]
    [HarmonyPostfix]
    private static void OnApplicationQuit_Postfix()
    {
        _logger.LogInfo("The game is quitting! (Postfix fired)");
        // You can't prevent Quit() from a postfix (original already ran)
        // Use a Prefix with `return false` to cancel.
    }

    // ═══════════════════════════════════════════════════════
    // EXAMPLE 2: Prefix — Run Before, Optionally Skip Original
    // ═══════════════════════════════════════════════════════

    // Prefix runs BEFORE the original method.
    // Return `false` (for void methods, use `__runOriginal = false`)
    // to prevent the original from executing.

    // Special parameters Harmony injects:
    //   __instance          → the 'this' reference
    //   __result            → the return value (modifiable in postfix)
    //   __runOriginal       → set to false to skip original (in prefix)
    //   __state             → pass data from prefix → postfix
    //   __targetMethod      → the actual MethodBase being patched

    [HarmonyPatch(typeof(Input), nameof(Input.GetKey))]
    [HarmonyPrefix]
    private static bool InputGetKey_Prefix(
        KeyCode key,
        ref bool __result,
        ref bool __runOriginal)
    {
        // Example: remap Left Control → Space
        if (key == KeyCode.LeftControl)
        {
            __result = Input.GetKeyDown(KeyCode.Space);
            __runOriginal = false; // Skip the original check
            return false;
        }

        // For all other keys, let the original run
        return true;
    }

    // ═══════════════════════════════════════════════════════
    // EXAMPLE 3: Prefix + Postfix with __state Communication
    // ═══════════════════════════════════════════════════════

    // Use __state (object) to pass data from prefix to postfix.
    // This is how you coordinate behavior across both hooks.

    [HarmonyPatch(typeof(Time), nameof(Time.timeScale), nameof(Time.set_timeScale))]
    [HarmonyPrefix]
    private static void TimeScaleSet_Prefix(
        float value,
        ref object __state)
    {
        // Capture the requested value before it's applied
        __state = new
        {
            Requested = value,
            Previous = Time.timeScale
        };
    }

    [HarmonyPatch(typeof(Time), nameof(Time.timeScale), nameof(Time.set_timeScale))]
    [HarmonyPostfix]
    private static void TimeScaleSet_Postfix(
        float value,
        ref object __state)
    {
        var state = (dynamic)__state;
        _logger.LogInfo(
            $"Time scale: {state.Previous:F3} → {state.Requested:F3}");
    }

    // ═══════════════════════════════════════════════════════
    // EXAMPLE 4: Modifying Return Values
    // ═══════════════════════════════════════════════════════

    // In a postfix, __result holds the original return value.
    // Modify it to change what the caller receives.

    [HarmonyPatch(typeof(Mathf), nameof(Mathf.Clamp),
        new[] { typeof(float), typeof(float), typeof(float) })]
    [HarmonyPostfix]
    private static void MathfClamp_Postfix(
        ref float __result,
        float value, float min, float max)
    {
        // Example: log extreme clamps
        if (value < min || value > max)
        {
            _logger.LogDebug($"Clamped {value} → {__result} (range: {min}-{max})");
        }
    }

    // ═══════════════════════════════════════════════════════
    // EXAMPLE 5: Patching by Delegate (Overload-Safe)
    // ═══════════════════════════════════════════════════════

    // When a method has overloads, AccessTools needs parameter types.
    // Alternatively, use a delegate to let the compiler infer types.

    private void SetupDelegatePatch()
    {
        // Instead of:
        //   AccessTools.Method(typeof(Debug), "Log",
        //       new[] { typeof(object), typeof(Component) })
        // Use a delegate:
        var original = ((Action<object, Component>)(
            (msg, context) => Debug.Log(msg, context))).Method;

        _harmony.Patch(original,
            prefix: new HarmonyMethod(typeof(DebugLog_Prefix)));
    }

    private static void DebugLog_Prefix(ref object message)
    {
        // Prepend mod tag to all Debug.Log calls
        message = $"[MOD] {message}";
    }

    // ═══════════════════════════════════════════════════════
    // EXAMPLE 6: Conditional Patching
    // ═══════════════════════════════════════════════════════

    // Only apply patches when a condition is met.
    // Check BEFORE applying, not inside the patch (performance).

    private void ApplyConditionalPatches()
    {
        var gameVersion = Application.version;

        if (gameVersion.StartsWith("1."))
        {
            _logger.LogInfo("Game v1.x detected — applying v1 patches");
            // Apply v1-specific patches
        }
        else if (gameVersion.StartsWith("2."))
        {
            _logger.LogInfo("Game v2.x detected — applying v2 patches");
            // Apply v2-specific patches
        }
        else
        {
            _logger.LogWarning($"Unknown game version: {gameVersion}");
        }
    }
}

// ═══════════════════════════════════════════════════════
// PATCH CLASS ORGANIZATION
// ═══════════════════════════════════════════════════════

// Patches can live in separate static classes for organization.
// Each class handles one method's patches.

[HarmonyPatch(typeof(Screen), nameof(Screen.width))]
[HarmonyGet]
static class ScreenWidth_Patch
{
    [HarmonyPostfix]
    private static void Postfix(ref int __result)
    {
        // Example: force a minimum screen width
        if (__result < 1280) __result = 1280;
    }
}
