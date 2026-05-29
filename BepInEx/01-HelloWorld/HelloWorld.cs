// ═══════════════════════════════════════════════════════════
// BEGINNER: Your First BepInEx Plugin
// Build → drop DLL into BepInEx/plugins/ → launch game
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Logging;
using UnityEngine;

// [BepInPlugin(GUID, Name, Version)]
// GUID must be UNIQUE across all mods. Use reverse-DNS style.
[BepInPlugin(
    guid: "com.example.helloworld",
    name: "Hello World",
    version: "1.0.0"
)]
public class HelloWorldPlugin : BaseUnityPlugin
{
    // Logger is injected by BepInEx — use it instead of Debug.Log
    // Config is also injected if you need settings (see ConfigExample)

    private void Awake()
    {
        // Awake() is the first lifecycle method called
        Logger.LogInfo($"HelloWorld plugin loaded! (BepInEx v{BepInEx.BepInEx.Version})");
        Logger.LogInfo($"Game: {Application.title} v{Application.version}");
        Logger.LogInfo($"Unity: {Application.unityVersion}");
        Logger.LogInfo($"Platform: {Application.platform}");

        // This is where you'd set up Harmony patches, config, etc.
    }

    private void Start()
    {
        // Start() fires after ALL plugins have called Awake()
        // Safe to reference other plugins here
        Logger.LogInfo("All plugins are awake. Safe to interop.");
    }
}
