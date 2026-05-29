// ============================================================
// Example 04: IMGUI UI (Intermediate)
// ============================================================
// Demonstrates:
// - Unity IMGUI for custom HUD elements
// - Creating buttons, labels, sliders, toggles
// - Positioning and styling UI
// - Toggle visibility with keyboard shortcut
//
// IMGUI is drawn every frame in OnGUI()
// ============================================================

using System;
using UnityEngine;
using MelonLoader;

namespace IMGUIUIExample
{
    [assembly: MelonInfo_name("IMGUI UI Example")]
    [assembly: MelonInfo_author("YourName")]
    [assembly: MelonInfo_version("1.0.0")]
    [assembly: MelonInfo_description("Demonstrates IMGUI-based custom UI")]

    public class IMGUIUIExample : MelonMod
    {
        // UI State
        private bool _showUI = true;
        private Vector2 _scrollPosition;

        // Demo values
        private float _sliderValue = 50f;
        private bool _toggleValue = true;
        private string _textFieldValue = "Hello IMGUI!";

        // Window position (for movable windows)
        private Rect _windowRect = new Rect(10, 10, 300, 400);
        private bool _isDragging = false;
        private Vector2 _dragOffset;

        override public void OnGUI()
        {
            // Toggle visibility with F7
            if (Input.GetKeyDown(KeyCode.F7))
            {
                _showUI = !_showUI;
            }

            if (!_showUI) return;

            // Option 1: Fixed position UI
            DrawFixedUI();

            // Option 2: Movable window UI
            _windowRect = GUILayout.Window(
                12345, // Unique window ID
                _windowRect,
                DrawWindowContents,
                "Config Window");
        }

        /// <summary>
        /// Draw UI at fixed screen positions
        /// </summary>
        private void DrawFixedUI()
        {
            // Top-left stats display
            GUILayout.BeginArea(new Rect(10, 10, 250, 150));

            GUILayout.BeginVertical("box");

            GUILayout.Label("=== Mod Stats ===", GUILayout.ExpandWidth(true));
            GUILayout.Label($"FPS: {1f / Time.deltaTime:F1}");
            GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}");
            GUILayout.Label($"Time: {Time.time:F1}s");

            GUILayout.Space(10);

            // Slider
            _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 0f, 100f);
            GUILayout.Label($"Slider: {_sliderValue:F1}");

            // Toggle
            _toggleValue = GUILayout.Toggle(_toggleValue, "Toggle Option");

            // Text field
            _textFieldValue = GUILayout.TextField(_textFieldValue);

            // Buttons
            if (GUILayout.Button("Click Me!"))
            {
                MelonLogger.Msg($"Button clicked! Field: {_textFieldValue}");
            }

            if (GUILayout.Button("Reset Values"))
            {
                _sliderValue = 50f;
                _toggleValue = true;
                _textFieldValue = "Reset!";
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Draw contents of a movable window
        /// </summary>
        private void DrawWindowContents(int windowID)
        {
            GUILayout.BeginVertical();

            // Scrollable area
            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition,
                GUILayout.MaxHeight(300));

            GUILayout.Label("Window Content", EditorStyles.boldLabel);
            GUILayout.Label("This window can be dragged by its title bar.");

            GUILayout.Space(10);

            // Example controls
            _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 0f, 100f);
            GUILayout.Label($"Value: {_sliderValue:F1}");

            _toggleValue = GUILayout.Toggle(_toggleValue, "Option Toggle");

            _textFieldValue = GUILayout.TextField(_textFieldValue);

            GUILayout.Space(10);

            // Color picker
            Color pickedColor = GUILayout.ColorPicker(Color.white);
            GUILayout.Label($"Color: {pickedColor}");

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            // Close button
            if (GUILayout.Button("Close Window"))
            {
                _showUI = false;
            }

            // Allow dragging
            GUI.DragWindow();
        }

        /// <summary>
        /// Example: Draw a custom styled label
        /// </summary>
        private void DrawStyledLabel(string text, float x, float y, float width, float height)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.yellow;

            GUI.Label(
                new Rect(x, y, width, height),
                text,
                style);
        }

        override public void OnApplicationQuit()
        {
            MelonLogger.Msg("IMGUI UI mod unloaded");
        }
    }
}
