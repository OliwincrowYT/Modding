---
name: bepInEx-modding-reference
description: Complete BepInEx modding reference covering beginner to expert topics
metadata:
  type: project
  created: 2026-05-29
---

Created a comprehensive BepInEx modding reference in this folder. Structure:

**CLAUDE.md** — Master reference document with 14 sections:
1. Getting Started (installation, first plugin, project setup)
2. C# & Unity Fundamentals (events, delegates, coroutines, lifecycle)
3. Plugin Anatomy (attributes, lifecycle, static constructors)
4. Harmony Patching (prefix, postfix, special params, applying/unpatching)
5. Game APIs & Patterns (singletons, input, scenes)
6. Configuration (all config entry types, sections, change events)
7. UI Development (IMGUI, EGUI, Canvas-based Unity UI)
8. Asset & Content Mods (textures, models, AssetBundles, audio)
9. Advanced Harmony (transpilers, IL opcodes, finalizers, priorities)
10. Performance & Debugging (logging, profiling, common pitfalls)
11. Multi-Mod Compatibility (patch order, interop, conflict resolution)
12. Distribution (building, packaging, Thunderstore, CI/CD)
13. Expert Techniques (IL injection, dynamic methods, P/Invoke, memory reading, runtime assembly gen)
14. Cheat Sheet (templates, one-liners, debugging checklist)

**Numbered example folders** with runnable code:
- `01-HelloWorld/` — First plugin + .csproj template
- `02-ConfigExample/` — All config entry types with change events
- `03-HarmonyBasics/` — Prefix, postfix, __state, delegate patching
- `04-AccessTools/` — Type/method/field lookup, delegate creation
- `05-UILogin/` — IMGUI mod menu with tabs, sliders, buttons
- `06-Transpiler/` — IL modification: replace constants, inject code, remove instructions, pattern matching, branch neutralization
- `07-PluginInterop/` — Dependencies, public APIs, events, conflict resolution, priorities
- `08-AdvancedPatterns/` — Finalizers, dynamic methods, coroutines, scene hooks, resource intercept, P/Invoke, reversible patches, field watchers, assembly patchers, object pooling

**Why:** User requested a complete beginner-to-expert BepInEx modding reference.
**How to apply:** Use CLAUDE.md as the master reference. Numbered examples are progressive — follow them in order to learn. Each example is self-contained and can be compiled independently.
