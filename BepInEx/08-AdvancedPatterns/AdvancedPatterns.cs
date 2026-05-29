// ═══════════════════════════════════════════════════════════
// EXPERT: Advanced Patterns & Techniques
// Dynamic methods, finalizers, IL injection, native interop
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[BepInPlugin("com.example.advanced", "Advanced Patterns", "1.0.0")]
public class AdvancedPatterns : BaseUnityPlugin
{
    private readonly Harmony _harmony = new("com.example.advanced");
    private static ManualLogSource _logger;

    private void Awake()
    {
        _logger = Logger;
        SetupAllPatterns();
    }

    private void OnDestroy() => _harmony.UnpatchSelf();

    private void SetupAllPatterns()
    {
        DemonstrateFinalizer();
        DemonstrateDynamicMethod();
        DemonstrateCoroutineInjection();
        DemonstrateSceneManagement();
        DemonstrateResourceIntercept();
        DemonstrateNativeInterop();
    }

    // ═══════════════════════════════════════════════════════
    // PATTERN 1: Finalizer — Exception Handling Wrapper
    // Wraps the entire patched method in try/catch
    // ═══════════════════════════════════════════════════════

    private void DemonstrateFinalizer()
    {
        // Finalizers catch ANY exception from the original method
        // or from prefixes/postfixes. Use for crash prevention.

        var original = AccessTools.Method(typeof(UnsafeClass), "DangerousMethod");
        var finalizer = AccessTools.Method(typeof(FinalizerPatches), nameof(FinalizerPatches.CatchAll));

        _harmony.Patch(original, finalizer: new HarmonyMethod(finalizer));
    }
}

static class FinalizerPatches
{
    // __exception is null if no exception occurred
    [HarmonyFinalizer]
    private static void CatchAll(Exception __exception)
    {
        if (__exception != null)
        {
            // Log but don't crash
            Debug.LogError($"[MOD] Caught exception in patched method: {__exception}");

            // Optionally: show a user-friendly message
            // GUIHelper.ShowError("A mod caught an error. Game continues.");
        }
    }
}

// ═══════════════════════════════════════════════════════
// PATTERN 2: Dynamic Method Generation
// Create patch methods at runtime — no static class needed
// ═══════════════════════════════════════════════════════

static class DynamicMethodDemo
{
    public static void Apply(Harmony harmony, MethodBase original)
    {
        // Create a dynamic prefix method
        var prefix = new DynamicMethod(
            name: $"prefix_{original.Name}",
            returnType: typeof(void),
            parameters: original.GetParameters()
                .Select(p => p.ParameterType)
                .Prepend(original.IsStatic
                    ? Type.EmptyTypes
                    : new[] { original.DeclaringType })
                .ToArray(),
            declaringType: original.DeclaringType,
            skipVisibility: true); // Can access private members

        var il = prefix.GetILGenerator();

        // Emit: Debug.Log($"[Dynamic] Entering {original.Name}")
        il.Emit(OpCodes.Ldstr, $"[Dynamic] Entering {original.Name}");
        il.Emit(OpCodes.Call, AccessTools.Method(typeof(Debug), "Log",
            new[] { typeof(object) }));
        il.Emit(OpCodes.Ret);

        harmony.Patch(original, prefix: new HarmonyMethod(prefix));
    }
}

// ═══════════════════════════════════════════════════════
// PATTERN 3: Coroutine Injection
// Spawn coroutines from patches to do async work
// ═══════════════════════════════════════════════════════

[HarmonyPatch(typeof(Player), "Start")]
[HarmonyPostfix]
static void InjectCoroutine(Player __instance)
{
    // Start a coroutine on the patched object's MonoBehaviour
    var behaviour = __instance.GetComponent<Behaviour>();
    // behaviour.StartCoroutine(DeferredInit(__instance));
}

static IEnumerator DeferredInit(Player player)
{
    // Wait for the first frame
    yield return null;

    // Wait for a specific condition
    while (player.isLoaded == false)
    {
        yield return new WaitForSeconds(0.1f);
    }

    // Now do stuff
    Debug.Log($"Player {player.name} is fully loaded!");
}

// ═══════════════════════════════════════════════════════
// PATTERN 4: Scene Management Hooks
// React to scene loads, inject objects per-scene
// ═══════════════════════════════════════════════════════

[HarmonyPatch(typeof(SceneManager), "LoadScene",
    new[] { typeof(string), typeof(SceneLoadMode) })]
[HarmonyPrefix]
static void OnSceneLoad(string sceneName, SceneLoadMode mode)
{
    Debug.Log($"[MOD] Loading scene: {sceneName} ({mode})");

    switch (sceneName)
    {
        case "MainMenu":
            // Inject menu modifications
            break;
        case "Gameplay":
            // Inject gameplay modifications
            break;
    }
}

[HarmonyPatch(typeof(SceneManager), "sceneLoaded")]
[HarmonyPrefix]
static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    Debug.Log($"[MOD] Scene loaded: {scene.name}");

    // Spawn persistent objects
    var DontDestroy = SceneManager.GetSceneByName("DontDestroyOnLoad");
}

// ═══════════════════════════════════════════════════════
// PATTERN 5: Resource Loading Intercept
// Hook asset/resource loading to modify at load time
// ═══════════════════════════════════════════════════════

[HarmonyPatch(typeof(Resources), "Load",
    new[] { typeof(string) })]
[HarmonyPostfix]
static void OnResourceLoad(string path, ref Object __result)
{
    // Intercept specific resource loads
    if (path == "Textures/Background" && __result is Texture2D tex)
    {
        // Modify the loaded texture
        var pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Invert colors
            pixels[i] = new Color(
                1f - pixels[i].r,
                1f - pixels[i].g,
                1f - pixels[i].b,
                pixels[i].a);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        Debug.Log($"[MOD] Modified texture: {path}");
    }
}

// ═══════════════════════════════════════════════════════
// PATTERN 6: Native Interop (P/Invoke)
// Call Windows/Linux system APIs directly
// ═══════════════════════════════════════════════════════

static class NativeInterop
{
    // ── Windows Console ──────────────────────────────────
    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(uint nStdHandle);

    [DllImport("kernel32.dll")]
    public static extern bool SetStdHandle(uint nStdHandle, IntPtr hHandle);

    // ── File I/O (msvcrt) ───────────────────────────────
    [DllImport("msvcrt.dll")]
    public static extern IntPtr fopen(string filename, string mode);

    [DllImport("msvcrt.dll")]
    public static extern int fprintf(IntPtr stream, string format, int value);

    [DllImport("msvcrt.dll")]
    public static extern int fclose(IntPtr stream);

    // ── Usage ────────────────────────────────────────────
    public static void EnableConsole()
    {
        if (GetConsoleWindow() == IntPtr.Zero)
        {
            AllocConsole();
            // Redirect stdout/stderr to console
            var consoleHandle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
        }
    }

    public static void DisableConsole()
    {
        FreeConsole();
    }
}

// ═══════════════════════════════════════════════════════
// PATTERN 7: Reversible Patch Manager
// Apply/undo patches at runtime (e.g., per-scene)
// ═══════════════════════════════════════════════════════

public class ReversiblePatchManager
{
    private readonly Harmony _harmony;
    private readonly List<PatchRecord> _records = new();

    public ReversiblePatchManager(string guid)
    {
        _harmony = new Harmony(guid);
    }

    public void Apply(MethodBase original,
        MethodBase prefix = null,
        MethodBase postfix = null,
        MethodBase transpiler = null)
    {
        _harmony.Patch(original,
            prefix: prefix != null ? new HarmonyMethod(prefix) : null,
            postfix: postfix != null ? new HarmonyMethod(postfix) : null,
            transpiler: transpiler != null ? new HarmonyMethod(transpiler) : null);

        if (prefix != null) _records.Add(new PatchRecord(original, HarmonyPatchType.Prefix, prefix));
        if (postfix != null) _records.Add(new PatchRecord(original, HarmonyPatchType.Postfix, postfix));
        if (transpiler != null) _records.Add(new PatchRecord(original, HarmonyPatchType.Transpiler, transpiler));
    }

    public void UndoAll()
    {
        foreach (var record in _records)
        {
            _harmony.Unpatch(record.Original, record.Type, record.Patch.MethodHandle.GetFunctionPointer());
        }
        _records.Clear();
    }

    public int Count => _records.Count;

    private readonly struct PatchRecord
    {
        public readonly MethodBase Original;
        public readonly HarmonyPatchType Type;
        public readonly MethodBase Patch;
        public PatchRecord(MethodBase o, HarmonyPatchType t, MethodBase p)
        {
            Original = o; Type = t; Patch = p;
        }
    }
}

// ═══════════════════════════════════════════════════════
// PATTERN 8: Field Watcher (Property Change Detection)
// Detect when a field changes without patching setters
// ═══════════════════════════════════════════════════════

public class FieldWatcher : MonoBehaviour
{
    private readonly Dictionary<FieldInfo, object> _watched = new();
    private readonly Dictionary<FieldInfo, Action<object, object>> _callbacks = new();

    public void Watch(object target, FieldInfo field, Action<object, object> onChanged)
    {
        _watched[field] = field.GetValue(target);
        _callbacks[field] = onChanged;
    }

    private void Update()
    {
        foreach (var kvp in _watched)
        {
            var current = kvp.Key.GetValue(null); // static fields
            if (!Equals(current, kvp.Value))
            {
                _callbacks[kvp.Key]?.Invoke(kvp.Value, current);
                _watched[kvp.Key] = current;
            }
        }
    }

    private void OnDestroy()
    {
        _watched.Clear();
        _callbacks.Clear();
    }
}

// ═══════════════════════════════════════════════════════
// PATTERN 9: Assembly Redirect
// Intercept type resolution for dependency injection
// ═══════════════════════════════════════════════════════

// In BepInEx, use the `patchers` folder for pre-load assembly modification.
// This runs BEFORE the game loads any assemblies.

// BepInEx/patchers/MyPatcher.cs:
/*
using BepInEx;
using Mono.Cecil;
using Mono.Cecil.Cil;

[BepInProcess("MyGame.exe")]
public class MyAssemblyPatcher : IAssemblyPreProcessor
{
    public bool Process(string path, AssemblyDefinition assembly)
    {
        // Modify the assembly before it's loaded
        if (assembly.Name.Name == "Assembly-CSharp")
        {
            // Find a type
            var type = assembly.Types.FirstOrDefault(t => t.Name == "Player");
            if (type != null)
            {
                // Add a new field
                var field = new FieldDefinition("_modHealth",
                    FieldAttributes.Private,
                    assembly.MainModule.TypeSystem.Float);
                type.Fields.Add(field);
            }
            return true; // Assembly was modified
        }
        return false;
    }
}
*/

// ═══════════════════════════════════════════════════════
// PATTERN 10: Object Pooling (Performance)
// Eliminate GC pressure in hot paths
// ═══════════════════════════════════════════════════════

public static class ObjectPool<T> where T : new()
{
    private static readonly Stack<T> _pool = new();

    public static T Get()
    {
        if (_pool.TryPop(out var item))
            return item;
        return new T();
    }

    public static void Return(T item)
    {
        _pool.Push(item);
    }

    public static void ReturnAll(params T[] items)
    {
        foreach (var item in items)
            _pool.Push(item);
    }

    public static int Available => _pool.Count;
}

// Usage:
// var vec = ObjectPool<Vector3>.Get();
// vec.Set(1, 2, 3);
// // ... use ...
// ObjectPool<Vector3>.Return(vec);
