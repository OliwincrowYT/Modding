# MelonLoader Mod Project Template

## Quick Start

1. **Copy this folder** to a new location for your mod
2. **Rename** the folder and files:
   - `YourMod.csproj` → `YourModName.csproj`
   - `YourMod.cs` → `YourModName.cs`
3. **Edit** `YourMod.cs`:
   - Change the `[assembly: MelonInfo_*]` attributes
   - Update the namespace and class name
   - Add your mod logic
4. **Setup references**:
   - Create a `References` folder with:
     - `MelonLoader.dll` (from MelonLoader installation)
     - `0Harmony.dll` (from MelonLoader installation)
     - Unity assemblies (from your game folder)
   - Update `<HintPath>` in `.csproj`
5. **Build**:
   ```bash
   dotnet build
   ```
6. **Deploy**:
   - Copy the `.dll` from `bin/Debug/` to your game's `mods/` folder
7. **Test**:
   - Launch the game with MelonLoader
   - Check the log for your mod's messages

## Project Structure

```
YourMod/
├── YourMod.cs          # Main mod class
├── YourMod.csproj      # Project configuration
├── README.md           # This file
├── assets/             # Optional: textures, models, etc.
└── References/         # DLL references (create this)
    ├── MelonLoader.dll
    ├── 0Harmony.dll
    └── UnityEngine*.dll
```

## Key Concepts

### Mod Attributes
```csharp
[assembly: MelonInfo_name("Your Mod Name")]
[assembly: MelonInfo_author("Your Name")]
[assembly: MelonInfo_version("1.0.0")]
[assembly: MelonInfo_description("What your mod does")]
```

### Lifecycle Methods
- `OnApplicationStart()` - Mod loaded, game starting
- `OnUpdate()` - Called every frame
- `OnGUI()` - Draw IMGUI elements
- `OnSceneWasLoaded()` - Scene changed
- `OnApplicationQuit()` - Game closing

### References
Find Unity assemblies in your game folder:
```
GameFolder/
├── GameAssembly.dll              (IL2CPP games)
├── Managed/                      (Mono games)
│   ├── UnityEngine.dll
│   ├── UnityEngine.CoreModule.dll
│   ├── UnityEngine.IMGUIModule.dll
│   └── ...
```

## Tips

- Start simple: log messages, then add features
- Use `MelonLogger.Msg()` for debugging
- Test frequently in-game
- Read the [GUIDE.md](../../GUIDE.md) for advanced topics
- Check other example mods for patterns

## Next Steps

- Look at examples `01-HelloWorld` through `09-InterModComms`
- Read the [GUIDE.md](../../GUIDE.md) for full API reference
- Join the MelonLoader Discord for community support
