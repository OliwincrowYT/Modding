# MelonLoader Example Mods

Complete collection of example mods demonstrating MelonLoader features from beginner to expert level.

## Examples Overview

| # | Example | Level | Key Concepts |
|---|---------|-------|-------------|
| 01 | [HelloWorld](01-HelloWorld/) | Beginner | Mod attributes, lifecycle methods |
| 02 | [ConfigMod](02-ConfigMod/) | Beginner | MelonPreferences, persistent settings |
| 03 | [HarmonyPatching](03-HarmonyPatching/) | Intermediate | Prefix/postfix patches, AccessTools |
| 04 | [IMGUI-UI](04-IMGUI-UI/) | Intermediate | Custom HUD, buttons, sliders, windows |
| 05 | [SceneHooks](05-SceneHooks/) | Intermediate | Scene lifecycle, object finding |
| 06 | [Transpiler](06-Transpiler/) | Advanced | IL opcode manipulation, bytecode editing |
| 07 | [SaveSystem](07-SaveSystem/) | Advanced | JSON persistence, save/load, versioning |
| 08 | [AssetBundles](08-AssetBundles/) | Advanced | Loading custom assets, prefabs, textures |
| 09 | [InterModComms](09-InterModComms/) | Expert | Mod detection, reflection, API exposure |
| 10 | [ProjectTemplate](10-ProjectTemplate/) | All | Starter project, build setup |

## How to Use

1. **Read the [GUIDE.md](../GUIDE.md)** for comprehensive documentation
2. **Pick an example** matching your skill level
3. **Study the code** - each file is heavily commented
4. **Build and test** in a MelonLoader-enabled game
5. **Use [ProjectTemplate](10-ProjectTemplate/)** to start your own mod

## Building

Each example needs:
1. **.NET SDK** installed
2. **References** to MelonLoader and Unity assemblies
3. **Build command**:
   ```bash
   dotnet build
   ```

## Key Files

- [GUIDE.md](../GUIDE.md) - Complete modding guide
- [ProjectTemplate](10-ProjectTemplate/) - Starter project
- Each example folder - Self-contained, documented code

## Learning Path

```
Beginner → Intermediate → Advanced → Expert
    ↓            ↓            ↓          ↓
 01, 02       03, 04, 05    06, 07, 08    09
    ↓
 10 (use anytime)
```

## Notes

- Examples reference game classes (`Player`, `Enemy`, etc.) - replace with your game's actual classes
- Keyboard shortcuts (F5-F12, Home) are for demonstration - customize as needed
- All examples follow MelonLoader best practices
- Code is designed to be copied and adapted

## Community

- [MelonLoader GitHub](https://github.com/LachesisUS/MelonLoader)
- [MelonLoader Discord](https://discord.gg/melonloader)
- [Documentation](https://docs.melonmod.com/)
