// ============================================================
// Example 07: Save System (Advanced)
// ============================================================
// Demonstrates:
// - Persisting mod data to disk
// - JSON serialization/deserialization
// - Save/load with versioning
// - Automatic save on game quit
// - Manual save triggers
//
// Save data stored in: MelonLoader/mods/<ModName>/
// ============================================================

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MelonLoader;

namespace SaveSystemExample
{
    [assembly: MelonInfo_name("Save System Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates persistent save system")]

    public class SaveSystemExample : MelonMod
    {
        // Save file path
        private static readonly string _saveDirectory;
        private static readonly string _saveFilePath;

        // Current save data
        private GameSaveData _saveData;

        // JSON serializer options
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        static SaveSystemExample()
        {
            _saveDirectory = Path.Combine(MelonModManager.ModsDir, "SaveSystemExample");
            _saveFilePath = Path.Combine(_saveDirectory, "save.json");
        }

        override public void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing save system...");

            // Create save directory if it doesn't exist
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
                MelonLogger.Msg($"Created save directory: {_saveDirectory}");
            }

            // Load existing save or create new one
            _saveData = LoadSave();
            if (_saveData == null)
            {
                _saveData = CreateNewSave();
                MelonLogger.Msg("Created new save file");
            }
            else
            {
                MelonLogger.Msg($"Loaded save: {_saveData.SaveName}");
            }

            // Log current stats
            LogStats();
        }

        override public void OnApplicationQuit()
        {
            // Auto-save on quit
            SaveGame();
            MelonLogger.Msg("Auto-saved on quit");
        }

        override public void OnUpdate()
        {
            // Press F9 to save
            if (Input.GetKeyDown(KeyCode.F9))
            {
                SaveGame();
                MelonLogger.Msg("Manual save completed");
            }

            // Press F10 to load
            if (Input.GetKeyDown(KeyCode.F10))
            {
                _saveData = LoadSave();
                if (_saveData != null)
                {
                    LogStats();
                    MelonLogger.Msg("Save loaded");
                }
            }

            // Press F11 to reset save
            if (Input.GetKeyDown(KeyCode.F11))
            {
                _saveData = CreateNewSave();
                SaveGame();
                MelonLogger.Msg("Save reset to defaults");
            }
        }

        /// <summary>
        /// Load save data from file
        /// </summary>
        private GameSaveData LoadSave()
        {
            if (!File.Exists(_saveFilePath))
            {
                MelonLogger.Msg("No save file found");
                return null;
            }

            try
            {
                string json = File.ReadAllText(_saveFilePath);
                var saveData = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions);

                // Handle save versioning
                if (saveData.SaveVersion < 1)
                {
                    MelonLogger.Msg("Upgrading old save file format");
                    saveData.UpgradeFromOldFormat();
                }

                MelonLogger.Msg($"Save loaded from: {_saveFilePath}");
                return saveData;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to load save: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save current game data to file
        /// </summary>
        private void SaveGame()
        {
            if (_saveData == null)
            {
                _saveData = CreateNewSave();
            }

            // Update timestamps
            _saveData.LastSaved = DateTime.Now;
            _saveData.PlayTimeSeconds += Time.deltaTime;

            try
            {
                string json = JsonSerializer.Serialize(_saveData, _jsonOptions);
                File.WriteAllText(_saveFilePath, json);
                MelonLogger.Msg($"Game saved to: {_saveFilePath}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to save game: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new save with default values
        /// </summary>
        private GameSaveData CreateNewSave()
        {
            return new GameSaveData
            {
                SaveName = "New Game",
                SaveVersion = 1,
                Created = DateTime.Now,
                LastSaved = DateTime.Now,
                PlayTimeSeconds = 0,
                PlayerStats = new PlayerStats
                {
                    Level = 1,
                    Experience = 0,
                    Health = 100,
                    Gold = 0,
                    Kills = 0
                },
                UnlockedAchievements = new System.Collections.Generic.List<string>(),
                Settings = new GameSettings
                {
                    Difficulty = "Normal",
                    Volume = 0.5f,
                    GodMode = false
                }
            };
        }

        /// <summary>
        /// Log current save stats
        /// </summary>
        private void LogStats()
        {
            if (_saveData == null) return;

            MelonLogger.Msg("=== Save Stats ===");
            MelonLogger.Msg($"Save: {_saveData.SaveName}");
            MelonLogger.Msg($"Level: {_saveData.PlayerStats.Level}");
            MelonLogger.Msg($"XP: {_saveData.PlayerStats.Experience}");
            MelonLogger.Msg($"Health: {_saveData.PlayerStats.Health}");
            MelonLogger.Msg($"Gold: {_saveData.PlayerStats.Gold}");
            MelonLogger.Msg($"Kills: {_saveData.PlayerStats.Kills}");
            MelonLogger.Msg($"Play Time: {_saveData.PlayTimeSeconds:F1}s");
            MelonLogger.Msg($"Achievements: {_saveData.UnlockedAchievements.Count}");
            MelonLogger.Msg("==================");
        }

        /// <summary>
        /// Example: Add experience to player
        /// </summary>
        public static void AddExperience(int amount)
        {
            // In a real mod, access the current instance
            // This is just example logic
        }

        /// <summary>
        /// Example: Unlock an achievement
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (_saveData != null && !_saveData.UnlockedAchievements.Contains(achievementId))
            {
                _saveData.UnlockedAchievements.Add(achievementId);
                MelonLogger.Msg($"Achievement unlocked: {achievementId}");
                SaveGame();
            }
        }
    }

    // =========================================================
    // Save Data Classes
    // =========================================================

    /// <summary>
    /// Main save data container
    /// </summary>
    public class GameSaveData
    {
        [JsonPropertyName("saveName")]
        public string SaveName { get; set; }

        [JsonPropertyName("version")]
        public int SaveVersion { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("lastSaved")]
        public DateTime LastSaved { get; set; }

        [JsonPropertyName("playTimeSeconds")]
        public double PlayTimeSeconds { get; set; }

        [JsonPropertyName("playerStats")]
        public PlayerStats PlayerStats { get; set; }

        [JsonPropertyName("achievements")]
        public System.Collections.Generic.List<string> UnlockedAchievements { get; set; }

        [JsonPropertyName("settings")]
        public GameSettings Settings { get; set; }

        /// <summary>
        /// Upgrade old save format to new format
        /// </summary>
        public void UpgradeFromOldFormat()
        {
            // Add new default fields for old saves
            if (PlayerStats == null)
            {
                PlayerStats = new PlayerStats();
            }

            if (UnlockedAchievements == null)
            {
                UnlockedAchievements = new System.Collections.Generic.List<string>();
            }

            if (Settings == null)
            {
                Settings = new GameSettings();
            }

            SaveVersion = 1;
        }
    }

    /// <summary>
    /// Player statistics
    /// </summary>
    public class PlayerStats
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("experience")]
        public int Experience { get; set; }

        [JsonPropertyName("health")]
        public float Health { get; set; }

        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [JsonPropertyName("kills")]
        public int Kills { get; set; }
    }

    /// <summary>
    /// Game settings persisted in save
    /// </summary>
    public class GameSettings
    {
        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; }

        [JsonPropertyName("volume")]
        public float Volume { get; set; }

        [JsonPropertyName("godMode")]
        public bool GodMode { get; set; }
    }
}
