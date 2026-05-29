// ============================================================
// Project Template - Base MelonLoader Mod
// ============================================================
// Copy this folder to start a new mod project!
//
// Setup:
// 1. Rename this folder to your mod name
// 2. Edit MelonMod.cs with your mod details
// 3. Build with: dotnet build
// 4. Copy the .dll to your game's mods folder
//
// Folder structure:
// YourMod/
//   ├── MelonMod.cs          (main mod class)
//   ├── YourMod.csproj       (project file)
//   ├── README.md            (this file)
//   └── assets/              (optional: textures, models, etc.)
// ============================================================

using System;
using MelonLoader;
using UnityEngine;

namespace YourModNamespace
{
    /// <summary>
    /// Your mod's main class. Extend MelonMod to hook into
    /// the game's lifecycle and add your functionality.
    /// </summary>
    [assembly: MelonInfo_name("Your Mod Name")]
    [assembly: MelonInfo_author("Your Name")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Your mod description here")]

    public class YourMod : MelonMod
    {
        /// <summary>
        /// Called when the mod is loaded.
        /// Initialize your mod here.
        /// </summary>
        override public void OnApplicationStart()
        {
            MelonLogger.Msg("Your mod is now active!");

            // TODO: Add your initialization code here
            // - Set up Harmony patches
            // - Load configuration
            // - Register commands
            // - Spawn objects
        }

        /// <summary>
        /// Called every frame.
        /// Similar to Unity's Update().
        /// </summary>
        override public void OnUpdate()
        {
            // TODO: Add per-frame logic here
            // - Check input
            // - Update game state
            // - Render UI
        }

        /// <summary>
        /// Called when drawing IMGUI.
        /// Use this for custom HUD elements.
        /// </summary>
        override public void OnGUI()
        {
            // TODO: Add custom UI here
            // GUILayout.Label("Hello World!");
        }

        /// <summary>
        /// Called when a scene is loaded.
        /// Use this for scene-specific setup.
        /// </summary>
        override public void OnSceneWasLoaded(int sceneBuildIndex, string sceneName)
        {
            MelonLogger.Msg($"Scene loaded: {sceneName}");

            // TODO: Add scene-specific logic here
            // - Find game objects
            // - Apply modifications
            // - Spawn custom content
        }

        /// <summary>
        /// Called when the game is closing.
        /// Clean up resources here.
        /// </summary>
        override public void OnApplicationQuit()
        {
            MelonLogger.Msg("Your mod is being unloaded.");

            // TODO: Clean up here
            // - Unpatch Harmony patches
            // - Save data
            // - Destroy custom objects
        }
    }
}
