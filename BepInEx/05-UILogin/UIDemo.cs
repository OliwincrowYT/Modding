// ═══════════════════════════════════════════════════════════
// INTERMEDIATE: In-Game UI with Unity IMGUI
// Quick & dirty UI — perfect for debug panels & mod menus
// ═══════════════════════════════════════════════════════════

using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

[BepInPlugin("com.example.uidemo", "UI Demo", "1.0.0")]
public class UIDemoPlugin : BaseUnityPlugin
{
    // ── Config ──────────────────────────────────────────────
    private ConfigEntry<bool> _showMenu;
    private ConfigEntry<KeyCode> _toggleKey;

    // ── UI State ────────────────────────────────────────────
    private Vector2 _scrollPos;
    private float _sliderValue = 0.5f;
    private int _selectedTab = 0;
    private string _inputText = "";

    // ── Lifecycle ───────────────────────────────────────────

    private void Awake()
    {
        _showMenu = Config.Bind("UI", "Show Menu", true, "Start with menu visible");
        _toggleKey = Config.Bind("UI", "Toggle Key", KeyCode.F5, "Key to toggle menu");
    }

    private void Update()
    {
        // Toggle visibility
        if (Input.GetKeyDown(_toggleKey.Value))
        {
            _showMenu.Value = !_showMenu.Value;
        }
    }

    // ── OnGUI ───────────────────────────────────────────────
    // Called every frame by Unity for IMGUI rendering.
    // Only runs when the plugin is active.

    private void OnGUI()
    {
        if (!_showMenu.Value) return;

        // Position & size
        var width = 350f;
        var height = 400f;
        var x = Screen.width / 2f - width / 2f;
        var y = Screen.height / 2f - height / 2f;

        // Draggable window
        GUI.Window(0, new Rect(x, y, width, height), DrawWindowContent, "Mod Menu");
    }

    private void DrawWindowContent(int windowId)
    {
        GUILayout.BeginVertical();

        // ── Tab Selector ────────────────────────────────────
        var tabNames = new[] { "General", "Combat", "Debug" };
        var newTab = GUILayout.Toolbar(_selectedTab, tabNames);
        if (newTab != _selectedTab) _selectedTab = newTab;

        // ── Scrollable Content ──────────────────────────────
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(250f));

        switch (_selectedTab)
        {
            case 0: DrawGeneralTab(); break;
            case 1: DrawCombatTab(); break;
            case 2: DrawDebugTab(); break;
        }

        GUILayout.EndScrollView();

        // ── Close Button ────────────────────────────────────
        if (GUILayout.Button("Close"))
        {
            _showMenu.Value = false;
        }

        GUILayout.EndVertical();

        // Allow the window to be dragged
        GUI.DragWindow();
    }

    private void DrawGeneralTab()
    {
        GUILayout.Label("General Settings", GUILayout.Height(20));
        GUILayout.Space(5);

        _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 0f, 1f);
        GUILayout.Label($"Slider: {_sliderValue:F2}");

        _inputText = GUILayout.TextField(_inputText);

        if (GUILayout.Button("Submit Text"))
        {
            Debug.Log($"You typed: {_inputText}");
        }

        GUILayout.Space(10);
        GUILayout.Label($"Resolution: {Screen.width}x{Screen.height}");
        GUILayout.Label($"FPS: {1f / Time.deltaTime:F0}");
    }

    private void DrawCombatTab()
    {
        GUILayout.Label("Combat Settings", GUILayout.Height(20));
        GUILayout.Space(5);

        if (GUILayout.Button("God Mode Toggle"))
        {
            Debug.Log("God Mode toggled!");
        }

        if (GUILayout.Button("Unlock All"))
        {
            Debug.Log("Everything unlocked!");
        }

        GUILayout.Space(10);
        GUILayout.Label("Damage: ∞");
        GUILayout.Label("Speed: ∞");
    }

    private void DrawDebugTab()
    {
        GUILayout.Label("Debug Info", GUILayout.Height(20));
        GUILayout.Space(5);

        GUILayout.Label($"Time Scale: {Time.timeScale:F3}");
        GUILayout.Label($"Frame: {Time.frameCount}");
        GUILayout.Label($"Active Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        GUILayout.Label($"Memory: {System.GC.GetTotalMemory(false) / 1024f / 1024f:F1} MB");

        GUILayout.Space(10);

        if (GUILayout.Button("Force GC"))
        {
            System.GC.Collect();
        }

        if (GUILayout.Button("Dump Active Objects"))
        {
            var count = Object.FindObjectsOfType<GameObject>().Length;
            Debug.Log($"Active GameObjects: {count}");
        }
    }
}
