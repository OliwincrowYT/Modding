# BepInEx Modding: Beginner to Expert

---

# Table of Contents

1. **[Getting Started](#1-getting-started)** — What is BepInEx, installation, first plugin
2. **[C# & Unity Fundamentals](#2-c--and-unity-fundamentals)** — The building blocks
3. **[Plugin Anatomy](#3-plugin-anatomy)** — Every part of a BepInEx plugin
4. **[Harmony Patching](#4-harmony-patching)** — The heart of BepInEx modding
5. **[Game APIs & Patterns](#5-game-apis--patterns)** — Working with game systems
6. **[Configuration](#6-configuration)** — User-facing settings
7. **[UI Development](#7-ui-development)** — In-game menus, HUDs, debug panels
8. **[Asset & Content Mods](#8-asset-and-content-mods)** — Models, textures, audio
9. **[Advanced Harmony](#9-advanced-harmony)** — Transpilers, finalizers, stack manipulation
10. **[Performance & Debugging](#10-performance-and-debugging)** — Profiling, logging, troubleshooting
11. **[Multi-Mod Compatibility](#11-multi-mod-compatibility)** — Patch order, conflicts, coexistence
12. **[Distribution](#12-distribution)** — Building, packaging, publishing
13. **[Expert Techniques](#13-expert-techniques)** — IL injection, dynamic methods, native interop
14. **[Cheat Sheet](#14-cheat-sheet)** — Quick reference

---

## 1. Getting Started

### What is BepInEx?

**BepInEx** (BepInEx = **B**e **P**lugin **In** **E**x) is a plugin framework for Unity/MonoGames games. It lets you:

- Load DLL-based plugins at runtime
- Hook into game methods with **Harmony** (code patching at the IL level)
- Expose configuration files to users
- Interact with Unity APIs (Input, UI, Physics, etc.)

**Supported:** Unity 4.x–2022.x (Mono & IL2CPP), MonoGame, FNA, SDL2 apps.

### Version Quick Reference

| Version | Unity Support | Notes |
|---------|-------------|-------|
| BepInEx 5 | Unity 4–2019 | Legacy, Mono only |
| BepInEx 6 | Unity 4–2021 | Current stable, Mono + IL2CPP |
| BepInEx 7 | Unity 2020+ | Beta, newer Unity APIs |

### Installation

1. Download the latest release from https://github.com/BepInEx/BepInEx/releases
2. Pick the right package:
   - `win` = Windows x64
   - `linux` = Linux x64
   - `macos` = macOS
3. Extract into the game's root folder (where `.exe` lives)
4. Launch the game — it creates `BepInEx/` with folders:
   ```
   GameRoot/
   ├── Game.exe
   ├── BepInEx/
   │   ├── core/           # BepInEx assemblies (DON'T TOUCH)
   │   ├── plugins/        # Drop your .dll here
   │   ├── patchers/       # Assembly patchers (pre-load)
   │   ├── shader_chainconfig/
   │   └── BeatLoader.cfg  # Main config
   ```

### Your First Plugin

```csharp
using System;
using BepInEx;

// BepInPlugin: name, GUID, version — GUID MUST BE UNIQUE
[BepInPlugin("com.yourname.helloworld", "HelloWorld", "1.0.0")]
public class HelloWorldPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo("Hello, BepInEx! Your plugin is running.");
    }
}
```

**Project file (`.csproj`):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BepInEx.Analyzers" Version="1.*" PrivateAssets="all" />
    <PackageReference Include="BepInEx.Core" Version="5.4.21" />
    <PackageReference Include="BepInEx.PluginInfoProps" Version="1.*" />
  </ItemGroup>
  <!-- Reference game assemblies -->
  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>path/to/game/Managed/Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

Build → copy `bin/Debug/YourPlugin.dll` into `BepInEx/plugins/` → launch game.

---

## 2. C# and Unity Fundamentals

### C# Essentials for Modding

**Events & Delegates** — the backbone of hooking:
```csharp
// Delegate = a type that represents a method signature
public delegate void OnPlayerHit(int damage);

// Event = a delegate that only the owning class can fire
public event OnPlayerHit PlayerHitEvent;

// Subscribe
someObject.PlayerHitEvent += (dmg) => { /* react */ };

// Unsubscribe (prevent memory leaks)
someObject.PlayerHitEvent -= handler;
```

**Generics** — reusable code patterns:
```csharp
public T Clamp<T>(T value, T min, T max) where T : IComparable<T>
{
    return value.CompareTo(min) < 0 ? min :
           value.CompareTo(max) > 0 ? max : value;
}
```

**Extension Methods** — add methods to existing types:
```csharp
public static class VectorExtensions
{
    // 'this' keyword marks the extended type
    public static float DistanceTo2D(this Vector3 a, Vector3 b)
    {
        return new Vector2(a.x - b.x, a.z - b.z).magnitude;
    }
}
// Usage: transform.position.DistanceTo2D(target.position);
```

**LINQ** — query collections:
```csharp
using System.Linq;

var enemies = GameObject.FindObjectsOfType<Enemy>()
    .Where(e => e.IsAlive && e.Health > 0)
    .OrderBy(e => Vector3.Distance(e.transform.position, player.position))
    .ToList();
```

### Unity Engine Concepts

**GameObject** — everything in a scene. Think "container for components."

**Component** — behavior attached to GameObjects (`MonoBehaviour`, `Rigidbody`, etc.)

**Transform** — position, rotation, scale. Every GameObject has one.

**MonoBehaviour Lifecycle:**
```
Awake()          → Called first. Initialize references.
OnEnable()       → Called when the object becomes active.
Start()          → First frame update. Safe to reference other awaked objects.
FixedUpdate()    → Physics timestep (default 0.02s). Forces, collisions.
Update()         → Every frame. Input, movement, logic.
LateUpdate()     → After all Update(). Camera follow, aim.
OnDisable()      → Object deactivated.
OnDestroy()      → Object destroyed. Cleanup.
```

**Coroutines** — timed sequences:
```csharp
IEnumerator WaitThenDoStuff()
{
    yield return new WaitForSeconds(2f);      // Wait 2 seconds
    yield return new WaitForFixedUpdate();     // Wait for physics
    yield return null;                         // Wait one frame
    yield return new WWW("http://...");        // Wait for web request
    yield return StartCoroutine(OtherRoutine());// Nested coroutine
}

// Start: StartCoroutine(WaitThenDoStuff());
// Stop:  StopCoroutine(WaitThenDoStuff());
```

---

## 3. Plugin Anatomy

### The BaseUnityPlugin

```csharp
[BepInPlugin("com.example.mymod", "MyMod", "2.0.0")]
[BepInDependency("com.other.required", BepInDependencyMode.Hard)]    // MUST exist
[BepInDependency("com.other.optional", BepInDependencyMode.Soft)]    // Nice to have
[BepInIncompatibility("com.evil.conflicting")]                       // Won't load with this
[BepInProcess("GameExe")]                                            // Only load in this game
[BepInProcess("UnityPlayer.dll")]                                    // Or this
public class MyMod : BaseUnityPlugin
{
    // Logger, Chainloader, and Config are injected automatically
    private void Awake()
    {
        var logger = Logger;       // BepInEx.Logging.ManualLogSource
        var config = Config;       // BepInEx.Configuration.Configuration
    }
}
```

### Attributes Deep-Dive

```csharp
// Plugin identity — GUID is the unique key across all mods
[BepInPlugin(
    guid: "com.github.user.myamazingmod",
    name: "My Amazing Mod",
    version: "1.3.0"
)]

// Hard dependency — plugin won't load without it
[BepInDependency("com.example.core", BepInDependencyMode.Hard)]

// Soft dependency — works without, but unlocks features if present
[BepInDependency("com.example.extra", BepInDependencyMode.Soft)]

// Check if a dependency is loaded
[BepInProcess("MyGame.exe")]
public class MyPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Access another plugin's public API
        var otherPlugin = Chainloader.PluginInfos
            .FirstOrDefault(p => p.Value.Metadata.GUID == "com.example.core")
            .Value.Instance as OtherPluginType;

        if (otherPlugin != null)
        {
            Logger.LogInfo("Found the other plugin! Can interop.");
        }
    }
}
```

### Lifecycle Events

```csharp
public class MyPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Called first. Set up Harmony, config, etc.
        // Other plugins may not be awaked yet.
    }

    private void Start()
    {
        // All Awake() calls done. Safe to interact with other plugins.
    }

    private void OnGUI()
    {
        // Called by Unity for IMGUI rendering
    }

    private void OnDestroy()
    {
        // Cleanup: unsubscribe events, undo patches
    }
}
```

### Static Constructor Alternative

```csharp
// For plugins that don't need Unity lifecycle
[BepInPlugin("com.example.static", "StaticMod", "1.0.0")]
public class StaticPlugin
{
    // Runs when the type is first loaded — before any plugin Awake()
    static StaticPlugin()
    {
        // Good for early Harmony patches
        var harmony = new HarmonyLib.Harmony("com.example.static");
        harmony.PatchAll();
    }
}
```

---

## 4. Harmony Patching

### What is Harmony?

**Harmony** is a runtime code-patching library. It modifies method IL (Intermediate Language) to inject your code **before**, **after**, or **instead of** existing code — without touching the original source.

### Patch Types

```
Original Method
┌─────────────────────┐
│                     │
├─────────────────────┤
│   original body     │
├─────────────────────┤
│                     │
└─────────────────────┘

PREPATCH  (Prefix)    → Runs BEFORE the original
POSTPATCH (Postfix)   → Runs AFTER the original
INSTEAD   (Transpiler)→ Modifies the IL itself
WRAPPER   (Finalizer) → Wraps everything in try/catch
```

### Basic Patch

```csharp
using HarmonyLib;
using UnityEngine;

[HarmonyPatch]                    // No target specified — use name/declaringtype
[HarmonyPatch(typeof(Player))]    // Patch the Player class
[HarmonyPatch("TakeDamage")]      // Patch the TakeDamage method
[HarmonyPatch(new Type[] { typeof(float), typeof(DamageType) })] // Overload disambiguation
static class PlayerTakeDamage_Patch
{
    // Runs BEFORE TakeDamage
    [HarmonyPrefix]
    static bool Prefix(Player __instance, ref float damage, DamageType type)
    {
        // __instance = 'this' of the original method
        // Parameters match the original method signature

        // Return false to SKIP the original method entirely
        if (__instance.isInvincible)
        {
            damage = 0f;
            return false;  // Original won't run
        }

        // Modify damage before it's applied
        damage *= 0.5f;  // 50% damage reduction
        return true;      // Continue to original
    }

    // Runs AFTER TakeDamage (regardless of prefix return)
    [HarmonyPostfix]
    static void Postfix(Player __instance, float damage)
    {
        // __instance is the 'this' reference
        Debug.Log($"Player took {damage} damage. HP: {__instance.health}");
    }
}
```

### Applying Patches

```csharp
// Manual patch
var harmony = new HarmonyLib.Harmony("com.example.mymod");

var original = AccessTools.Method(typeof(Player), "TakeDamage");
var prefix = AccessTools.Method(typeof(PlayerTakeDamage_Patch), nameof(PlayerTakeDamage_Patch.Prefix));
var postfix = AccessTools.Method(typeof(PlayerTakeDamage_Patch), nameof(PlayerTakeDamage_Patch.Postfix));

harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

// PatchAll — applies all [HarmonyPatch] methods in the assembly
harmony.PatchAll();

// Unpatch
harmony.UnpatchAll("com.example.mymod");

// Unpatch specific
harmony.Unpatch(original, HarmonyPatchType.Prefix, prefix);
```

### Special Parameters

```csharp
[HarmonyPrefix]
static void Prefix(
    Player __instance,           // 'this' reference
    ref float damage,            // Original parameter (ref = can modify)
    DamageType type,             // Original parameter (in = read-only)

    // Result: the return value of the original (only in postfix)
    // ref bool __result,        // In postfix: original's return value

    // State: pass data from prefix to postfix
    // ref HarmonyMethod __targetMethod,  // The actual method being patched
    // ref bool __runOriginal,            // In prefix: set false to skip original
    // ref object[] __state               // Pass arbitrary data prefix→postfix
    ref object __state
)
{
    // Use __state to communicate between prefix and postfix
    __state = new { OriginalDamage = damage };
    damage *= 2f;
}

[HarmonyPostfix]
static void Postfix(
    Player __instance,
    ref float __result,          // The return value (can modify!)
    ref object __state           // Receive data from prefix
)
{
    var state = (dynamic)__state;
    Debug.Log($"Original damage was {state.OriginalDamage}, result: {__result}");
}
```

### Patching Overloads

```csharp
// By name + parameter types (for overloaded methods)
[HarmonyPatch(typeof(Transform), "GetChild")]
[HarmonyPatch(new Type[] { typeof(int) })]  // GetChild(int), not GetChild(string)

// By delegate (most reliable)
static void SetupPatches(Harmony harmony)
{
    // Original: void Player.TakeDamage(float amount, DamageSource source)
    var original = new Action<Player, float, DamageSource>((p, a, s) =>
        p.TakeDamage(a, s));
    var originalMethod = original.Method.GetGenericMethodDefinition()
        .GetParameters()[0].ParameterType.GetMethod();

    // Simpler with AccessTools
    originalMethod = AccessTools.Method(typeof(Player), "TakeDamage",
        new[] { typeof(float), typeof(DamageSource) });

    harmony.Patch(originalMethod,
        prefix: new HarmonyMethod(typeof(MyPatches), "MyPrefix"));
}
```

### Conditional Patches

```csharp
[HarmonyPrefix]
static bool Prefix(ref bool __runOriginal)
{
    // Only apply our patch when the config toggle is on
    if (!Config.EnableDamageMod.Value)
    {
        __runOriginal = true;
        return true;  // Let original run, skip our logic
    }

    // Our patch logic here...
    return true;
}
```

---

## 5. Game APIs and Patterns

### Finding Game Types

```csharp
// Assembly-CSharp.dll is the game's main code
// Use a decompiler (ILSpy, dnSpy, dotPeek) to explore

// AccessTools — your best friend for finding things
var playerType = AccessTools.TypeByName("Game.Player");
var healthField = AccessTools.Field(playerType, "_health");
var takeDamageMethod = AccessTools.Method(playerType, "TakeDamage");

// When you don't know the namespace
var anyType = AccessTools.TypeByName("Player");        // Just the name
var nestedType = AccessTools.TypeByName("Game+Player+State"); // Nested classes use +

// Get private/protected members
var privateField = AccessTools.Field(typeof(Player), "<health>k__BackingField");
var privateMethod = AccessTools.Method(typeof(Player), "<>c__DisplayClass5_0.<Attack>b__0");

// Properties
var property = AccessTools.Property(typeof(Player), "Health");

// Constructors
var ctor = AccessTools.Constructor(typeof(Player), new[] { typeof(string) });

// Generic methods
var genericMethod = AccessTools.Method(typeof(Player), "GetComponent", Type.EmptyTypes);
```

### Common Unity Patterns in Games

```csharp
// Singleton pattern (very common in games)
public static GameManager Instance => GameManager.instance;

// Patch to hook into singleton init
[HarmonyPatch(typeof(GameManager), "Initialize")]
[HarmonyPostfix]
static void OnGameInit()
{
    var gm = GameManager.Instance;
    // Now you have access to the game manager
}

// FindObjectOfType — slow but reliable
var player = Object.FindObjectOfType<PlayerController>();

// FindObjectOfType<T> — all instances
var allEnemies = Object.FindObjectsOfType<EnemyAI>();

// SendMessage — Unity's reflection-based messaging
gameObject.SendMessage("OnHit", damage, SendMessageOptions.DontRequireReceiver);
```

### Input Handling

```csharp
// Unity Input Manager (legacy)
if (Input.GetKey(KeyCode.Space)) { /* held */ }
if (Input.GetKeyDown(KeyCode.Space)) { /* pressed this frame */ }
if (Input.GetMouseButton(0)) { /* left click */ }
if (Input.GetAxis("Horizontal") > 0.5f) { /* A/D or left/right arrow */ }

// Unity New Input System (if game uses it)
if (PlayerInputActions.Default.Move.IsPressed()) { /* ... */ }
```

### Scene Management

```csharp
// Current scene
var current = SceneManager.GetActiveScene();
Debug.Log(current.name);

// Scene load hooks
SceneManager.sceneLoaded += (scene, mode) =>
{
    Debug.Log($"Loaded scene: {scene.name}");
};

SceneManager.sceneUnloaded += (scene) =>
{
    Debug.Log($"Unloaded scene: {scene.name}");
};

// Force scene load
SceneManager.LoadScene("MainMenu");
```

---

## 6. Configuration

### Basic Config

```csharp
[BepInPlugin("com.example.configdemo", "ConfigDemo", "1.0.0")]
public class ConfigDemo : BaseUnityPlugin
{
    // Auto-generated config entry — appears in Config.cfg
    ConfigEntry<string> playerName;
    ConfigEntry<int> damageMultiplier;
    ConfigEntry<bool> enableGodMode;
    ConfigEntry<KeyCode> bindKey;
    ConfigEntry<AcceptedValueTypes> restrictedValue;

    private void Awake()
    {
        // Simple: name, default value, description
        playerName = Config.Bind("General", "Player Name", "Anonymous",
            "Your display name in-game");

        damageMultiplier = Config.Bind("Combat", "Damage Multiplier", 100,
            "Percentage. 100 = normal, 200 = double damage");

        enableGodMode = Config.Bind("Cheats", "God Mode", false,
            "Invincibility when enabled");

        // Key binding
        bindKey = Config.Bind("Controls", "Toggle Key", KeyCode.RightControl,
            "Press to toggle mod features");

        // AcceptedValues — restrict to a set of options
        restrictedValue = Config.Bind("Display", "Theme",
            AcceptedValueTypes.Light,
            new AcceptableValueList<AcceptedValueTypes>(
                AcceptedValueTypes.Light, AcceptedValueTypes.Dark, AcceptedValueTypes.Auto));

        // Listen for changes
        enableGodMode.SettingChanged += (o, n) =>
        {
            Logger.LogInfo($"God mode is now {(n ? "ON" : "OFF")}");
        };

        // Read values
        var name = playerName.Value;
        var mult = damageMultiplier.Value;
    }

    enum AcceptedValueTypes { Light, Dark, Auto }
}
```

### Config Sections & Subsettings

```csharp
// Sections organize settings in the config file
Config.Bind("Player|Appearance", "Scale", 1.0f, "Player model scale");
Config.Bind("Player|Appearance", "Color", "#FF0000", "Player color hex");
Config.Bind("Player|Combat", "AutoAim", false, "Enable auto-aim");
Config.Bind("Player|Combat", "AimRadius", 5.0f, "Auto-aim detection radius");
```

### Manual Config Parsing

```csharp
// For complex settings that don't fit built-in types
ConfigEntry<string> customVector;

private void Awake()
{
    customVector = Config.Bind("Custom", "MyVector3", "1,2,3",
        "Format: x,y,z");

    // Parse manually
    var parts = customVector.Value.Split(',');
    var vec = new Vector3(
        float.Parse(parts[0]),
        float.Parse(parts[1]),
        float.Parse(parts[2])
    );
}
```

---

## 7. UI Development

### Unity IMGUI (Immediate Mode)

```csharp
public class MyPlugin : BaseUnityPlugin
{
    bool showMenu = true;
    float sliderValue = 0.5f;

    private void OnGUI()
    {
        if (!showMenu) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");

        GUILayout.Label("My Mod v1.0");
        sliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f);
        GUILayout.Label($"Value: {sliderValue:F2}");

        if (GUILayout.Button("Do Thing"))
        {
            Debug.Log("Button clicked!");
        }

        showMenu = GUILayout.Toggle(showMenu, "Keep Open");
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
```

### EGUI Wrapper (Recommended for Clean UI)

```csharp
// EGUI is a popular BepInEx UI library
// Install via NuGet: BepInEx.EGUI or similar

using EGUI;

public class MyPlugin : BaseUnityPlugin
{
    EGUIWindow window;

    private void Awake()
    {
        window = new EGUIWindow("My Mod Settings", new Vector2(400, 300));
        window.DrawChildren = true;

        var slider = new EGUIFloatSlider("Damage", 0f, 10f)
        {
            Value = 1f
        };
        window.AddChild(slider);
    }

    private void OnGUI()
    {
        window.OnGUI();
    }
}
```

### Canvas-Based UI (Unity UI System)

```csharp
// For proper Unity UI (not IMGUI)
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CanvasUI : BaseUnityPlugin
{
    private Canvas canvas;
    private void Awake()
    {
        // Create canvas if none exists
        canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("ModCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();

            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                new GameObject("EventSystem").AddComponent<EventSystem>();
            }
        }

        // Create a panel
        var panel = new GameObject("ModPanel");
        panel.transform.SetParent(canvas.transform, false);

        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10, 10);
        rect.offsetMax = new Vector2(-10, -10);

        var image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);

        // Create a button
        var buttonGo = new GameObject("MyButton");
        buttonGo.transform.SetParent(panel.transform, false);
        var button = buttonGo.AddComponent<Button>();
        buttonGo.AddComponent<Image>();
        buttonGo.AddComponent<Text>().text = "Click Me!";

        button.onClick.AddListener(() =>
        {
            Debug.Log("Unity UI button clicked!");
        });
    }
}
```

---

## 8. Asset and Content Mods

### Loading Custom Assets

```csharp
// Load a texture from the plugin folder
private Texture2D LoadTexture(string filename)
{
    var path = Path.Combine(Paths.PluginPath, filename);
    var bytes = File.ReadAllBytes(path);
    var tex = new Texture2D(1, 1);
    tex.LoadImage(bytes);
    return tex;
}

// Load a custom model (FBX → Unity GameObject)
// Requires: Unity's Model import or external library like AssetBundle
private GameObject LoadModel(string filename)
{
    var path = Path.Combine(Paths.PluginPath, filename);
    // Option 1: AssetBundle (recommended)
    var bundle = AssetBundle.LoadFromFile(path);
    return bundle.LoadAsset<GameObject>("MyModel");
}
```

### AssetBundles

```csharp
// Building AssetBundles (done in Unity Editor, not in the mod itself)
// [Unity Editor Script]
// BuildPipeline.BuildAssetBundle(...);

// Loading in the mod
private Dictionary<string, AssetBundle> _bundles = new();

private T LoadFromBundle<T>(string bundleName, string assetName) where T : Object
{
    if (!_bundles.ContainsKey(bundleName))
    {
        var path = Path.Combine(Paths.PluginPath, bundleName);
        _bundles[bundleName] = AssetBundle.LoadFromFile(path);
    }
    return _bundles[bundleName].LoadAsset<T>(assetName);
}

private void OnDestroy()
{
    foreach (var bundle in _bundles.Values)
        bundle.Unload(false);
}
```

### Replacing Game Textures

```csharp
// Swap a material's texture at runtime
[HarmonyPatch(typeof(SomeRenderer), "Start")]
[HarmonyPostfix]
static void ReplaceTexture(SkinnedMeshRenderer __instance)
{
    var newTex = LoadTexture("custom_diffuse.png");
    var material = __instance.material;
    material.mainTexture = newTex;
}
```

### Audio

```csharp
// Play a sound
private AudioClip LoadAudioClip(string filename)
{
    var path = Path.Combine(Paths.PluginPath, filename);
    var bytes = File.ReadAllBytes(path);

    var clip = AudioClip.Create("CustomSound",
        SampleData.Length, 1, 44100, false);
    clip.SetData(SampleData, 0);
    return clip;
}

// Play through AudioSource
var source = new GameObject("AudioPlayer").AddComponent<AudioSource>();
source.clip = audioClip;
source.Play();
```

---

## 9. Advanced Harmony

### Transpilers (IL Modification)

```csharp
// Transpilers modify the IL bytecode of the original method
// Use when prefix/postfix isn't enough

[HarmonyPatch(typeof(TargetClass), "TargetMethod")]
static class TargetMethod_Patch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        // Local variables used by our injected IL
        var local = generator.DeclareLocal(typeof(float));

        var patched = new List<CodeInstruction>();
        bool jumpedOver = false;

        foreach (var instruction in instructions)
        {
            // Find: ldarg.1 (the 'amount' parameter)
            //       ldc.r4 10.0
            //       mul
            if (!jumpedOver && instruction.matches(new CodeInstruction(
                OpCodes.Ldc_R4, 10.0f)))
            {
                // Replace the constant 10.0 with 999.9
                patched.Add(new CodeInstruction(OpCodes.Ldc_R4, 999.9f));
                jumpedOver = true;
                continue;
            }

            // Inject: Debug.Log("Patched!") after the first ldstr
            patched.Add(instruction);

            if (instruction.opcode == OpCodes.Ldstr
                && instruction.operand?.ToString() == "Game Start")
            {
                // Inject our code here
                patched.Add(Push.Value(generator, "Patched: Game Start"));
                patched.Add(new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(Debug), "Log", new[] { typeof(string) })));
            }
        }

        return patched;
    }
}
```

### IL CodeInstruction Basics

```
IL Opcode           | C# Equivalent           | Description
────────────────────┼─────────────────────────┼─────────────────
ldarg.0             | this                    | Load arg 0 (this)
ldarg.1             | param1                  | Load arg 1
ldarg.s (byte)      | paramN                  | Load arg by index
ldarga              | ref param               | Load address of arg
ldloc.0             | local0                  | Load local variable 0
stloc.0             | local0 = ...            | Store to local 0
ldsfld              | ClassName.field         | Load static field
ldfld               | instance.field          | Load instance field
stfld               | instance.field = ...    | Store instance field
ldstr               | "string literal"        | Load string
ldc.i4              | int constant            | Load int constant
ldc.r4              | float constant          | Load float constant
ldc.r8              | double constant         | Load double constant
call                | MethodName(...)         | Call a method
callvirt            | virtualMethod(...)      | Call virtual method
newobj              | new TypeName(...)       | Create new object
ret                 | return                  | Return from method
br                  | goto                    | Unconditional branch
brtrue/brfalse      | if (...) goto           | Conditional branch
nop                 | (nothing)               | No operation
```

### Finalizer (Try/Catch Wrapper)

```csharp
[HarmonyPatch(typeof(UnsafeClass), "DangerousMethod")]
static class DangerousMethod_Patch
{
    [HarmonyFinalizer]
    static void Finalizer(Exception __exception)
    {
        if (__exception != null)
        {
            // The original threw — handle it
            Logger.LogError($"Caught: {__exception.Message}");
            // Don't rethrow — the exception is swallowed
        }
        else
        {
            // No exception — normal completion
        }
    }
}
```

### Patch Priority & Insertion Location

```csharp
[HarmonyPatch(typeof(Target), "Method")]
static class PriorityPatch
{
    // Priority: higher number = runs first (for prefixes)
    // Default priority = 0
    // BepInEx internal patches = -1000

    [HarmonyPriority(100)]           // Run early
    [HarmonyPrefix]
    static void EarlyPrefix() { }

    [HarmonyPriority(-100)]          // Run late
    [HarmonyPrefix]
    static void LatePrefix() { }

    // Insertion: where does this patch go relative to others?
    [HarmonyPatchBefore("com.other.mod")]    // Before their patches
    [HarmonyPatchAfter("com.other.mod")]     // After their patches
    [HarmonyPostfix]
    static void OrderedPostfix() { }
}
```

### Dynamic Methods (No Patcher Class Needed)

```csharp
var harmony = new HarmonyLib.Harmony("com.example.dynamic");

// Create a dynamic method at runtime
var original = AccessTools.Method(typeof(Player), "Move");

var prefix = new HarmonyMethod(typeof(DynamicPatches), "MovePrefix");
// Or use a delegate directly (Harmony 2+)
harmony.Patch(original,
    prefix: new HarmonyMethod((Action<Player, Vector3>)((p, v) =>
    {
        // Inline prefix logic
        Debug.Log($"Moving to {v}");
    }), null),
    postfix: null);
```

### AccessTools Deep Dive

```csharp
// Find methods with filters
AccessTools.Method(typeof(T), "Name")                           // By name
AccessTools.Method(typeof(T), "Name", new[] { typeof(int) })   // By name + params
AccessTools.AllDeclaredMethods(typeof(T))                       // All methods
AccessTools.GetDeclaredMethods(typeof(T))                       // Instance methods

// Find fields
AccessTools.Field(typeof(T), "_fieldName")
AccessTools.DeclaredField(typeof(T), "_fieldName")
AccessTools.AllDeclaredFields(typeof(T))

// Properties
AccessTools.Property(typeof(T), "PropertyName")

// Constructors
AccessTools.Constructor(typeof(T), new[] { typeof(string), typeof(int) })

// Nested types
AccessTools.TypeByName("Namespace.Outer+Inner")
AccessTools.InnerClass(typeof(Outer), "Inner")

// Declaring type + all bases
AccessTools.GetDeclaredMethods(typeof(T))       // Only T, not base
AccessTools.GetMethods(typeof(T))               // T + all base classes

// Make a private method accessible as a delegate
var privateFunc = AccessTools.Method(typeof(PrivateClass), "PrivateMethod")
    .CreateDelegate<Func<int, string>>();
var result = privateFunc(42);
```

---

## 10. Performance and Debugging

### Logging

```csharp
public class MyPlugin : BaseUnityPlugin
{
    // Logger is injected by BepInEx
    private void Awake()
    {
        Logger.LogInfo("Information message");
        Logger.LogWarning("Warning message");
        Logger.LogError("Error message");
        Logger.LogDebug("Debug message");       // Only in debug builds
        Logger.LogMessage(new LogEventArgs(
            LogLevel.Fatal, "Critical issue"));
    }
}

// Custom log source (for libraries)
var logSource = new LogSource();
logSource.SourceInfo = new LogSourceInfo
{
    DisplayName = "MyLibrary",
    LogLevel = LogLevel.Info,
    Level = LogLevel.Message
};
BepInEx.Logging.Logger.CreateLogSource(logSource);
logSource.LogMessage(new LogEventArgs(LogLevel.Info, "Library msg", "MyLibrary"));
```

### Performance Profiling

```csharp
// Unity Profiler
using UnityEngine.Profiling;

void Update()
{
    Profiler.BeginSample("MyExpensiveOperation");
    // ... expensive code ...
    Profiler.EndSample();
}

// Simple timing
var sw = System.Diagnostics.Stopwatch.StartNew();
DoWork();
Logger.LogInfo($"DoWork took {sw.ElapsedMilliseconds}ms");

// Frame timing
void Update()
{
    _frameStart = Time.realtimeSinceStartup;
}

void LateUpdate()
{
    var ms = (Time.realtimeSinceStartup - _frameStart) * 1000f;
    if (ms > 5f) // Warn if frame takes > 5ms
        Logger.LogWarning($"Slow frame: {ms:F1}ms");
}
```

### Common Pitfalls

```csharp
// ❌ BAD: Allocating in Update (GC pressure → stutter)
void Update()
{
    var list = new List<Enemy>();  // Allocated every frame!
    // ...
}

// ✅ GOOD: Reuse collections
private List<Enemy> _enemyPool = new();
void Update()
{
    _enemyPool.Clear();
    // ... populate ...
}

// ❌ BAD: FindObjectOfType every frame
void Update()
{
    var player = FindObjectOfType<Player>();  // O(n) search every frame!
}

// ✅ GOOD: Cache references in Awake/Start
private Player _player;
void Start()
{
    _player = FindObjectOfType<Player>();  // Once
}

// ❌ BAD: Creating strings in tight loops
for (int i = 0; i < 1000; i++)
{
    log += $"Item {i}\n";  // New string each iteration
}

// ✅ GOOD: StringBuilder
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
    sb.AppendLine($"Item {i}");
```

### Debug Tools

```csharp
// Unity Debug draws (visible in Scene view & game view with [Depth])
Debug.DrawLine(start, end, Color.red, 5f);       // Lasts 5 seconds
Debug.DrawRay(origin, direction, Color.green);
Debug.Log(message, targetGameObject);             // Log linked to object
Debug.LogWarning(message);
Debug.LogError(message);

// Create a persistent gizmo
void OnDrawGizmosSelected()
{
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, 5f);
}
```

---

## 11. Multi-Mod Compatibility

### Patch Order

```
Load Order:
1. BepInEx core loads
2. Patchers load (BepInEx/patchers/)
3. Plugins load (alphabetical by folder, then filename)
4. Each plugin's Awake() fires
5. Each plugin's Start() fires

Patch Application Order:
1. Higher [HarmonyPriority] runs first (prefixes)
2. Same priority → first registered wins
3. [HarmonyPatchBefore/After] adjusts relative order
```

### Avoiding Conflicts

```csharp
// Check if another mod already patched a method
var patchInfo = Harmony.GetPatchInfo(originalMethod);
if (patchInfo.Prefixes.Any(p =>
    p.Owner != "com.mymod" && p.PatchMethod.IsVisible))
{
    Logger.LogWarning("Another mod already patches this. Skipping.");
    return;
}

// Share patch responsibility — only patch if no one else did
[HarmonyPrefix]
static bool Prefix(ref object __state)
{
    // Check if a higher-priority prefix already handled this
    if (__state is HandledState) return true; // Skip, already handled

    __state = new MyState(); // Mark as handled by us
    // ... our logic ...
    return true;
}
```

### Plugin Interop

```csharp
public class MyPlugin : BaseUnityPlugin
{
    // Public API for other plugins
    public static MyPlugin Instance { get; private set; }

    public event Action<Player> OnPlayerModified;

    private void Awake()
    {
        Instance = this;

        // Check for compatible mods
        var hasOtherMod = Chainloader.PluginInfos
            .Any(p => p.Value.Metadata.GUID == "com.other.mod");

        if (hasOtherMod)
        {
            Logger.LogInfo("Other mod detected — enabling compat mode");
        }

        // Call another plugin's public API
        var other = Chainloader.PluginInfos
            .Select(p => p.Value.Instance)
            .OfType<OtherPluginType>()
            .FirstOrDefault();

        other?.RegisterCallback(OnMyCallback);
    }

    public void DoSomething(Player player)
    {
        OnPlayerModified?.Invoke(player);
    }
}
```

### Version Detection

```csharp
// Detect game version
var gameVersion = UnityEngine.Application.version;
Logger.LogInfo($"Game version: {gameVersion}");

// Detect Unity version
var unityVersion = UnityEngine.Application.unityVersion;

// Conditional patching based on version
if (int.Parse(gameVersion.Split('.')[0]) >= 2)
{
    harmony.PatchAll();
}
else
{
    Logger.LogWarning("Game version too old. Patches may not work.");
}
```

---

## 12. Distribution

### Building for Distribution

```xml
<!-- .csproj Release config -->
<PropertyGroup>
  <Configuration>Release</Configuration>
  <PlatformTarget>AnyCPU</PlatformTarget>
  <Optimize>true</Optimize>
  <DebugType>none</DebugType>  <!-- Strip debug symbols -->
</PropertyGroup>
```

```bash
# Build
dotnet build -c Release

# Output: bin/Release/YourPlugin.dll
```

### Package Structure

```
YourMod-1.0.0.zip
├── README.md                    # What it does, install instructions
├── CHANGELOG.md                 # Version history
├── LICENSE                      # MIT, GPL, etc.
├── BepInEx/
│   └── plugins/
│       └── YourMod/
│           ├── YourMod.dll      # The plugin
│           ├── YourMod.dll.mdb  # Debug symbols (optional)
│           ├── textures/        # Bundled assets
│           │   └── logo.png
│           └── config.example   # Config template
└── thumbnails/
    └── preview.jpg              # For Thunderstore
```

### Thunderstore Publishing

```
index.jpg              # 1200x600 banner
icon.jpg               # 256x256 icon
manifest.json          # Package metadata
```

**manifest.json:**
```json
{
  "name": "YourMod",
  "version_number": "1.0.0",
  "description": "Does amazing things.",
  "dependencies": [
    "BepInEx-BepInExPack-5.4.2100"
  ],
  "website_support": "https://github.com/user/yourmod",
  "donation": "https://ko-fi.com/user"
}
```

### CI/CD (GitHub Actions)

```yaml
# .github/workflows/build.yml
name: Build
on:
  push:
    tags: ['v*']
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 6.0.x
      - run: dotnet build -c Release
      - uses: actions/upload-artifact@v4
        with:
          name: plugin
          path: bin/Release/
```

---

## 13. Expert Techniques

### IL Injection at Specific Points

```csharp
[HarmonyTranspiler]
static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
{
    var label = generator.DefineLabel();
    var endLabel = generator.DefineLabel();

    // Define a local for our injected code
    LocalBuilder resultLocal = generator.DeclareLocal(typeof(bool));

    return instructions.Select((inst, i) =>
    {
        // Find the exact instruction pattern
        if (inst.opcode == OpCodes.Stfld
            && inst.operand is FieldInfo f
            && f.Name == "_isGameOver")
        {
            // Inject right before storing _isGameOver
            return new CodeInstruction[]
            {
                // Save the value being stored
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Stloc, resultLocal),

                // Call our hook
                new CodeInstruction(OpCodes.Ldarg, 0),  // 'this'
                new CodeInstruction(OpCodes.Ldloc, resultLocal), // value
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(MyPlugin), "OnGameOverSet")),

                // Continue with original store
                inst
            }.FirstOrDefault(); // Only return first, rest handled by iteration
        }
        return inst;
    }).Flatten(); // Flatten nested collections
}

static void OnGameOverSet(object __instance, bool isGameOver)
{
    if (isGameOver)
    {
        // Prevent game over
        // ... reverse the flag via reflection ...
    }
}
```

### Dynamic Method Generation

```csharp
using System.Reflection.Emit;

// Create a method at runtime without a containing class
var dynamicMethod = new DynamicMethod(
    "DynamicPrefix",
    MethodAttributes.Public | MethodAttributes.Static,
    CallingConventions.Standard,
    typeof(void),
    new[] { typeof(Player), typeof(float) },
    typeof(Player),
    false);

var il = dynamicMethod.GetILGenerator();

// emit: Console.WriteLine("Patched!")
il.Emit(OpCodes.Ldstr, "Patched!");
il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine",
    new[] { typeof(string) }));
il.Emit(OpCodes.Ret);

// Use as a Harmony patch
var harmony = new HarmonyLib.Harmony("com.example.dynamic");
var original = AccessTools.Method(typeof(Player), "TakeDamage");
harmony.Patch(original,
    prefix: new HarmonyMethod(dynamicMethod));
```

### Native Interop (P/Invoke)

```csharp
using System.Runtime.InteropServices;

public class NativeUtils
{
    // Call Windows API
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll")]
    static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    [DllImport("msvcrt.dll")]
    static extern IntPtr fopen(string filename, string mode);

    [DllImport("msvcrt.dll")]
    static extern int fprintf(IntPtr stream, string format, int value);

    // Show the Windows console (for raw debugging)
    public static void EnableConsole()
    {
        if (GetConsoleWindow() == IntPtr.Zero)
            AllocConsole();
    }
}
```

### Memory Reading (Unsafe)

```csharp
// ⚠️ Only when you have no other option. Fragile across game updates.
using System.Runtime.CompilerServices;

unsafe class MemoryReader
{
    // Read a float at a specific offset from a pointer
    public static float ReadFloat(void* basePtr, int offset)
    {
        return *((float*)((byte*)basePtr + offset));
    }

    // Read a pointer chain (common in Unity native code)
    public static void* FollowChain(void* start, int[] offsets)
    {
        var ptr = start;
        foreach (var offset in offsets)
        {
            ptr = *(void**)((byte*)ptr + offset);
            if (ptr == null) return null;
        }
        return ptr;
    }
}
```

### Runtime Assembly Generation

```csharp
using System.Reflection;
using System.Reflection.Emit;

// Generate an entire assembly at runtime
var assemblyName = new AssemblyName("RuntimeGenerated");
var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
    assemblyName, AssemblyBuilderAccess.Run);

var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
var typeBuilder = moduleBuilder.DefineType(
    "RuntimePlayer",
    TypeAttributes.Public | TypeAttributes.Class,
    typeof(MonoBehaviour));

// Add a field
var healthField = typeBuilder.DefineField("_health", typeof(float),
    FieldAttributes.Private);

// Add a property
var propBuilder = typeBuilder.DefineProperty(
    "Health", PropertyAttributes.HasDefault, typeof(float), Type.EmptyTypes);

var getMethod = typeBuilder.DefineMethod(
    "get_Health",
    MethodAttributes.Public | MethodAttributes.SpecialName,
    typeof(float), Type.EmptyTypes);

var getIl = getMethod.GetILGenerator();
getIl.Emit(OpCodes.Ldarg_0);
getIl.Emit(OpCodes.Ldfld, healthField);
getIl.Emit(OpCodes.Ret);
propBuilder.SetGetMethod(getMethod);

// Create the type
Type runtimeType = typeBuilder.CreateType();

// Instantiate
var instance = Activator.CreateInstance(runtimeType);
```

### Undoing Patches Safely

```csharp
public class ReversiblePatches
{
    private readonly Harmony _harmony;
    private readonly List<(MethodBase Original, HarmonyPatchType Type, MethodBase Patch)>
        _appliedPatches = new();

    public ReversiblePatches(string guid)
    {
        _harmony = new Harmony(guid);
    }

    public void Apply(MethodBase original, MethodBase prefix = null,
        MethodBase postfix = null)
    {
        _harmony.Patch(original,
            prefix: prefix != null ? new HarmonyMethod(prefix) : null,
            postfix: postfix != null ? new HarmonyMethod(postfix) : null);

        if (prefix != null)
            _appliedPatches.Add((original, HarmonyPatchType.Prefix, prefix));
        if (postfix != null)
            _appliedPatches.Add((original, HarmonyPatchType.Postfix, postfix));
    }

    public void UndoAll()
    {
        foreach (var (original, type, patch) in _appliedPatches)
        {
            _harmony.Unpatch(original, type,
                patch.MethodHandle.GetFunctionPointer());
        }
        _appliedPatches.Clear();
    }
}
```

### Hooking Unity's Internal Systems

```csharp
// Hook Unity's resource loading to intercept assets
[HarmonyPatch(typeof(ResourceRequest), "GetProgress")]
[HarmonyPostfix]
static void OnResourceLoadProgress(ref float __result)
{
    // Track loading progress for a custom loading screen
    CustomLoadingScreen.SetProgress(__result);
}

// Hook coroutine execution
[HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine),
    new[] { typeof(string) })]
[HarmonyPrefix]
static void OnCoroutineStart(MonoBehaviour __instance, string methodName)
{
    // Log or modify coroutine starts
    Debug.Log($"[{__instance.gameObject.name}] Starting coroutine: {methodName}");
}

// Hook Unity message dispatch
[HarmonyPatch(typeof(Component), "SendMessage",
    new[] { typeof(string), typeof(object), typeof(SendMessageOptions) })]
[HarmonyPrefix]
static void OnSendMessage(Component __instance, string methodName)
{
    // Intercept SendMessage calls
    if (methodName == "OnDamaged")
    {
        Debug.Log($"Intercepted: {methodName} on {__instance.gameObject.name}");
    }
}
```

---

## 14. Cheat Sheet

### Quick Reference

```csharp
// ═══════════════════════════════════════════════════════════
// PLUGIN TEMPLATE — Copy-paste starting point
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.yourname.modname", "ModName", "1.0.0")]
public class ModName : BaseUnityPlugin
{
    static ModName Instance;
    readonly Harmony _harmony = new("com.yourname.modname");

    private void Awake()
    {
        Instance = this;
        Logger.LogInfo("ModName loaded!");
        _harmony.PatchAll(typeof(ModName)); // Patch methods in this class
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf(); // Clean up
    }
}
```

```csharp
// ═══════════════════════════════════════════════════════════
// PATCH TEMPLATE — Copy-paste patch
// ═══════════════════════════════════════════════════════════

[HarmonyPatch(typeof(ClassName), "MethodName")]
static class ClassName_MethodName_Patch
{
    [HarmonyPrefix]
    static void Prefix(ref /* first param */, /* ... */)
    {
        // Before original
    }

    [HarmonyPostfix]
    static void Postfix(ref /* return value */)
    {
        // After original
    }
}
```

```csharp
// ═══════════════════════════════════════════════════════════
// COMMON ONE-LINERS
// ═══════════════════════════════════════════════════════════

// Get a type by full name
var t = AccessTools.TypeByName("Namespace.ClassName");

// Get a method by name + params
var m = AccessTools.Method(t, "MethodName", new[] { typeof(int) });

// Get a private field value
var f = AccessTools.Field(typeof(T), "_field");
var val = f.GetValue(instance);

// Set a private field
f.SetValue(instance, newValue);

// Invoke a private method
m.Invoke(instance, new object[] { arg1, arg2 });

// Find all GameObjects of a type
var all = Object.FindObjectsOfType<T>();

// Get the game's data path
var path = Paths.GameRootPath;
var pluginPath = Paths.PluginPath;
var configPath = Paths.ConfigPath;

// Harmony patch one-liner
new Harmony("guid").PatchAll();

// Log
Debug.Log("msg");
Logger.LogInfo("msg");

// Config
var cfg = Config.Bind("Section", "Key", defaultValue, "description");
var value = cfg.Value;
```

### Debugging Checklist

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| Plugin doesn't load | Wrong folder, bad GUID, missing dependency | Check `BepInEx/log_OUTPUT.log` |
| Crash on launch | Incompatible game version, wrong BepInEx version | Match versions |
| Patch not applying | Wrong method signature, overload mismatch | Use `AccessTools.Method` with param types |
| Null reference | GameObject not found, scene not loaded | Add null checks, wait for scene load |
| GC spikes | Allocating in Update | Cache references, object pooling |
| Patches conflict | Two mods patch same method | Use priority, check patch info |
| IL error in transpiler | Unbalanced stack, wrong operand type | Check IL stack balance |
| Config not saving | Wrong path permissions | Check `config.cfg` write access |

### Useful NuGet Packages

```xml
<!-- Core -->
<PackageReference Include="BepInEx.Core" Version="5.4.21" />

<!-- Harmony (usually included with BepInEx.Core) -->
<PackageReference Include="Lib.Harmony" Version="2.3.3" />

<!-- Unity assemblies — reference from game folder, not NuGet -->
<!-- Assembly-CSharp.dll, UnityEngine.dll, etc. -->

<!-- Optional helpers -->
<PackageReference Include="HarmonyX" Version="2.7.0" />  <!-- Harmony fork -->
<PackageReference Include="MMHOOK.Assembly-CSharp" Version="*" />  <!-- Method hooks -->
```

### Game-Specific Notes

| Game | BepInEx Version | Special Notes |
|------|----------------|---------------|
| Risk of Rain 2 | 6.x | Uses Unity 2019.4, Mono |
| Unity Multiplayer Games | 6.x | Network patches need special handling |
| IL2CPP Games | 6.x+ | Use BepInEx IL2CPP build, different patching |
| MonoGame Games | 6.x | No Unity APIs, use game-specific frameworks |

---

## Further Resources

- **BepInEx Docs:** https://docs.bepinex.dev/
- **Harmony Docs:** https://harmony.pardeike.net/
- **BepInEx GitHub:** https://github.com/BepInEx/BepInEx
- **Harmony GitHub:** https://github.com/pardeike/Harmony
- **Unity Scripting API:** https://docs.unity3d.com/ScriptReference/
- **IL OpCodes Reference:** https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes

---

*Built for the journey from "I dropped a DLL in a folder" to "I'm rewriting the game's networking layer at runtime."*
