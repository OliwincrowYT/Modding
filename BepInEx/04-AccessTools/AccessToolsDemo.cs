// ═══════════════════════════════════════════════════════════
// INTERMEDIATE: AccessTools — Finding & Using Game Code
// AccessTools is Harmony's reflection utility — your #1 tool
// for navigating obfuscated or private game code.
// ═══════════════════════════════════════════════════════════

using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[BepInPlugin("com.example.accesstools", "AccessTools Demo", "1.0.0")]
public class AccessToolsDemo : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo("=== AccessTools Examples ===");

        DemonstrateTypeLookup();
        DemonstrateMethodLookup();
        DemonstrateFieldAccess();
        DemonstratePropertyAccess();
        DemonstrateDelegateCreation();
        DemonstrateGenericMethods();
        DemonstrateNestedTypes();
        DemonstrateSearchPatterns();
    }

    // ── 1. Type Lookup ─────────────────────────────────────

    private void DemonstrateTypeLookup()
    {
        // By full name (namespace + class)
        Type playerType = AccessTools.TypeByName("Game.Player");

        // By partial name (searches all assemblies)
        Type anyType = AccessTools.TypeByName("Player");

        // From a known type's assembly
        Type inAssembly = AccessTools.TypeInAssembly(
            typeof(KnownType), "TargetClassName");

        // All types in an assembly
        Type[] allTypes = AccessTools.AllTypes()
            .Where(t => t.Name.Contains("Enemy"))
            .ToArray();

        Logger.LogInfo($"Found {allTypes.Length} types with 'Enemy' in the name");
    }

    // ── 2. Method Lookup ───────────────────────────────────

    private void DemonstrateMethodLookup()
    {
        Type t = typeof(MonoBehaviour); // or any game type

        // Simple lookup by name
        MethodBase m1 = AccessTools.Method(t, "Start");

        // With parameter types (disambiguates overloads)
        MethodBase m2 = AccessTools.Method(t, "GetComponent",
            new Type[] { });

        // Static method
        MethodBase m3 = AccessTools.Method(t, "GetInstanceID");

        // Private method
        MethodBase m4 = AccessTools.Method(t, "<>nrig");

        // All declared methods (not inherited)
        MethodBase[] declared = AccessTools.GetDeclaredMethods(t);

        // All methods (including inherited)
        MethodBase[] all = AccessTools.GetMethods(t);

        // By delegate (compiler infers parameter types)
        MethodBase m5 = ((Action<string>)Debug.Log).Method;

        // Extension method
        MethodBase ext = AccessTools.Method(
            typeof(UnityEngine.GameObjectExtensions), "SetActiveRecursively");
    }

    // ── 3. Field Access ────────────────────────────────────

    private void DemonstrateFieldAccess()
    {
        Type t = typeof(Vector3);

        // Get a field
        FieldInfo xField = AccessTools.Field(t, "x");
        FieldInfo privateField = AccessTools.Field(typeof(Transform), "<position>k__BackingField");

        // Read a field value
        Vector3 pos = new Vector3(1, 2, 3);
        float x = (float)xField.GetValue(pos); // x = 1

        // Write a field value (even private!)
        FieldInfo healthField = AccessTools.Field(typeof(Player), "_health");
        // healthField.SetValue(playerInstance, 999f);

        // Static fields
        FieldInfo staticField = AccessTools.Field(typeof(Time), "timeScale");
        float currentScale = (float)staticField.GetValue(null);

        // Set static field
        staticField.SetValue(null, 0.5f); // Slow motion!
    }

    // ── 4. Property Access ─────────────────────────────────

    private void DemonstratePropertyAccess()
    {
        PropertyInfo healthProp = AccessTools.Property(typeof(Player), "Health");

        // Read
        float health = (float)healthProp.GetValue(playerInstance);

        // Write
        healthProp.SetValue(playerInstance, 999f);

        // Get the underlying getter/setter methods
        MethodInfo getter = healthProp.GetGetMethod(true); // true = include non-public
        MethodInfo setter = healthProp.GetSetMethod(true);
    }

    // ── 5. Delegate Creation ───────────────────────────────

    private void DemonstrateDelegateCreation()
    {
        // Turn any method (even private) into a callable delegate
        // This is MUCH faster than MethodInfo.Invoke()

        Type t = typeof(SomeGameClass);

        // Private method: internal bool CheckWinCondition()
        MethodInfo checkWin = AccessTools.Method(t, "CheckWinCondition");

        // Create a delegate
        Func<bool> checkWinDelegate = checkWin.CreateDelegate<Func<bool>>();

        // Call it (on static methods)
        bool isWin = checkWinDelegate();

        // Instance method: void TakeDamage(float amount)
        MethodInfo takeDamage = AccessTools.Method(t, "TakeDamage",
            new[] { typeof(float) });

        Action<float> damageDelegate = takeDamage.CreateDelegate<Action<SomeGameClass, float>>();
        // damageDelegate(instance, 50f);
    }

    // ── 6. Generic Methods ─────────────────────────────────

    private void DemonstrateGenericMethods()
    {
        // GetComponent<T>() is generic — AccessTools handles it
        MethodInfo getComponent = AccessTools.Method(
            typeof(Component), "GetComponent");

        // Make it specific: GetComponent<Transform>()
        MethodInfo getTransform = getComponent.MakeGenericMethod(typeof(Transform));

        // Or use AccessTools.Method directly with generic arguments
        MethodInfo getCollider = AccessTools.Method(
            typeof(Component), "GetComponent",
            new Type[] { }, // method type args
            new Type[] { typeof(Collider) }); // constructor type args
    }

    // ── 7. Nested Types ────────────────────────────────────

    private void DemonstrateNestedTypes()
    {
        // Nested class: OuterClass.InnerClass
        // In reflection: "OuterClass+InnerClass"

        Type inner = AccessTools.TypeByName("Game.Player+State");

        // Or from the outer type
        Type outer = AccessTools.TypeByName("Game.Player");
        Type inner2 = AccessTools.Inner(outer, "State");

        // Nested enum
        Type enumType = AccessTools.Inner(outer, "MovementMode");

        // Deeply nested: A.B.C
        Type deep = AccessTools.TypeByName("Game+Systems+Inventory+Item");
    }

    // ── 8. Search Patterns ─────────────────────────────────

    private void DemonstrateSearchPatterns()
    {
        // Find all types matching a pattern
        var allPlayerTypes = AccessTools.AllTypes()
            .Where(t => t.Name.StartsWith("Player") && t.IsClass)
            .ToList();

        // Find methods by return type
        var allBoolMethods = AccessTools.GetDeclaredMethods(typeof(GameManager))
            .Where(m => m.ReturnType == typeof(bool))
            .ToList();

        // Find methods that take a specific parameter
        var damageTakers = AccessTools.AllTypes()
            .SelectMany(t => AccessTools.GetDeclaredMethods(t))
            .Where(m => m.Name.Contains("Damage") &&
                        m.GetParameters().Any(p => p.ParameterType == typeof(float)))
            .ToList();

        // Find fields of a specific type
        var allHealthFields = AccessTools.AllTypes()
            .SelectMany(t => AccessTools.GetDeclaredFields(t))
            .Where(f => f.FieldType == typeof(float) &&
                        f.Name.Contains("health", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Logger.LogInfo($"Found {allHealthFields.Length} health-related fields");
    }

    // ── 9. Making Inaccessible Members Accessible ──────────

    private void DemonstrateNonPublicAccess()
    {
        // GetDeclaredMethods/Fields with nonPublic flag
        var privateMethods = typeof(MonoBehaviour).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        // AccessTools always includes non-public by default
        var allMethods = AccessTools.GetDeclaredMethods(typeof(MonoBehaviour));

        // Force a private constructor
        Type t = AccessTools.TypeByName("Game.Singleton");
        ConstructorInfo ctor = AccessTools.Constructor(t, Type.EmptyTypes);
        var instance = ctor.Invoke(new object[] { });
    }

    // ── 10. PatchInfo — Inspecting Existing Patches ────────

    private void DemonstratePatchInfo()
    {
        var harmony = new Harmony("com.example.inspector");
        MethodBase target = AccessTools.Method(typeof(Debug), "Log",
            new[] { typeof(object) });

        var patchInfo = harmony.GetPatchInfo(target);

        Logger.LogInfo($"Prefixes: {patchInfo.Prefixes.Count}");
        Logger.LogInfo($"Postfixes: {patchInfo.Postfixes.Count}");
        Logger.LogInfo($"Transpilers: {patchInfo.Transpilers.Count}");
        Logger.LogInfo($"Finalizers: {patchInfo.Finalizers.Count}");

        // Check who patched this
        foreach (var prefix in patchInfo.Prefixes)
        {
            Logger.LogInfo($"  Prefix by: {prefix.Owner}");
        }
    }
}
