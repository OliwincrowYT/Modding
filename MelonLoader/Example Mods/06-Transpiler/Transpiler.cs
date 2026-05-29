// ============================================================
// Example 06: Opcode Transpiler (Advanced)
// ============================================================
// Demonstrates:
// - IL opcode manipulation
// - Replacing values in bytecode
// - Injecting new instructions
// - Removing instructions
// - Reading and modifying method behavior at IL level
//
// Transpilers modify the method's bytecode before it's JIT compiled
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TranspilerExample
{
    [assembly: MelonInfo_name("Transpiler Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates IL opcode transpilation")]

    public class TranspilerExample : MelonMod
    {
        private static readonly Harmony _harmony = new Harmony("com.example.transpiler");

        override public void OnApplicationStart()
        {
            MelonLogger.Msg("Applying transpiler patches...");

            // Apply transpilers
            ApplyTranspilers();

            MelonLogger.Msg("Transpiler patches applied!");
        }

        private void ApplyTranspilers()
        {
            // Example 1: Replace a hardcoded value
            PatchHardcodedValue();

            // Example 2: Inject logging code
            InjectMethodLogger();

            // Example 3: Remove a specific instruction
            RemoveInstruction();

            // Example 4: Complex transformation
            ComplexTransformation();
        }

        // =========================================================
        // Example 1: Replace hardcoded values
        // =========================================================

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Enemy), "GetDamage")]
        private static IEnumerable<CodeInstruction> ReplaceHardcodedDamage(
            IEnumerable<CodeInstruction> instructions,
            MethodBase originalMethod)
        {
            var newInstructions = new List<CodeInstruction>();

            foreach (var instruction in instructions)
            {
                // Find and replace Ldc_R4 (load float constant)
                if (instruction.opcode == OpCodes.Ldc_R4)
                {
                    float oldValue = (float)instruction.operand;
                    float newValue = oldValue * 2f; // Double all float constants

                    newInstructions.Add(new CodeInstruction(
                        OpCodes.Ldc_R4,
                        newValue));

                    MelonLogger.Msg($"[Transpiler] Replaced float: {oldValue} -> {newValue}");
                    continue;
                }

                // Find and replace Ldc_I4 (load int constant)
                if (instruction.opcode == OpCodes.Ldc_I4)
                {
                    int oldValue = (int)instruction.operand;
                    int newValue = oldValue * 2; // Double all int constants

                    newInstructions.Add(new CodeInstruction(
                        OpCodes.Ldc_I4,
                        newValue));

                    MelonLogger.Msg($"[Transpiler] Replaced int: {oldValue} -> {newValue}");
                    continue;
                }

                newInstructions.Add(instruction);
            }

            return newInstructions;
        }

        // =========================================================
        // Example 2: Inject code at method entry
        // =========================================================

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "TakeDamage")]
        private static IEnumerable<CodeInstruction> InjectDamageLogger(
            IEnumerable<CodeInstruction> instructions,
            MethodBase originalMethod)
        {
            var newInstructions = new List<CodeInstruction>(instructions);

            // Find the first instruction
            var firstInstruction = newInstructions.FirstOrDefault();

            if (firstInstruction != null)
            {
                // Inject code to log damage taken
                // This calls: MelonLogger.Msg("Damage taken!");

                // Load string literal
                newInstructions.Insert(0,
                    new CodeInstruction(OpCodes.Ldstr, "Damage taken!"));

                // Call MelonLogger.Msg(string)
                var logMethod = AccessTools.Method(
                    typeof(MelonLogger),
                    nameof(MelonLogger.Msg));

                newInstructions.Insert(1,
                    new CodeInstruction(OpCodes.Call, logMethod));

                // Pop the return value if any
                newInstructions.Insert(2,
                    new CodeInstruction(OpCodes.Pop));

                MelonLogger.Msg("[Transpiler] Injected damage logger");
            }

            return newInstructions;
        }

        // =========================================================
        // Example 3: Remove specific instructions
        // =========================================================

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "CheckDeath")]
        private static IEnumerable<CodeInstruction> RemoveDeathCheck(
            IEnumerable<CodeInstruction> instructions,
            MethodBase originalMethod)
        {
            var newInstructions = new List<CodeInstruction>();

            foreach (var instruction in instructions)
            {
                // Skip instructions that call a specific method
                if (instruction.opcode == OpCodes.Call)
                {
                    var method = instruction.operand as MethodBase;
                    if (method != null && method.Name == "TriggerDeathAnimation")
                    {
                        MelonLogger.Msg("[Transpiler] Removed death animation call");
                        continue; // Skip this instruction
                    }
                }

                // Skip branch to a specific label
                if (instruction.opcode == OpCodes.Br)
                {
                    var target = instruction.operand as Instruction;
                    if (target != null && target.opcode == OpCodes.Ret)
                    {
                        MelonLogger.Msg("[Transpiler] Removed branch to return");
                        continue;
                    }
                }

                newInstructions.Add(instruction);
            }

            return newInstructions;
        }

        // =========================================================
        // Example 4: Complex transformation
        // =========================================================

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameManager), "CalculateScore")]
        private static IEnumerable<CodeInstruction> ComplexScoreModification(
            IEnumerable<CodeInstruction> instructions,
            MethodBase originalMethod)
        {
            var newInstructions = new List<CodeInstruction>(instructions);

            // Find all multiplication instructions
            var multiplyInstructions = newInstructions
                .Where(i => i.opcode == OpCodes.Mul)
                .ToList();

            if (multiplyInstructions.Count > 0)
            {
                // Replace the last multiplication with our custom calculation
                var lastMul = multiplyInstructions.Last();

                // Remove the multiply
                newInstructions.Remove(lastMul);

                // Insert custom score calculation:
                // score = score * 2 + 100

                // Duplicate the score value (assuming it's on the stack)
                newInstructions.Insert(
                    newInstructions.IndexOf(lastMul),
                    new CodeInstruction(OpCodes.Dup));

                // Multiply by 2
                newInstructions.Insert(
                    newInstructions.IndexOf(lastMul) + 1,
                    new CodeInstruction(OpCodes.Ldc_R4, 2f));

                newInstructions.Insert(
                    newInstructions.IndexOf(lastMul) + 2,
                    new CodeInstruction(OpCodes.Mul));

                // Add 100
                newInstructions.Insert(
                    newInstructions.IndexOf(lastMul) + 3,
                    new CodeInstruction(OpCodes.Ldc_R4, 100f));

                newInstructions.Insert(
                    newInstructions.IndexOf(lastMul) + 4,
                    new CodeInstruction(OpCodes.Add));

                MelonLogger.Msg("[Transpiler] Applied complex score modification");
            }

            return newInstructions;
        }

        // =========================================================
        // Helper: Print IL instructions for debugging
        // =========================================================

        public static void PrintIL(MethodBase method)
        {
            MelonLogger.Msg($"=== IL for {method.Name} ===");

            var instructions = method.GetInstructions();

            for (int i = 0; i < instructions.Length; i++)
            {
                var instr = instructions[i];
                string operand = instr.operand != null
                    ? $" {instr.operand}"
                    : "";

                MelonLogger.Msg($"  [{i:D3}] {instr.opcode}{operand}");
            }
        }

        override public void OnApplicationQuit()
        {
            _harmony.UnpatchAll(_harmony.Id);
            MelonLogger.Msg("Transpiler patches unpatched");
        }
    }
}
