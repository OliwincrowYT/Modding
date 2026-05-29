// ============================================================
// Example 01: Hello World Mod (Beginner)
// ============================================================
// The simplest possible MelonLoader mod. Demonstrates:
// - MelonMod base class
// - Required attributes
// - Lifecycle methods
//
// Build: Compile as a .dll, then drop into game's mods folder
// ============================================================

using System;
using MelonLoader;

namespace HelloWorldMod
{
    /// <summary>
    /// A minimal mod that logs messages at each lifecycle stage.
    /// </summary>
    [assembly: MelonInfo_name("Hello World Mod")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Your first MelonLoader mod!")]

    public class HelloWorldMod : MelonMod
    {
        /// <summary>
        /// Called when MelonLoader loads the mod assembly.
        /// Use this for early initialization that doesn't depend on game state.
        /// </summary>
        override public void OnApplicationStart()
        {
            MelonLogger.Msg("HelloWorldMod: Application starting!");
            MelonLogger.Msg($"Game: {MelonHandlers.UnityApplication.name}");
            MelonLogger.Msg($"MelonLoader version: {MelonLoader.Version}");
        }

        /// <summary>
        /// Called after the game's first scene is loaded.
        /// Safe to access most game objects and components here.
        /// </summary>
        override public void OnSceneWasLoaded(int sceneBuildIndex, string sceneName)
        {
            MelonLogger.Msg($"HelloWorldMod: Scene loaded - {sceneName} (index {sceneBuildIndex})");
        }

        /// <summary>
        /// Called every frame while the mod is enabled.
        /// Similar to Unity's Update(). Avoid heavy operations here.
        /// </summary>
        override public void OnUpdate()
        {
            // Example: Press F5 to log a message
            if (Input.GetKeyDown(KeyCode.F5))
            {
                MelonLogger.Msg("HelloWorldMod: F5 was pressed! Hello, world!");
            }
        }

        /// <summary>
        /// Called when the mod is being unloaded.
        /// Clean up resources here.
        /// </summary>
        override public void OnApplicationQuit()
        {
            MelonLogger.Msg("HelloWorldMod: Goodbye! Application quitting.");
        }
    }
}
