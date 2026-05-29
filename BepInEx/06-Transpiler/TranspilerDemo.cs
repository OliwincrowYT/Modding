// ═══════════════════════════════════════════════════════════
// ADVANCED: Transpilers — IL-Level Code Modification
// Modify the bytecode of methods at runtime.
// This is where you go from "hooking" to "surgery."
// ═══════════════════════════════════════════════════════════

using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[BepInPlugin("com.example.transpiler", "Transpiler Demo", "1.0.0")]
public class TranspilerDemo : BaseUnityPlugin
{
    private readonly Harmony _harmony = new("com.example.transpiler");

    private void Awake()
    {
        _harmony.PatchAll(typeof(TranspilerDemo));
        Logger.LogInfo("Transpiler patches applied.");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }

    // ═══════════════════════════════════════════════════════
    // TECHNIQUE 1: Replace a Constant Value
    // Find `ldc.r4 10.0` and change it to `ldc.r4 999.0`
    // ═══════════════════════════════════════════════════════

    [HarmonyPatch(typeof(SomeGameClass), "CalculateDamage")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ReplaceConstant(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var sorted = instructions.ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            var inst = sorted[i];

            // Find: ldc.r4 0.5f (damage reduction factor)
            if (inst.opcode == OpCodes.Ldc_R4 && (float)inst.operand == 0.5f)
            {
                Logger.LogDebug($"Replaced 0.5f → 1.0f at index {i}");
                sorted[i] = new CodeInstruction(OpCodes.Ldc_R4, 1.0f);
            }
        }

        return sorted;
    }

    // ═══════════════════════════════════════════════════════
    // TECHNIQUE 2: Inject Code After a Specific Instruction
    // Add a Debug.Log() call after a specific line
    // ═══════════════════════════════════════════════════════

    [HarmonyPatch(typeof(Player), "TakeDamage")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> InjectAfter(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var sorted = instructions.ToList();
        var newLocal = generator.DeclareLocal(typeof(string));

        // Find the first `ldstr` (string literal load)
        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].opcode == OpCodes.Ldstr)
            {
                // Inject: Debug.Log($"Patched: {originalString}")
                var inject = new CodeInstruction[]
                {
                    // Push "Patched: " string
                    new CodeInstruction(OpCodes.Ldstr, "Patched: "),

                    // Duplicate the original string (still on stack)
                    new CodeInstruction(OpCodes.Dup),

                    // Call string.Concat
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(string), "Concat",
                            new[] { typeof(object), typeof(object) })),

                    // Call Debug.Log
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Debug), "Log",
                            new[] { typeof(object) })),

                    // NOP to clean up
                    new CodeInstruction(OpCodes.Nop),
                };

                // Insert after the current instruction
                sorted.InsertRange(i + 1, inject);
                break; // Only inject once
            }
        }

        return sorted;
    }

    // ═══════════════════════════════════════════════════════
    // TECHNIQUE 3: Remove an Instruction (Jump Over a Block)
    // Delete a specific check or condition
    // ═══════════════════════════════════════════════════════

    [HarmonyPatch(typeof(LockSystem), "CanOpen")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> RemoveCheck(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var sorted = instructions.ToList();

        // Remove all calls to IsPlayerAuthenticated()
        var authMethod = AccessTools.Method(typeof(LockSystem), "IsPlayerAuthenticated");

        return sorted.Where(inst =>
            !(inst.opcode == OpCodes.Call && inst.operand == authMethod));
    }

    // ═══════════════════════════════════════════════════════
    // TECHNIQUE 4: Match Instruction Patterns (Labels)
    // Use Match() for reliable pattern matching
    // ═══════════════════════════════════════════════════════

    [HarmonyPatch(typeof(Inventory), "AddItem")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PatternMatch(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var sorted = instructions.ToList();

        // Match: ldc.i4.0 → ret  (returning 0)
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].opcode == OpCodes.Ldc_I4_0
                && sorted[i + 1].opcode == OpCodes.Ret)
            {
                // Replace "return 0" with "return 999"
                sorted[i] = new CodeInstruction(OpCodes.Ldc_I4, 999);
                Logger.LogDebug($"Replaced 'return 0' with 'return 999' at {i}");
            }
        }

        return sorted;
    }

    // ═══════════════════════════════════════════════════════
    // TECHNIQUE 5: Branch Manipulation (Change Control Flow)
    // Redirect a jump, neutralize a conditional
    // ═══════════════════════════════════════════════════════

    [HarmonyPatch(typeof(GameManager), "CheckGameOver")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> NeutralizeBranch(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var sorted = instructions.ToList();

        // Find brtrue.s -> GameOver() and replace with nop
        for (int i = 0; i < sorted.Count; i++)
        {
            var inst = sorted[i];

            // brtrue / brfalse → turn into nop (always continue)
            if (inst.opcode == OpCodes.Br_S || inst.opcode == OpCodes.Brtrue)
            {
                // Check if this branch targets the GameOver method call
                if (inst.operand is Label label)
                {
                    // Replace branch with nop
                    sorted[i] = new CodeInstruction(OpCodes.Nop);
                    Logger.LogDebug($"Neutralized branch at {i}");
                }
            }
        }

        return sorted;
    }

    // ═══════════════════════════════════════════════════════
    // TECHNIQUE 6: Full Method Replacement via Transpiler
    // Replace the entire body with your own code
    // ═══════════════════════════════════════════════════════

    [HarmonyPatch(typeof(TargetClass), "TargetMethod")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ReplaceEntireBody(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        // Return completely new IL
        var newLocal = generator.DeclareLocal(typeof(int));

        return new CodeInstruction[]
        {
            // int result = 42;
            new CodeInstruction(OpCodes.Ldc_I4_S, 42),
            new CodeInstruction(OpCodes.Stloc_0),

            // Debug.Log("Method replaced!");
            new CodeInstruction(OpCodes.Ldstr, "Method replaced!"),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(Debug), "Log",
                    new[] { typeof(object) })),

            // return result;
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Ret),
        };
    }

    // ═══════════════════════════════════════════════════════
    // HELPER: Dump IL for Debugging
    // Print the bytecode of any method to the log
    // ═══════════════════════════════════════════════════════

    public static string DumpIL(MethodBase method)
    {
        var module = method.Module;
        var rawIL = method.GetMethodBody()?.GetILAsByteArray();

        if (rawIL == null) return "(no body)";

        var harmony = new Harmony("dump.temp");
        var patchInstructions = Harmony.GetOriginalMethod(method)
            ?.GetInstructions() ?? Enumerable.Empty<CodeInstruction>();

        var result = $"═══ IL for {method.DeclaringType?.Name}.{method.Name} ═══\n";

        int i = 0;
        foreach (var inst in patchInstructions)
        {
            result += $"{i,4}: {inst.opcode.ToString(),-12} {inst.operand ?? ""}\n";
            i++;
        }

        return result;
    }
}

// ═══════════════════════════════════════════════════════
// IL OPCODE QUICK REFERENCE
// ═══════════════════════════════════════════════════════
/*
LOAD OPERATIONS:
  Ldarg_0         Load argument 0 (or 'this' for instance methods)
  Ldarg_1         Load argument 1
  Ldarg_S (byte)  Load argument by index (short form)
  Ldarga          Load address of argument (for ref/out)
  Ldloc_0         Load local variable 0
  Ldloc_1         Load local variable 1
  Ldloc_S (byte)  Load local by index
  Ldloca          Load address of local
  Ldsfld          Load static field
  Ldfld           Load instance field
  Ldstr           Load string literal
  Ldc_I4_0        Push int 0
  Ldc_I4_1        Push int 1
  Ldc_I4_S (byte) Push signed byte as int
  Ldc_I4 (int)    Push 32-bit int constant
  Ldc_R4 (float)  Push float constant
  Ldc_R8 (double) Push double constant
  Ldnull          Push null reference
  Ldobj           Load a value type by copying
  Ldelem          Load an element from an array

STORE OPERATIONS:
  Stloc_0         Store to local 0
  Stloc_1         Store to local 1
  Stloc_S (byte)  Store to local by index
  Stsfld          Store to static field
  Stfld           Store to instance field
  Stelem          Store to array element
  Stobj           Store a value type

CALL OPERATIONS:
  Call            Call a method (static or virtual, non-virtual call)
  Callvirt        Call a virtual method
  Newobj          Create a new object (call constructor)
  Calli           Call a function pointer

BRANCH OPERATIONS:
  Br (label)      Unconditional branch
  Br_S (label)    Short unconditional branch
  Brtrue (label)  Branch if true (non-zero)
  Brfalse (label) Branch if false (zero)
  Beq (label)     Branch if equal
  Bne_Un (label)  Branch if not equal (unsigned)
  Blt (label)     Branch if less than
  Bgt (label)     Branch if greater than

ARITHMETIC:
  Add             Addition
  Sub             Subtraction
  Mul             Multiplication
  Div             Division (signed)
  Div_Un          Division (unsigned)
  Rem             Remainder
  Neg             Negate

CONVERSION:
  Conv_I4         Convert to int32
  Conv_R4         Convert to float
  Conv_R8         Convert to double
  Conv_U4         Convert to uint32
  Castclass       Cast to a reference type
  Isinst          Check if object is an instance of a type

STACK:
  Dup             Duplicate the top value
  Pop             Remove the top value
  Ldobj           Load object
  Initobj         Zero-initialize a value type

CONTROL FLOW:
  Ret             Return from method
  Nop             No operation
  Throw           Throw an exception
  Leave           Exit a try block (jump to finally/catch)
*/
