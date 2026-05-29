// ============================================================
// Example 03: Harmony Patching (Intermediate)
// ============================================================
// Demonstrates:
// - Prefix patches (run before original method)
// - Postfix patches (run after original method)
// - Transpiler patches (modify IL bytecode)
// - Patch priority and stack priority
// - AccessTools for finding methods
//
// Harmony is MelonLoader's built-in patching framework
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;

namespace HarmonyPatchingExample
{
    [assembly: MelonInfo_name("Harmony Patching Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates Harmony patching techniques")]

    public class HarmonyPatchingExample : MelonMod
    {
        private static readonly Harmony _harmony = new Harmony("com.example.harmonypatch");

        override public void OnApplicationStart()
        {
            MelonLogger.Msg("Applying Harmony patches...");

            // Option 1: Patch by method name (simplest)
            PatchPlayerHealth();

            // Option 2: Patch using AccessTools (more reliable)
            PatchPlayerMovement();

            // Option 3: Patch all methods matching a pattern
            PatchAllDamageMethods();

            MelonLogger.Msg("All Harmony patches applied successfully!");
        }

        // =========================================================
        // PREFIX PATCH - Run BEFORE the original method
        // =========================================================
        // Use 'ref return' or '__result' to override the return value
        // Return false to skip the original method entirely
        // =========================================================

        /// <summary>
        /// Example: Modify player health before it's set
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "SetHealth")]
        [HarmonyPriority(100)] // Lower number = higher priority
        private static bool PrefixSetHealth(
            ref float newHealth,
            ref __Result result)
        {
            MelonLogger.Msg($"[Prefix] SetHealth called with: {newHealth}");

            // Modify the input parameter
            newHealth = Mathf.Clamp(newHealth, 0, 200); // Cap health at 200

            // Optionally skip original and set result directly
            // result = true;
            // return false;

            // Return true to continue with original method
            return true;
        }

        // =========================================================
        // POSTFIX PATCH - Run AFTER the original method
        // =========================================================
        // Access '__result' to see/modify the return value
        // Cannot prevent the original method from running
        // =========================================================

        /// <summary>
        /// Example: Log health after it's been set
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "SetHealth")]
        [HarmonyPriority(50)] // Lower number = runs first among same type
        private static void PostfixSetHealth(
            float newHealth,
            ref __Result result)
        {
            MelonLogger.Msg($"[Postfix] Health is now: {newHealth}");

            // Modify the return value
            // result = !result; // Flip the boolean result
        }

        // =========================================================
        // TRANSPILER PATCH - Modify the IL bytecode directly
        // =========================================================
        // Most powerful but also most complex
        // Can replace, remove, or inject instructions
        // =========================================================

        /// <summary>
        /// Example: Replace a hardcoded value with a dynamic one
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "CalculateDamage")]
        private static IEnumerable<CodeInstruction> TranspilerCalculateDamage(
            IEnumerable<CodeInstruction> instructions,
            MethodBase originalMethod)
        {
            var newInstructions = new List<CodeInstruction>(instructions);

            // Find the instruction that loads the hardcoded damage value
            var targetInstruction = newInstructions
                .Where(i => i.matches(new CodeMatch(OpCodes.Ldc_R4, 10f)))
                .FirstOrDefault();

            if (targetInstruction != null)
            {
                // Replace with our custom damage value
                targetInstruction.opcode = OpCodes.Ldc_R4;
                targetInstruction.operand = 25f; // New damage value

                MelonLogger.Msg($"[Transpiler] Replaced damage value with 25f");
            }
            else
            {
                MelonLogger.Msg("[Transpiler] Target instruction not found");
            }

            return newInstructions;
        }

        // =========================================================
        // ACCESS TOOLS - Find methods/fields safely
        // =========================================================

        private void PatchPlayerMovement()
        {
            // Find the method dynamically
            var method = AccessTools.Method(
                "Player:Move(Vector3)",
                new Type[] { typeof(Vector3) });

            if (method != null)
            {
                // Apply patch programmatically
                _harmony.Patch(
                    method,
                    prefix: new HarmonyMethod(typeof(HarmonyPatchingExample), nameof(PrefixMove)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatchingExample), nameof(PostfixMove)));

                MelonLogger.Msg("Patched Player.Move(Vector3)");
            }
            else
            {
                MelonLogger.Warning("Could not find Player.Move(Vector3)");
            }
        }

        private static void PrefixMove(ref Vector3 direction)
        {
            // Apply speed boost
            direction *= 1.5f;
        }

        private static void PostfixMove()
        {
            // Log movement
            MelonLogger.Msg("[Postfix] Player moved");
        }

        // =========================================================
        // PATCH MULTIPLE METHODS
        // =========================================================

        private void PatchAllDamageMethods()
        {
            // Patch all methods in a class that start with "TakeDamage"
            var playerType = AccessTools.TypeByName("Player");

            if (playerType != null)
            {
                var damageMethods = playerType.GetMethods()
                    .Where(m => m.Name.StartsWith("TakeDamage"));

                foreach (var method in damageMethods)
                {
                    _harmony.Patch(
                        method,
                        prefix: new HarmonyMethod(typeof(HarmonyPatchingExample), nameof(PrefixTakeDamage)));

                    MelonLogger.Msg($"Patched: {method.Name}");
                }
            }
        }

        private static void PrefixTakeDamage(ref float damage)
        {
            // Apply armor reduction
            damage *= 0.8f;
        }

        // =========================================================
        // CLEANUP
        // =========================================================

        override public void OnApplicationQuit()
        {
            // Unpatch all methods patched by this Harmony instance
            _harmony.UnpatchAll(_harmony.Id);
            MelonLogger.Msg("All Harmony patches unpatched");
        }
    }
}
