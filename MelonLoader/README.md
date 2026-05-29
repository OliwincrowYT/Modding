# MelonLoader Modding: Complete Guide (Beginner to Expert)

> **Master reference for everything MelonLoader modding.** Covers Unity game modding from absolute zero to expert-level techniques including Harmony patches, opcode transpilers, custom UI, networking, and more.

---

## Table of Contents

- [Part 0: Prerequisites](#part-0-prerequisites)
- [Part 1: Beginner - Your First Mod](#part-1-beginner---your-first-mod)
- [Part 2: Beginner - Project Structure & Build](#part-2-beginner)
- [Part 3: Beginner - MelonLoader Attributes](#part-3-beginner)
- [Part 4: Beginner - MelonMod Lifecycle](#part-4-beginner)
- [Part 5: Intermediate - Harmony Patching](#part-5-intermediate)
- [Part 6: Intermediate - Unity UI](#part-6-intermediate)
- [Part 7: Intermediate - Game Objects & Scenes](#part-7-intermediate)
- [Part 8: Intermediate - Reflection & Assembly](#part-8-intermediate)
- [Part 9: Intermediate - Configuration Files](#part-9-intermediate)
- [Part 10: Advanced - Opcode Transpilers](#part-10-advanced)
- [Part 11: Advanced - Multi-Method Patching](#part-11-advanced)
- [Part 12: Advanced - Custom Commands & Console](#part-12-advanced)
- [Part 13: Advanced - Save System & Persistence](#part-13-advanced)
- [Part 14: Advanced - Networking & Multiplayer](#part-14-advanced)
- [Part 15: Expert - IL Injection & Dynamic Assemblies](#part-15-expert)
- [Part 16: Expert - Asset Loading & Resource Management](#part-16-expert)
- [Part 17: Expert - Inter-Mod Communication](#part-17-expert)
- [Part 18: Expert - Performance & Profiling](#part-18-expert)
- [Part 19: Expert - Anti-Patterns & Pitfalls](#part-19-expert)
- [Appendix A: Quick Reference Cheatsheet](#appendix-a)
- [Appendix B: Common Games & Their Quirks](#appendix-b)
- [Appendix C: Debugging Checklist](#appendix-c)

---

## Part 0: Prerequisites

### What is MelonLoader?

MelonLoader is a **universal Unity mod loader**. It injects into Unity games (IL_2CPP and Mono), loads your compiled C# assemblies (`.dll` files), and provides hooks into the game's lifecycle. It works with:

- **Mono** Unity builds (32-bit and 64-bit)
- **IL2CPP** Unity builds (64-bit) — via its IL2CPP compatibility layer
- Unity versions 2017.x through 2022.x (and beyond)

### What You Need

| Requirement | Details |
|---|---|
| **C# Knowledge** | Understand classes, methods, attributes, delegates, generics |
| **.NET SDK** | .NET 6+ for building; target framework depends on game's Unity version |
| **IDE** | Visual Studio 2022 (recommended), VS Code, or Rider |
| **A Unity Game** | Any Mono or IL2CPP Unity game installed via Steam, etc. |
| **MelonLoader** | Downloaded from [GitHub](https://github.com/LachesisUS/MelonLoader) or [GitHub Releases](https://github.com/Lachee127/MelonLoader) |

### Installing MelonLoader

1. **Download** the correct build matching your game's architecture (Mono vs IL2CPP, 32-bit vs 64-bit)
2. **Extract** the entire contents into your game's root directory (where `.exe` lives)
3. **Launch** the game — MelonLoader auto-injects and creates its folder structure:
```
YourGame/
├── YourGame.exe
├── MelonLoader/
│   ├── MelonLoader.exe
│   ├── MelonLoader.dll
│   ├── MonoMod/
│   ├── 0Harmony/
│   └── ...
├── Mods/                  ← Drop .dll mods here
├── ModsBackup/
├── Plugins/
├── MelonCache/
├── MelonKeyBindings/
└── ModsConfig/            ← .json config files
```

### Target Framework by Unity Version

| Unity Version | Target Framework |
|---|---|
| Unity 2017.x and below | `.NET Framework 4.x` |
| Unity 2018.x – 2020.x | `.NET Standard 2.0` or `.NET Core 3.1` |
| Unity 2021.x+ | `.NET 6` / `.NET Core 3.1` |

> **Tip:** Check `game_Data/UnitySabreVersion.txt` or `MonoBleedingEdge/` for the exact Unity version.

---

## Part 1: Beginner - Your First Mod

### Hello World Mod

The absolute simplest MelonLoader mod:

```csharp
using MelonLoader;

public class HelloWorldMod : MelonMod
{
    public override void OnUpdateMelon()
    {
        MelonLogger.Msg("Hello, World! This runs every frame.");
    }
}
```

That's it. Compile to a `.dll`, drop it in `Mods/`, and launch the game.

### Understanding What Happens

1. MelonLoader scans the `Mods/` folder for `.dll` files
2. It finds classes that inherit from `MelonMod`
3. It reads the `MelonModInfo` attribute for metadata
4. It instantiates your mod and calls `OnInitializeMelon()`
5. On every Unity frame, `OnUpdateMelon()` fires

### Without MelonModBase (Manual Attribute)

```csharp
using MelonLoader;

[MelonModName("My Hello World")]
[MelonModAuthor("MyName")]
[MelonModVersion("1.0.0")]
[MelonModUserId(12345)]  // Optional: your MelonLoader user ID

public class HelloWorldMod
{
    // No base class needed — attributes alone work
    public void OnInitializeMelon()
    {
        MelonLogger.Msg("Mod loaded!");
    }
}
```

---

## Part 2: Beginner - Project Structure & Build

### Recommended Project Layout

```
MyMod/
├── MyMod.csproj
├── MyMod.cs              ← Main mod class
├── Features/
│   ├── Feature1.cs
│   └── Feature2.cs
├── Patches/
│   └── SomePatch.cs
├── Utils/
│   └── Helpers.cs
└── References/
    └── (optional: game assemblies for reference-only)
```

### .csproj Template (.NET 6)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <LangVersion>latest</LangVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <OutputPath>..\..\Builds</OutputPath>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="MelonLoader">
      <HintPath>References\MelonLoader.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Assembly-CSharp">
      <HintPath>References\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>References\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

### Game Assembly References

Extract from your game:
- **Mono games:** Assemblies are in `game_Data/Managed/`
- **IL2CPP games:** Use MelonLoader's generated assemblies in `MelonLoader/ManagedAssemblies/` (created after first launch)

> **Never copy game assemblies with your mod** — they're already in the game. Reference-only, `Private=false`.

### Build & Deploy

```bash
dotnet build -c Release
# Output: Builds/MyMod.dll → copy to YourGame/Mods/
```

Or set up an auto-copy post-build in `.csproj`:
```xml
<Target Name="DeployMod" AfterTargets="PostBuildEvent">
  <Copy SourceFiles="$(TargetPath)"
        DestinationFolder="C:\Path\To\YourGame\Mods\" />
</Target>
```

---

## Part 3: Beginner - MelonLoader Attributes

### MelonModInfo Attributes

| Attribute | Required | Description |
|---|---|---|
| `[MelonModName]` | Yes | Display name of your mod |
| `[MelonModAuthor]` | Yes | Your name / alias |
| `[MelonModVersion]` | Yes | Semantic version (`1.2.3`) |
| `[MelonModDescription]` | No | Description shown in console |
| `[MelonModUserId]` | No | Your MelonLoader API user ID |
| `[MelonModApiVersion]` | No | Required API version |
| `[MelonInfo]` | No | Custom key-value info display |

### MelonGame Attribute (Optional)

Restricts your mod to a specific game:
```csharp
[MelonGame("BONWORKER", "Boneworks")]
// or
[MelonGame("OVRR", "OVR")]
```

### MelonAssembly (For Plugin/Mod Dependencies)

```csharp
[MelonAssemblyName("MyMod")]
[MelonAssemblyVersion("1.0.0")]
```

### Full Example

```csharp
[MelonModName("Super Mod")]
[MelonModAuthor("AwesomeDev")]
[MelonModVersion("2.1.0")]
[MelonModDescription("Does amazing things")]
[MelonModUserId(99999)]
[MelonInfo("CustomKey", "CustomValue")]
[MelonGame("BONWORKER", "Boneworks")]
public class SuperMod : MelonMod { }
```

---

## Part 4: Beginner - MelonMod Lifecycle

### Lifecycle Methods (All Overridable)

```csharp
public class MyMod : MelonMod
{
    // Called when MelonLoader first loads your mod assembly
    public override void OnAwakeMelon()
    {
        MelonLogger.Msg("Assembly loaded, instance created.");
    }

    // Called after all mods are awoken — safe to access other mods
    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("All mods loaded. Safe to interact.");
    }

    // Called every Unity Update() frame
    public override void OnUpdateMelon()
    {
        // Per-frame logic
    }

    // Called every Unity FixedUpdate() frame
    public override void OnFixedUpdateMelon()
    {
        // Physics-tied logic
    }

    // Called every Unity LateUpdate() frame
    public override void OnLateUpdateMelon()
    {
        // After all updates — good for camera/final transforms
    }

    // Called when Unity is about to render a frame
    public override void OnGuiMelon()
    {
        // Legacy IMGUI rendering (see UI section for modern approaches)
    }

    // Called when the game is closing
    public override void OnDestroyMelon()
    {
        MelonLogger.Msg("Game closing. Clean up here.");
    }

    // Called when MelonLoader loads settings for your mod
    public override void OnSettingsCreate()
    {
        // Register settings
    }

    // Called when the game scene changes
    public override void OnSceneWasLoaded(int sceneBuildIndex, string sceneName)
    {
        MelonLogger.Msg($"Loaded scene: {sceneName}");
    }

    // Called when the game scene is about to unload
    public override void OnSceneWasLoadedAdditive(int sceneBuildIndex, string sceneName)
    {
        // Additive scene load
    }
}
```

### MelonMod Prefs (Built-in Settings System)

```csharp
public class MyMod : MelonMod
{
    // MelonPreferences auto-saves to ModsConfig/YourMod.json
    public static bool myToggle;
    public static int myNumber;
    public static string myText;
    public static float myFloat;

    public override void OnSettingsCreate()
    {
        // Section, Key, Default Value, Description, Section Name
        myToggle = MelonPreferences.GetSettingsValue<bool>("General", "MyToggle", true, "Toggle this feature", "My Mod");
        myNumber = MelonPreferences.GetSettingsValue<int>("General", "MyNumber", 42, "A number setting (1-100)", "My Mod");
        myText   = MelonPreferences.GetSettingsValue<string>("General", "MyText", "Hello", "Some text", "My Mod");
        myFloat  = MelonPreferences.GetSettingsValue<float>("Advanced", "MyFloat", 3.14f, "A float value", "My Mod");
    }
}
```

> Settings are **auto-persisted**. Call `MelonPreferences.SaveSetting()` after programmatic changes.

---

## Part 5: Intermediate - Harmony Patching

### What is Harmony?

Harmony (0Harmony) is a **runtime code patching** library. MelonLoader bundles it. It lets you inject code before, after, or instead of any method — even private/static ones.

### Patch Types

| Type | When It Runs | Can Cancel? |
|---|---|---|
| **Prefix** | Before the original method | Yes (return `false`) |
| **Postfix** | After the original method | No |
| **Transpiler** | Modifies IL bytecode at patch time | N/A |
| **Finalizer** | In the `finally` block | No |
| **ReversePatch** | Replaces with another method | N/A |

### Finding Methods to Patch

```csharp
using System.Reflection;
using HarmonyLib;

// Find a method by name, type, and optional parameters
Type targetType = AccessTools.TypeByName("GameNamespace.GameClass");
MethodInfo targetMethod = AccessTools.Method(targetType, "MethodName",
    new Type[] { typeof(int), typeof(string) });

// If the type is private nested:
Type privateType = AccessTools.TypeByName("OuterClass+InnerClass");

// Instance vs Static:
// - Instance method on "this": no extra flags needed
// - Static method: same syntax, Harmony figures it out
```

### Basic Prefix (Cancel & Replace)

```csharp
using HarmonyLib;
using System.Reflection;
using MelonLoader;

[HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
static class SomeMethod_Patch
{
    static bool Prefix()
    {
        MelonLogger.Msg("Original method will NOT run!");
        return false;  // false = skip original, true = run original
    }
}
```

### Basic Postfix (Run After)

```csharp
[HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
static class SomeMethod_Postfix
{
    static void Postfix(ref int __result)  // __result to modify return value
    {
        __result = 999;  // Override the return value
        MelonLogger.Msg($"Method returned: {__result}");
    }
}
```

### Prefix with Parameters

```csharp
[HarmonyPatch(typeof(Player), "TakeDamage")]
static class TakeDamage_Patch
{
    // Access original parameters by name
    static bool Prefix(Player __instance, float damage, Vector3 hitPoint, ref bool __result)
    {
        // __instance = "this" of the original method
        MelonLogger.Msg($"Block damage: {damage} at {hitPoint}");

        // God mode: cancel the method entirely
        if (isGodModeEnabled)
        {
            __result = false;  // Set return value
            return false;      // Skip original
        }

        // Reduce damage by 50%
        damage *= 0.5f;
        return true;  // Continue with modified params
    }
}
```

### Dynamic Patching (No Attributes)

```csharp
var harmony = new Harmony("com.mymod.patches");

MethodInfo original = AccessTools.Method(typeof(SomeClass), "SomeMethod");
MethodInfo prefix   = AccessTools.Method(typeof(PatchClass), "Prefix");
MethodInfo postfix  = AccessTools.Method(typeof(PatchClass), "Postfix");

harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));

// Unpatch later:
harmony.Unpatch(original, HarmonyPatchType.Prefix, prefix.MethodHandle.GetFunctionPointer());
```

### Patch Priority & Order

```csharp
[HarmonyPatch(typeof(Target), "Method")]
[HarmonyPriority(Priority.High)]  // Runs before Priority.Normal (default)
static class EarlyPatch
{
    static bool Prefix() { return true; }
}

[HarmonyPatch(typeof(Target), "Method")]
[HarmonyBefore(new[] { "com.other.mod" })]  // Explicit ordering
static class OrderedPatch { }
```

Priority levels: `VeryLow` (-10) → `Low` (-2) → `LowNormal` (-1) → `Normal` (0) → `HighNormal` (1) → `High` (2) → `VeryHigh` (10)

---

## Part 6: Intermediate - Unity UI

### Option 1: Unity IMGUI (Simplest)

```csharp
public class MyMod : MelonMod
{
    bool showMenu = true;
    Rect windowRect = new Rect(20, 20, 300, 200);

    public override void OnGuiMelon()
    {
        if (!showMenu) return;
        windowRect = GUI.Window(1, windowRect, DrawWindow, "My Mod v1.0");
    }

    void DrawWindow(int windowID)
    {
        GUI.enabled = true;

        // Toggle
        bool val = GUI.Toggle(new Rect(10, 30, 150, 20), val, "Enable Feature");

        // Slider
        currentSlider = GUI.HorizontalSlider(new Rect(10, 60, 150, 20), currentSlider, 0f, 100f);

        // Button
        if (GUI.Button(new Rect(10, 90, 100, 25), "Do Thing"))
        {
            MelonLogger.Msg("Button clicked!");
        }

        // Text Field
        currentText = GUI.TextField(new Rect(10, 120, 200, 25), currentText);

        GUI.DragWindow(new Rect(0, 0, 1000, 1000));  // Make draggable
    }
}
```

### Option 2: EGUITools (Better IMGUI)

EGUITools is a MelonLoader addon for improved GUI. Reference `EGUITools.dll` and use:

```csharp
using EGUITools;

public class MyMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        EGUITools.AddButton("My Button", () =>
        {
            MelonLogger.Msg("Clicked!");
        }, "My Category");
    }
}
```

### Option 3: Unity UI (Canvas-based)

```csharp
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    static GameObject canvasObj;
    static Canvas canvas;

    public static void Initialize()
    {
        // Create or find canvas
        canvasObj = GameObject.Find("ModCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("ModCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Auto-destroy on scene change if needed
            DontDestroyOnLoad(canvasObj);
        }

        // Create a button
        var button = new GameObject("MyButton");
        button.transform.SetParent(canvas.transform, false);
        var btnComponent = button.AddComponent<Button>();
        var image = button.AddComponent<Image>();

        // Position
        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(20, Screen.height - 60);
        rect.sizeDelta = new Vector2(150, 40);

        // Text label
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(button.transform, false);
        var textComp = textObj.AddComponent<Text>();
        textComp.text = "Click Me";
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 14;
        textComp.alignment = TextAnchor.MiddleCenter;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // Click handler
        btnComponent.onClick.AddListener(() =>
        {
            MelonLogger.Msg("Canvas button clicked!");
        });
    }
}
```

---

## Part 7: Intermediate - Game Objects & Scenes

### Spawning Objects

```csharp
using UnityEngine;

// Spawn a cube at the origin
var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
cube.transform.position = Vector3.zero;
cube.transform.localScale = Vector3.one * 2f;

// Add components
var rb = cube.AddComponent<Rigidbody>();
rb.useGravity = true;

// Destroy after 5 seconds
Destroy(cube, 5f);
```

### Loading Assets

```csharp
// From Resources folder (if the game has one)
var asset = Resources.Load<GameObject>("Path/To/Asset");
var instance = Instantiate(asset, position, rotation);

// From file system (Texture2D example)
var texture = new Texture2D(2, 2);
texture.LoadImage(File.ReadAllBytes(@"C:\path\to\image.png"));
var material = new Material(Shader.Find("Diffuse"));
material.mainTexture = texture;
```

### Scene Management

```csharp
// Get current scene
var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
MelonLogger.Msg($"Scene: {currentScene.name} (BuildIndex: {currentScene.buildIndex})");

// Load a scene
UnityEngine.SceneManagement.SceneManager.LoadScene("SceneName");

// Additive load
UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("ExtraScene", LoadSceneMode.Additive);

// Get all scene names
var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
for (int i = 0; i < sceneCount; i++)
{
    MelonLogger.Msg($"  [{i}] {UnityEngine.SceneManagement.SceneManager.GetScenePathByBuildIndex(i)}");
}
```

### Finding Game Objects

```csharp
// By name (slow — caches internally but still)
var obj = GameObject.Find("Player");

// By type
var allCameras = FindObjectsOfType<Camera>();

// By tag
var player = GameObject.FindGameObjectWithTag("Player");

//InChildren
var children = obj.GetComponentsInChildren<Transform>(true);
```

---

## Part 8: Intermediate - Reflection & Assembly

### AccessTools (Harmony's Helpers)

```csharp
using HarmonyLib;

// Get a type by full name (works across assemblies)
Type myType = AccessTools.TypeByName("Namespace.ClassName");

// Get nested type
Type nested = AccessTools.TypeByName("OuterClass+InnerClass");

// Get method (with optional parameter types for overloads)
MethodInfo method = AccessTools.Method(myType, "MethodName",
    new[] { typeof(int), typeof(string) });

// Get property
PropertyInfo prop = AccessTools.Property(myType, "SomeProperty");

// Get field
FieldInfo field = AccessTools.Field(myType, "_someField");

// Get constructor
ConstructorInfo ctor = AccessTools.Constructor(myType, new[] { typeof(int) });

// Get all methods/fields/properties
var allMethods = AccessTools.GetDeclaredMethods(myType);
var allFields  = AccessTools.GetDeclaredFields(myType);

// Deep search (includes base types)
var allDeep = AccessTools.GetDeclaredMethods(myType).Concat(
    AccessTools.GetDeclaredMethods(myType.BaseType));
```

### Field & Property Access

```csharp
// Read private field
var fieldValue = field.GetValue(instance);  // null for static

// Write private field
field.SetValue(instance, newValue);

// Shortcut via Harmony
var value = AccessTools.Field(refType, "fieldName").GetValue(target);
AccessTools.Field(refType, "fieldName").SetValue(target, 42);

// Property
var propValue = AccessTools.Property(refType, "PropertyName").GetValue(target);
```

### Dynamic Method Invocation

```csharp
// Call private method
method.Invoke(instance, new object[] { arg1, arg2 });

// Static method
method.Invoke(null, new object[] { arg1 });
```

---

## Part 9: Intermediate - Configuration Files

### MelonPreferences (Built-in)

Already covered in Part 4. Settings auto-save to `ModsConfig/YourMod.json`.

### Custom Config File

```csharp
using System.Text.Json;

[Serializable]
class MyModConfig
{
    public bool EnableFeature = true;
    public int MaxItems = 100;
    public string CustomText = "default";
    public List<string> Whitelist = new();
}

static class ConfigManager
{
    static readonly string ConfigPath = Path.Combine(MelonModManager.ModsDir, "MyModConfig.json");
    public static MyModConfig Instance { get; private set; }

    public static void Load()
    {
        if (File.Exists(ConfigPath))
        {
            var json = File.ReadAllText(ConfigPath);
            Instance = JsonSerializer.Deserialize<MyModConfig>(json);
        }
        else
        {
            Instance = new MyModConfig();
            Save();
        }
    }

    public static void Save()
    {
        var json = JsonSerializer.Serialize(Instance, new JsonSerializerOptions
        { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }
}
```

### fullmod.json (For Full Mods with Assets)

```json
{
    "name": "My Full Mod",
    "version_list": [
        {
            "melonloader": "0.5.0",
            "assets": [
                {
                    "path": "Mods/MyFullMod",
                    "files": [
                        "models/player.fbx",
                        "textures/icon.png",
                        "audio/sound.wav"
                    ]
                }
            ]
        }
    ]
}
```

Place `fullmod.json` in your mod's folder alongside the DLL:
```
Mods/
└── MyFullMod/
    ├── MyMod.dll
    ├── fullmod.json
    ├── models/
    ├── textures/
    └── audio/
```

---

## Part 10: Advanced - Opcode Transpilers

### What Are Transpilers?

Transpilers modify the **IL (Intermediate Language) bytecode** of a method at patch time. This lets you:

- Replace constant values (e.g., change `maxHealth = 100` to `maxHealth = 999`)
- Remove entire code blocks (e.g., skip a check)
- Inject new instructions
- Rewrite control flow

### IL Manipulation Basics

```csharp
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;  // Actually Harmony uses MonoMod.Cil

// The transpiler receives the existing IL instructions
// and returns the modified list
[HarmonyPatch(typeof(GameManager), "GetMaxScore")]
static class ScoreTranspiler
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var code = instructions.ToList();

        // Find and replace ldc.i4.s 100 with ldc.i4.s 999
        for (int i = 0; i < code.Count; i++)
        {
            var instr = code[i];

            // Match: load constant integer 100
            if (instr.opcode == OpCodes.Ldc_I4_S && instr.operand is byte b && b == 100)
            {
                instr.operand = (byte)999;
                MelonLogger.Msg("Replaced max score: 100 → 999");
            }

            // Match: load constant integer (full) 100
            if (instr.opcode == OpCodes.Ldc_I4 && instr.operand is int n && n == 100)
            {
                instr.operand = 999;
            }
        }

        return code;
    }
}
```

### Using CodeInstruction Extensions

```csharp
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    var code = instructions.ToList();

    // Find all calls to a specific method
    var targetMethod = AccessTools.Method(typeof(SomeClass), "SomeMethod");
    var callInstrs = code.Find(x => x.calls == targetMethod);

    foreach (var match in callInstrs)
    {
        // Replace with a call to our method
        match.opcode = OpCodes.Call;
        match.operand = AccessTools.Method(typeof(MyPatches), "ReplacementMethod");
    }

    // Remove an instruction (noop it)
    foreach (var match in code.Find(x => x.opcode == OpCodes.Ret).SkipLast(1))
    {
        match.opcode = OpCodes.Nop;
        match.operand = null;
    }

    return code;
}
```

### Injecting New Instructions

```csharp
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    var code = instructions.ToList();

    // Find the first 'ret' instruction
    var retIndex = code.FindLast(x => x.opcode == OpCodes.Ret);

    // Insert new code before return
    var newInstructions = new List<CodeInstruction>
    {
        new CodeInstruction(OpCodes.Ldstr, "Hello from transpiler!"),
        new CodeInstruction(OpCodes.Call,
            AccessTools.Method(typeof(MelonLoader.MelonLogger), "Msg",
                new[] { typeof(string) })),
    };

    code.InsertRange(retIndex, newInstructions);

    return code;
}
```

### Using Matcher (Cleaner Pattern)

```csharp
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    var code = instructions.ToList();
    var matcher = new CodeMatcher(code)
        .Start()
        .MatchForward(false, new Pattern[]
        {
            // Match pattern: ldarg.0, ldfld, ldc.i4, ble
            new Pattern(OpCodes.Ldarg_0, null),
            new Pattern(OpCodes.Ldfld, null),
            new Pattern(OpCodes.Ldc_I4, null),
            new Pattern(OpCodes.Ble, null),
        });

    if (matcher.MatchIsValid())
    {
        // Replace the constant in the pattern
        matcher.SetOperandAndAdvance((byte)999);

        // Or NOP out the branch (skip the check entirely)
        matcher.SetInstructionAndAdvance(
            new CodeInstruction(OpCodes.Nop));
    }

    return matcher.Instructions();
}
```

### Dumping IL for Analysis

```csharp
// Essential for reverse-engineering what to patch
static void DumpMethod(MethodInfo method)
{
    var instructions = method.GetInstructions();
    foreach (var instr in instructions)
    {
        MelonLogger.Msg($"{instr.opcode} {instr.operand ?? ""}");
    }
}
```

Common opcodes:
| Opcode | Description |
|---|---|
| `ldarg.0` – `ldarg.3` | Load argument 0-3 |
| `ldarg.s` | Load argument (short, with operand) |
| `ldloc.0` – `ldloc.3` | Load local variable 0-3 |
| `ldloc.s` | Load local (short) |
| `stloc.0` – `stloc.3` | Store local 0-3 |
| `stloc.s` | Store local (short) |
| `ldsfld` | Load static field |
| `ldfld` | Load instance field |
| `stsfld` | Store static field |
| `stfld` | Store instance field |
| `ldc.i4.S` | Load int32 constant (sbyte) |
| `ldc.i4` | Load int32 constant |
| `ldc.i4.0` – `ldc.i4.8` | Load constant 0-8 (no operand) |
| `ldc.r4` | Load float32 constant |
| `ldc.r8` | Load float64 constant |
| `ldstr` | Load string literal |
| `call` | Call method |
| `callvirt` | Call virtual method |
| `newobj` | Create new object |
| `br` / `brtrue` / `brfalse` | Branch |
| `ble` / `bgt` / `blt` | Branch if less/greater |
| `ret` | Return |
| `nop` | No operation |

---

## Part 11: Advanced - Multi-Method Patching

### Patching Multiple Methods at Once

```csharp
// Patch all methods that match a pattern
static void ApplyAllPatches(Harmony harmony)
{
    Type targetType = AccessTools.TypeByName("GameNamespace.WeaponManager");

    foreach (var method in AccessTools.GetDeclaredMethods(targetType))
    {
        if (method.Name.StartsWith("Fire"))
        {
            harmony.Patch(method,
                new HarmonyMethod(AccessTools.Method(typeof(WeaponPatches), "Prefix")));
        }
    }
}
```

### Conditional Patching Based on Game State

```csharp
static class SmartPatches
{
    static readonly Harmony harmony = new Harmony("com.mymod.smart");

    public static void ApplyPatches()
    {
        var original = AccessTools.Method(typeof(GameClass), "TargetMethod");

        // Check if the method already has patches (avoid conflicts)
        var patchInfo = original.GetPatchMethod(PatchType.Prefix);
        if (patchInfo == null)
        {
            harmony.Patch(original,
                prefix: new HarmonyMethod(AccessTools.Method(typeof(SmartPatches), "Prefix")));
            MelonLogger.Msg("Patch applied successfully.");
        }
        else
        {
            MelonLogger.Warning("Method already patched by another mod. Skipping.");
        }
    }

    public static void RemovePatches()
    {
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        // Selective unpatch
        var original = AccessTools.Method(typeof(GameClass), "TargetMethod");
        harmony.Unpatch(original, PatchType.Prefix);
    }

    static bool Prefix()
    {
        // Dynamic logic
        return !isFeatureEnabled;
    }
}
```

### Chaining Prefixes (Understanding Execution Order)

When multiple mods patch the same method:
1. **Prefixes** run in priority order (High → Low)
2. If any prefix returns `false`, the chain stops and the original is skipped
3. **Postfixes** always run (even if original was skipped), in reverse priority order

```csharp
// Mod A (High Priority)
[HarmonyPriority(Priority.High)]
static bool PrefixA()
{
    MelonLogger.Msg("A runs first");
    return true;  // Continue chain
}

// Mod B (Normal Priority)
static bool PrefixB()
{
    MelonLogger.Msg("B runs second");
    return false;  // Skip original, but A already ran
}
```

### Generic Patching with Delegates

```csharp
static class DelegatePatcher
{
    // Create a delegate to call the original after unpatching
    static Action<object, int> originalMethod;

    public static void Setup()
    {
        var method = AccessTools.Method(typeof(Target), "TargetMethod");

        // Create a wrapper that calls the original
        originalMethod = method.CreateDelegate<Action<object, int>>() as Action<object, int>;
    }

    public static void CallOriginal(object instance, int value)
    {
        originalMethod?.Invoke(instance, value);
    }
}
```

---

## Part 12: Advanced - Custom Commands & Console

### MelonHandler Commands

```csharp
public class MyMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        // Register a console command
        MelonCommands.MelonCommand("godmode", ToggleGodMode,
            "Toggle god mode. Usage: /godmode [true|false]");

        MelonCommands.MelonCommand("tp", Teleport,
            "Teleport to coordinates. Usage: /tp x y z");
    }

    static void ToggleGodMode(string command, params string[] args)
    {
        bool enabled = args.Length > 0 && args[0].ToLower() == "true";
        isGodMode = enabled;
        MelonLogger.Msg($"God mode: {(enabled ? "ON" : "OFF")}");
    }

    static void Teleport(string command, params string[] args)
    {
        if (args.Length >= 3 &&
            float.TryParse(args[0], out float x) &&
            float.TryParse(args[1], out float y) &&
            float.TryParse(args[2], out float z))
        {
            playerTransform.position = new Vector3(x, y, z);
            MelonLogger.Msg($"Teleported to {x}, {y}, {z}");
        }
        else
        {
            MelonLogger.Warning("Usage: /tp x y z");
        }
    }
}
```

### MelonInput (Keybinds)

```csharp
public class MyMod : MelonMod
{
    public static MelonKeybind jumpKeybind;
    public static MelonKeybind sprintKeybind;
    public static MelonKeybindCombo comboKeybind;

    public override void OnInitializeMelon()
    {
        // Single keybind
        jumpKeybind = new MelonKeybind(
            "Super Jump",           // Name
            KeyCode.LeftControl,     // Default key
            KeybindMode.Press,       // Press / Release / Toggle
            OnJump,                  // Callback
            "My Mod"                 // Settings section
        );

        // Sprint toggle
        sprintKeybind = new MelonKeybind(
            "Sprint Toggle",
            KeyCode.LeftShift,
            KeybindMode.Toggle,
            OnSprintToggle,
            "My Mod"
        );

        // Key combo (e.g., Ctrl + Shift + J)
        comboKeybind = new MelonKeybindCombo(
            "Dash Combo",
            KeyCode.J,
            KeyCode.LeftControl | KeyCode.LeftShift,
            OnDash,
            "My Mod"
        );
    }

    static bool OnJump(bool isActive)
    {
        if (isActive)
        {
            MelonLogger.Msg("Super jump activated!");
        }
        return false;  // false = allow game to also receive the key
    }

    static void OnSprintToggle(bool isActive)
    {
        MelonLogger.Msg($"Sprint: {(isActive ? "ON" : "OFF")}");
    }
}
```

---

## Part 13: Advanced - Save System & Persistence

### Persisting Custom Data

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSerializable]
class PlayerSaveData
{
    public string PlayerName;
    public Vector3 Position;
    public int Score;
    public Dictionary<string, object> CustomData = new();

    [JsonPropertyName("posX")] public float PosX { get => Position.x; set => Position = new Vector3(value, Position.y, Position.z); }
    [JsonPropertyName("posY")] public float PosY { get => Position.y; set => Position = new Vector3(Position.x, value, Position.z); }
    [JsonPropertyName("posZ")] public float PosZ { get => Position.z; set => Position = new Vector3(Position.x, Position.y, value); }
}

static class SaveManager
{
    static string SavePath => Path.Combine(MelonModManager.ModsDir, "MyMod", "save.json");

    public static void Save(PlayerSaveData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(SavePath, JsonSerializer.Serialize(data, options));
        MelonLogger.Msg("Game saved.");
    }

    public static PlayerSaveData Load()
    {
        if (!File.Exists(SavePath)) return null;
        var json = File.ReadAllText(SavePath);
        return JsonSerializer.Deserialize<PlayerSaveData>(json);
    }

    public static void Delete()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
```

### Hooking Game Save/Load

```csharp
// Patch the game's save method to inject your data
[HarmonyPatch(typeof(GameSaveSystem), "SaveGame")]
static class SaveHook
{
    static void Postfix()
    {
        // Game just saved — save our data too
        var data = GetCurrentPlayerData();
        SaveManager.Save(data);
    }
}

[HarmonyPatch(typeof(GameSaveSystem), "LoadGame")]
static class LoadHook
{
    static void Postfix()
    {
        // Game just loaded — restore our data
        var data = SaveManager.Load();
        if (data != null) ApplyPlayerData(data);
    }
}
```

---

## Part 14: Advanced - Networking & Multiplayer

### Hooking Game Network Messages

```csharp
// Find the network manager
Type netManager = AccessTools.TypeByName("NetworkManager");

// Patch send/receive
[HarmonyPatch(netManager, "Send")]
static class NetworkSendPatch
{
    static void Prefix(byte[] data, int channelId)
    {
        MelonLogger.Msg($"Sending {data.Length} bytes on channel {channelId}");
    }
}

// Intercept received messages
[HarmonyPatch(netManager, "OnReceive")]
static class NetworkReceivePatch
{
    static bool Prefix(NetworkMessage msg)
    {
        // Read the message type
        var messageType = msg.Read<int>();
        MelonLogger.Msg($"Received message type: {messageType}");
        return true;  // Let the game process it too
    }
}
```

### Custom Network Messages

```csharp
// Inject custom network data into existing messages
static void SendCustomMessage(string text)
{
    // Use the game's existing send infrastructure
    var method = AccessTools.Method(netManager, "Send");
    // Build a byte array with your custom message type
    using var ms = new MemoryStream();
    using var bw = new BinaryWriter(ms);
    bw.Write(9999);  // Custom message type ID
    bw.Write(text);
    method.Invoke(networkManagerInstance, new object[] { ms.ToArray(), 0 });
}
```

---

## Part 15: Expert - IL Injection & Dynamic Assemblies

### Dynamic Assembly Generation

```csharp
using System.Reflection.Emit;

static class DynamicAssemblyBuilder
{
    public static Assembly CreateModAssembly(string name, Type baseType)
    {
        var assemblyName = new AssemblyName(name);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName, AssemblyBuilderAccess.Run);

        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType(
            $"{name}Type",
            TypeAttributes.Public | TypeAttributes.Class,
            baseType);

        // Add a method
        var methodBuilder = typeBuilder.DefineMethod(
            "OnInitializeMelon",
            MethodAttributes.Public | MethodAttributes.Override,
            typeof(void),
            Type.EmptyTypes);

        var il = methodBuilder.GetILGenerator();

        // Emit: MelonLogger.Msg("Dynamic mod loaded!")
        il.Emit(OpCodes.Ldstr, "Dynamic mod loaded!");
        il.Emit(OpCodes.Call, AccessTools.Method(
            typeof(MelonLogger), "Msg", new[] { typeof(string) }));
        il.Emit(OpCodes.Ret);

        typeBuilder.DefineMethodOverride(methodBuilder,
            AccessTools.Method(baseType, "OnInitializeMelon"));

        var createdType = typeBuilder.CreateType();
        return assemblyBuilder;
    }
}
```

### Runtime IL Patching with MonoMod

```csharp
using MonoMod.RuntimeDetour;

static class RuntimeDetourExample
{
    static Detour detour;

    public static void Apply(MethodInfo original, MethodInfo replacement)
    {
        detour = new Detour(original, replacement);
        detour.Apply();
        MelonLogger.Msg("Runtime detour applied.");
    }

    public static void Remove()
    {
        detour?.Undo();
        detour?.Dispose();
    }
}
```

### Hot Reload (Development Tool)

```csharp
static class HotReload
{
    static string dllPath;
    static Assembly lastAssembly;

    public static void WatchAndReload(string path)
    {
        dllPath = path;
        lastAssembly = Assembly.LoadFrom(path);

        var timer = new System.Timers.Timer(2000);  // Check every 2s
        timer.Elapsed += (s, e) =>
        {
            try
            {
                var current = Assembly.LoadFrom(path);
                if (current != lastAssembly)
                {
                    MelonLogger.Msg("Hot reload detected!");
                    lastAssembly = current;
                    // Re-apply patches, re-init features, etc.
                }
            }
            catch { }  // File in use by old assembly — ignore
        };
        timer.Start();
    }
}
```

---

## Part 16: Expert - Asset Loading & Resource Management

### Loading Custom Assets at Runtime

```csharp
using UnityEngine;
using System.IO;

static class AssetLoader
{
    static string AssetsDir => Path.Combine(MelonModManager.ModsDir, "MyMod", "Assets");

    // Load a Texture2D from file
    public static Texture2D LoadTexture(string fileName)
    {
        var path = Path.Combine(AssetsDir, "Textures", fileName);
        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    // Load an AudioClip (OGG/WAV)
    public static AudioClip LoadAudio(string fileName)
    {
        var path = Path.Combine(AssetsDir, "Audio", fileName);
        var bytes = File.ReadAllBytes(path);

        AudioClip clip = null;
        if (fileName.EndsWith(".wav"))
            AudioImporter.ImportSampleWAV(bytes, out clip, out _, out _, out _);
        // For OGG, use WWW or UnityWebRequest
        return clip;
    }

    // Load a Model (requires AssetBundle or external loader)
    public static GameObject LoadModelFromBundle(string bundleName, string assetName)
    {
        var path = Path.Combine(AssetsDir, "Bundles", bundleName);
        var bundle = AssetBundle.LoadFromFile(path);
        var go = bundle.LoadAsset<GameObject>(assetName);
        return Instantiate(go);
    }

    // Apply material to a GameObject
    public static void ApplyMaterial(GameObject target, Material mat)
    {
        var renderers = target.GetComponentsInChildren<SkinnedMeshRenderer>()
            .Concat(target.GetComponentsInChildren<MeshRenderer>()).ToList();
        foreach (var r in renderers)
            r.sharedMaterial = mat;
    }
}
```

### Building AssetBundles

```csharp
// This runs in the Unity Editor — NOT in your mod
// Build script for creating AssetBundles:

using UnityEditor;
using UnityEngine;

public class BuildAssetBundles
{
    [MenuItem("Mods/Build AssetBundles")]
    static void Build()
    {
        var outputPath = Path.Combine(Application.dataPath, "..", "ModAssets", "Bundles");
        Directory.CreateDirectory(outputPath);

        // Assign asset bundle names in editor first, then:
        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);
    }
}
```

---

## Part 17: Expert - Inter-Mod Communication

### MelonModInteraction

```csharp
// Access other loaded mods
foreach (var mod in MelonModManager.Mods)
{
    MelonLogger.Msg($"Loaded: {mod.Info.Name} v{mod.Info.Version}");

    // Check if a specific mod is loaded
    if (mod.Info.Name == "OtherMod")
    {
        // Access public members via reflection
        var type = mod.GetType();
        var field = type.GetField("SharedVariable");
        if (field != null)
        {
            var value = field.GetValue(mod);
        }
    }
}
```

### Mod Dependency System

```csharp
[MelonModName("My Mod")]
[MelonModRequiredDependencies("com.other.mod")]  // Won't load without it
[MelonModOptionalDependencies("com.optional.mod")]  // Works without, but enhances with
public class MyMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        // Check for optional dependency
        var optionalMod = MelonModManager.Mods
            .FirstOrDefault(m => m.Info.Name == "Optional Mod");

        if (optionalMod != null)
        {
            MelonLogger.Msg("Optional mod detected! Enabling enhanced features.");
            EnableEnhancedFeatures(optionalMod);
        }
    }
}
```

### Event-Based Communication (Custom)

```csharp
// Publisher mod
public static class ModEvents
{
    public delegate void PlayerDamagedDelegate(float damage);
    public static event PlayerDamagedDelegate OnPlayerDamaged;

    public static void RaisePlayerDamaged(float damage)
    {
        OnPlayerDamaged?.Invoke(damage);
    }
}

// Subscriber mod (in a different DLL)
public class SubscriberMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        // Find the event via reflection
        var eventsType = AccessTools.TypeByName("ModEvents");
        var eventInfo = AccessTools.Property(eventsType, "OnPlayerDamaged");

        // Subscribe
        var addMethod = eventInfo.AddEventMethod;
        addMethod.Invoke(null, new object[] { OnPlayerDamaged });
    }

    void OnPlayerDamaged(float damage)
    {
        MelonLogger.Msg($"Player took {damage} damage!");
    }
}
```

### Shared Library Pattern

```csharp
// Create a shared DLL that both mods reference
// SharedAPI.dll:
namespace SharedAPI
{
    public interface IModInterface
    {
        string ModName { get; }
        void DoSomething(int value);
    }

    public static class ModRegistry
    {
        public static Dictionary<string, IModInterface> Mods = new();

        public static void Register(string key, IModInterface mod) => Mods[key] = mod;
        public static IModInterface Get(string key) => Mods.TryGetValue(key, out var m) ? m : null;
    }
}

// In Mod A:
ModRegistry.Register("ModA", this);

// In Mod B:
var modA = ModRegistry.Get("ModA");
modA?.DoSomething(42);
```

---

## Part 18: Expert - Performance & Profiling

### Performance Best Practices

```csharp
// BAD: Allocating every frame
public override void OnUpdateMelon()
{
    var list = new List<GameObject>();  // GC allocation every frame!
    // ...
}

// GOOD: Reuse allocations
List<GameObject> cachedList = new();
public override void OnUpdateMelon()
{
    cachedList.Clear();  // No allocation
    // ...
}

// BAD: GameObject.Find every frame
public override void OnUpdateMelon()
{
    var player = GameObject.Find("Player");  // Slow!
}

// GOOD: Cache references
static Transform playerTransform;
public override void OnInitializeMelon()
{
    playerTransform = GameObject.Find("Player").transform;  // Find once
}

public override void OnUpdateMelon()
{
    var pos = playerTransform.position;  // Fast
}
```

### Profiling Your Mod

```csharp
using System.Diagnostics;

static class ModProfiler
{
    static readonly Dictionary<string, Stopwatch> timers = new();
    static readonly Dictionary<string, double> totals = new;
    static readonly Dictionary<string, int> counts = new;

    public static void Start(string name)
    {
        if (!timers.ContainsKey(name))
        {
            timers[name] = new Stopwatch();
            totals[name] = 0;
            counts[name] = 0;
        }
        timers[name].Start();
    }

    public static void Stop(string name)
    {
        if (timers.TryGetValue(name, out var sw))
        {
            sw.Stop();
            totals[name] += sw.ElapsedMilliseconds;
            counts[name]++;
            sw.Reset();
        }
    }

    public static void Report()
    {
        MelonLogger.Msg("=== Performance Report ===");
        foreach (var (name, total) in totals)
        {
            var avg = total / counts[name];
            MelonLogger.Msg($"  {name}: {total}ms total, {avg:F2}ms avg ({counts[name]} calls)");
        }
    }
}

// Usage:
ModProfiler.Start("MyExpensiveOperation");
// ... do work ...
ModProfiler.Stop("MyExpensiveOperation");
```

### Frame-Budgeting Patches

```csharp
// Use FixedUpdate for physics, LateUpdate for camera, Update for logic
// Avoid heavy work in OnUpdateMelon — use coroutines or workers

// Coroutine pattern
public class MyMod : MelonMod
{
    static Coroutine heavyWorkCoroutine;

    public override void OnInitializeMelon()
    {
        // Run heavy work spread across frames
        heavyWorkCoroutine = MelonCoroutines.Start(DoHeavyWork());
    }

    static IEnumerator DoHeavyWork()
    {
        for (int i = 0; i < 1000; i++)
        {
            ProcessItem(i);
            yield return null;  // Wait one frame
        }
        MelonLogger.Msg("Heavy work complete!");
    }
}
```

---

## Part 19: Expert - Anti-Patterns & Pitfalls

### Common Mistakes

#### 1. Hardcoded Assembly Names
```csharp
// BAD: Breaks when game updates
Type t = typeof(Assembly-CSharp.SomeClass);

// GOOD: Use string-based lookup
Type t = AccessTools.TypeByName("SomeNamespace.SomeClass");
```

#### 2. Patching Without Error Handling
```csharp
// BAD: Crashes the game if the type doesn't exist
[HarmonyPatch(typeof(MaybeRenamedClass), "Method")]

// GOOD: Conditional patching
public override void OnInitializeMelon()
{
    var type = AccessTools.TypeByName("MaybeNamespace.MaybeRenamedClass");
    if (type != null)
    {
        harmony.Patch(AccessTools.Method(type, "Method"),
            prefix: new HarmonyMethod(typeof(Patches), "Prefix"));
    }
}
```

#### 3. Memory Leaks
```csharp
// BAD: Creating objects without cleaning up
public override void OnUpdateMelon()
{
    var go = new GameObject();  // Never destroyed!
}

// GOOD: Track and clean
List<GameObject> createdObjects = new();
// ...
foreach (var go in createdObjects) Destroy(go);
createdObjects.Clear();
```

#### 4. Blocking the Main Thread
```csharp
// BAD: Freezes the game
public override void OnInitializeMelon()
{
    Thread.Sleep(5000);  // Blocks Unity's main thread!
    File.ReadAllBytes(hugeFile);  // Sync I/O on main thread
}

// GOOD: Background thread
public override void OnInitializeMelon()
{
    Task.Run(() =>
    {
        var data = File.ReadAllBytes(hugeFile);
        // Back to main thread for Unity calls:
        MelonCoroutines.ExecAsync(() => ProcessData(data));
    });
}
```

#### 5. Ignoring Patch Conflicts
```csharp
// BAD: Overwrites other mod's patches
harmony.Patch(original, prefix: new HarmonyMethod(myPrefix));

// GOOD: Check for existing patches
var patchInfo = HarmonyGetPatchInfo(original);
if (patchInfo.Prefixes.Any())
{
    MelonLogger.Warning("Method already has a prefix. Coexisting carefully.");
}
```

#### 6. Not Handling IL2CPP Differences
```csharp
// IL2CPP may have different method signatures or class layouts
// Always verify types exist and use AccessTools generously

#if IL2CPP
    // IL2CPP-specific handling
#else
    // Mono-specific handling
#endif
```

### Debugging Checklist

| Symptom | Likely Cause | Fix |
|---|---|---|
| Mod doesn't load | Wrong target framework | Match game's Unity version |
| Mod doesn't load | Missing MelonModInfo | Add required attributes |
| Patch doesn't apply | Wrong method signature | Dump IL, check parameter types |
| Patch doesn't apply | Type name changed | Use AccessTools.TypeByName |
| Game crashes on launch | Patch throws exception | Wrap in try/catch, check logs |
| GUI doesn't show | Wrong render mode | Check canvas/camera setup |
| Mod conflicts | Same patch target | Use HarmonyPriority, check conflicts |
| Performance drop | Allocating in OnUpdate | Cache, reuse, profile |

---

## Appendix A: Quick Reference Cheatsheet

### Minimal Mod
```csharp
using MelonLoader;

[MelonModName("My Mod")]
[MelonModAuthor("Me")]
[MelonModVersion("1.0.0")]
public class MyMod : MelonMod
{
    public override void OnInitializeMelon() => MelonLogger.Msg("Loaded!");
}
```

### Harmony Patch Template
```csharp
using HarmonyLib;
using MelonLoader;

[HarmonyPatch]
static class MyPatch
{
    static bool Prefix() => MelonLogger.Msg("Before!"), true;
    static void Postfix(ref int __result) => __result = 42;

    static bool Prepare()
    {
        var type = AccessTools.TypeByName("Namespace.Class");
        var method = AccessTools.Method(type, "Method");
        HarmonyPatch(targetmethod: method);
        return type != null && method != null;
    }
}
```

### Key MelonLoader APIs
| API | Purpose |
|---|---|
| `MelonLogger.Msg()` | Log message |
| `MelonLogger.Warning()` | Log warning |
| `MelonLogger.Error()` | Log error |
| `MelonLogger.DelayedCall()` | Log with delay |
| `MelonModManager.Mods` | All loaded mods |
| `MelonModManager.ModsDir` | Path to Mods folder |
| `MelonPreferences` | Settings system |
| `MelonCommands` | Console commands |
| `MelonKeybind` | Key bindings |
| `MelonCoroutines` | Unity coroutines |

### Harmony __ Identifiers
| Identifier | Meaning |
|---|---|
| `__instance` | The `this` of the original method |
| `__result` | Return value (modifiable in postfix) |
| `__state` | Shared state between prefix/postfix |
| `__target` | The actual target instance (for extension methods) |
| `__originalMethod` | The original MethodInfo |
| `__runningMethod` | The currently running MethodBase |

---

## Appendix B: Common Games & Their Quirks

| Game | Unity | Type | Notes |
|---|---|---|---|
| Boneworks | 2019.4 | IL2CPP | Uses custom input, VR-focused |
| Bonsai | 2019.4 | IL2CPP | Sequel to Boneworks |
| People Playground | 2019.3 | Mono | Straightforward API |
| Lethal Company | 2021.3 | IL2CPP | NetworkManager-based multiplayer |
| Content Warning | 2022.x | IL2CPP | Lethal Company successor |
| Goat Simulator 3 | 2021.3 | IL2CPP | Standard Unity patterns |
| Assetto Corsa | — | — | **Not Unity** — uses own mod system |

---

## Appendix C: Debugging Checklist

### Mod Won't Load
1. Check `MelonLoader.txt` in the game folder for errors
2. Verify target framework matches the game
3. Ensure `MelonModInfo` attributes are present
4. Check that referenced assemblies exist in the game

### Patches Don't Work
1. Use `DumpMethod()` to verify the method exists
2. Check if another mod already patches it
3. Verify parameter types match exactly (including generics)
4. Try `Prepare()` method for conditional patching

### Game Crashes
1. Check MelonLoader logs (`MelonLoader.txt`)
2. Wrap patches in try/catch
3. Test with only your mod loaded
4. Use Visual Studio remote attach for debugging

### Visual Studio Remote Debugging
1. In the game folder, create `vswhere.json` or use:
   ```
   MelonLoader/options.json → "Debug.AttachVs": true
   ```
2. Launch Visual Studio → Debug → Attach to Process
3. Attach to the game process
4. Set breakpoints in your mod code

---

## Further Resources

- [MelonLoader GitHub](https://github.com/LachesisUS/MelonLoader)
- [MelonLoader Documentation](https://docs.melonmod.com)
- [Harmony Documentation](https://harmony.pardeike.net/)
- [MonoMod Documentation](https://github.com/MonoMod/MonoMod)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)

---

*This guide covers the complete spectrum of MelonLoader modding. Work through it sequentially for the best learning experience. Example mods for each level are in the `Example Mods/` folder.*
