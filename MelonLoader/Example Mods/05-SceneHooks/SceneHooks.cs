// ============================================================
// Example 05: Scene Hooks (Intermediate)
// ============================================================
// Demonstrates:
// - Reacting to scene load/unload events
// - Accessing scene objects and components
// - Conditional behavior based on scene name
// - Loading/unloading scene-specific resources
//
// Scene hooks are essential for game-specific mods
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;

namespace SceneHooksExample
{
    [assembly: MelonInfo_name("Scene Hooks Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates scene lifecycle hooks")]

    public class SceneHooksExample : MelonMod
    {
        // Track scene state
        private string _currentSceneName = "";
        private int _sceneLoadCount = 0;

        // Scene-specific data
        private Dictionary<string, SceneData> _sceneData = new Dictionary<string, SceneData>();

        override public void OnSceneWasLoaded(int sceneBuildIndex, string sceneName)
        {
            MelonLogger.Msg($"[Scene] Loaded: {sceneName} (index {sceneBuildIndex})");

            _currentSceneName = sceneName;
            _sceneLoadCount++;

            // Handle scene-specific initialization
            OnSceneInitialized(sceneName);
        }

        override public void OnSceneWasUnloaded(int sceneBuildIndex, string sceneName)
        {
            MelonLogger.Msg($"[Scene] Unloaded: {sceneName} (index {sceneBuildIndex})");

            // Clean up scene-specific resources
            OnSceneCleanup(sceneName);
        }

        /// <summary>
        /// Initialize scene-specific functionality
        /// </summary>
        private void OnSceneInitialized(string sceneName)
        {
            switch (sceneName)
            {
                case "MainMenu":
                    InitializeMainMenu();
                    break;

                case "GameLevel":
                    InitializeGameLevel();
                    break;

                case "Settings":
                    InitializeSettingsScene();
                    break;

                default:
                    MelonLogger.Msg($"[Scene] Unknown scene: {sceneName}");
                    break;
            }
        }

        /// <summary>
        /// Example: Initialize main menu modifications
        /// </summary>
        private void InitializeMainMenu()
        {
            MelonLogger.Msg("[MainMenu] Initializing menu mods...");

            // Find specific UI elements
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                MelonLogger.Msg($"[MainMenu] Found Canvas: {canvas.name}");
            }

            // Add custom button to menu
            var go = new GameObject("CustomMenuButton");
            var button = go.AddComponent<UnityEngine.UI.Button>();

            MelonLogger.Msg("[MainMenu] Added custom button");
        }

        /// <summary>
        /// Example: Initialize game level modifications
        /// </summary>
        private void InitializeGameLevel()
        {
            MelonLogger.Msg("[GameLevel] Initializing level mods...");

            // Find player object
            var player = FindObjectOfType<Player>();
            if (player != null)
            {
                MelonLogger.Msg($"[GameLevel] Found player: {player.name}");

                // Apply scene-specific modifications
                ApplyPlayerModifications(player);
            }

            // Find all enemies
            var enemies = FindObjectsOfType<Enemy>();
            MelonLogger.Msg($"[GameLevel] Found {enemies.Length} enemies");

            // Log scene objects
            LogSceneObjects();
        }

        /// <summary>
        /// Example: Initialize settings scene
        /// </summary>
        private void InitializeSettingsScene()
        {
            MelonLogger.Msg("[Settings] Injecting custom settings...");

            // Hook into settings UI
            var settingsPanel = GameObject.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                MelonLogger.Msg($"[Settings] Found settings panel: {settingsPanel.name}");
            }
        }

        /// <summary>
        /// Example: Apply modifications to player in-game
        /// </summary>
        private void ApplyPlayerModifications(Player player)
        {
            // Modify player stats based on scene
            // Example: Double starting health in game level
            // player.Health *= 2;
        }

        /// <summary>
        /// Log all objects in the current scene
        /// </summary>
        private void LogSceneObjects()
        {
            var scene = SceneManager.GetActiveScene();
            MelonLogger.Msg($"[Scene] Active scene: {scene.name}");
            MelonLogger.Msg($"[Scene] Object count: {scene.rootCount}");

            // Log root objects
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                MelonLogger.Msg($"[Scene] Root object: {rootObj.name}");
            }
        }

        /// <summary>
        /// Clean up scene-specific resources
        /// </summary>
        private void OnSceneCleanup(string sceneName)
        {
            if (_sceneData.ContainsKey(sceneName))
            {
                _sceneData[sceneName].Cleanup();
                _sceneData.Remove(sceneName);
                MelonLogger.Msg($"[Scene] Cleaned up data for: {sceneName}");
            }
        }

        /// <summary>
        /// Example: Get scene-specific data
        /// </summary>
        public SceneData GetSceneData(string sceneName)
        {
            if (!_sceneData.ContainsKey(sceneName))
            {
                _sceneData[sceneName] = new SceneData(sceneName);
            }
            return _sceneData[sceneName];
        }

        override public void OnUpdate()
        {
            // Press F8 to log scene info
            if (Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("=== Scene Info ===");
                MelonLogger.Msg($"Current Scene: {_currentSceneName}");
                MelonLogger.Msg($"Scene Load Count: {_sceneLoadCount}");
                MelonLogger.Msg($"Scene Count: {SceneManager.sceneCount}");

                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    MelonLogger.Msg($"  [{i}] {scene.name} (loaded: {scene.isLoaded})");
                }
            }
        }
    }

    /// <summary>
    /// Example: Scene-specific data container
    /// </summary>
    public class SceneData
    {
        public string SceneName { get; }
        public DateTime LoadedAt { get; }
        public Dictionary<string, object> CustomData { get; }

        public SceneData(string sceneName)
        {
            SceneName = sceneName;
            LoadedAt = DateTime.Now;
            CustomData = new Dictionary<string, object>();
        }

        public void Cleanup()
        {
            CustomData.Clear();
        }
    }
}
