# Master Tool - SPT Mod Menu

An advanced BepInEx mod menu for **Single Player Tarkov (SPT)** featuring combat utilities, ESP overlays, movement enhancements, visual tweaks, and a full in-game GUI with seven configuration tabs.

---

## Prerequisites

| Requirement | Version |
|-------------|---------|
| SPT (Single Player Tarkov) | 4.0+ |
| BepInEx | 5.x |
| .NET Framework | 4.7.2 (bundled with SPT) |

## Installation

1. Download the latest `MasterTool.dll` from the [Releases](https://github.com/M4st3rzzz/Master-Tool/releases) page.
2. Copy `MasterTool.dll` into your SPT installation:
   ```
   SPT\BepInEx\plugins\MasterTool.dll
   ```
3. Launch SPT. The mod menu is accessible in-raid by pressing **Insert**.

## Building from Source

### 1. Clone the repository

```bash
git clone https://github.com/M4st3rzzz/Master-Tool.git
cd Master-Tool
```

### 2. Copy required game assemblies

Create a `libs/` folder in the repository root and copy the following DLLs from your SPT installation:

| DLL | Source path |
|-----|-------------|
| `BepInEx.dll` | `SPT/BepInEx/core/BepInEx.dll` |
| `0Harmony.dll` | `SPT/BepInEx/core/0Harmony.dll` |
| `Assembly-CSharp.dll` | `SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` |
| `UnityEngine.dll` | `SPT/EscapeFromTarkov_Data/Managed/UnityEngine.dll` |
| `UnityEngine.CoreModule.dll` | `SPT/EscapeFromTarkov_Data/Managed/UnityEngine.CoreModule.dll` |
| `UnityEngine.IMGUIModule.dll` | `SPT/EscapeFromTarkov_Data/Managed/UnityEngine.IMGUIModule.dll` |
| `UnityEngine.PhysicsModule.dll` | `SPT/EscapeFromTarkov_Data/Managed/UnityEngine.PhysicsModule.dll` |
| `Comfort.dll` | `SPT/EscapeFromTarkov_Data/Managed/Comfort.dll` |

### 3. Build

```bash
dotnet build
```

The compiled `MasterTool.dll` will be in `src/MasterTool/bin/Debug/net472/`.

---

## Features

### Combat

| Feature | Description |
|---------|-------------|
| **God Mode** | Immunity to all damage sources including bullets, explosions, and fall damage. |
| **Teleport Enemies** | Teleport all enemies to your position. |

### Movement

| Feature | Description |
|---------|-------------|
| **Infinite Stamina** | Keeps leg stamina, arm stamina (aiming), and oxygen at 100% at all times. |
| **No Weight Penalties** | Removes movement speed and stamina drain penalties from carrying heavy gear or loot. |
| **Speedhack** | Adjustable movement speed multiplier. |

### ESP (Extrasensory Perception)

| Feature | Description |
|---------|-------------|
| **Player ESP** | Displays all players and bots through walls with faction-based color coding (BEAR, USEC, Boss, Scav/Raider). Includes distance tracking, customizable colors via RGB sliders, adjustable update rate, and distance filter. |
| **Item ESP** | Shows loose items on the ground. Supports multi-filter search by name or ID with comma-separated lists (e.g., `LedX, GPU, Salewa`). |
| **Container ESP** | Reveals items inside containers, crates, jackets, safes, and bodies. Uses a smart caching system (10-second refresh) and squared-distance calculations for zero FPS impact. |
| **Quest ESP** | Highlights quest-related objectives and locations on the map. |
| **Chams** | Applies colored material overlays to player models for enhanced visibility through geometry. |

### Visual

| Feature | Description |
|---------|-------------|
| **Thermal Vision** | Toggles thermal imaging overlay. |
| **Night Vision** | Toggles night vision overlay. |
| **Big Head Mode** | Scales enemy head bones for easier target acquisition. |
| **Weapon FOV** | Adjusts the weapon viewmodel field of view. |

### Utility

| Feature | Description |
|---------|-------------|
| **Unlock All Doors** | Instantly unlocks every locked door and container on the map. No keys required. |
| **Performance Culling** | Disables distant meshes for a performance boost. Note: bots may appear suddenly when entering the configured range. |
| **Teleport Items** | Teleports nearby loot items to your position. |

### UI

| Feature | Description |
|---------|-------------|
| **Mod Menu** | Full in-game GUI with 7 tabs for configuring all features. |
| **Status Window** | Compact overlay showing active feature toggles and current settings. |

---

## Hotkey Reference

| Key | Action |
|-----|--------|
| `Insert` | Open / Close Mod Menu |
| `Numpad 0` | Toggle Status Window |
| `Numpad 1` | Toggle God Mode |
| `Numpad 2` | Toggle Infinite Stamina |
| `Numpad 3` | Toggle Quest ESP |
| `Numpad 4` | Toggle No Weight Penalties |
| `Numpad 5` | Toggle Player ESP |
| `Numpad 6` | Toggle Item ESP |
| `Numpad 7` | Toggle Container ESP |
| `Numpad 8` | Toggle Performance Culling |
| `Numpad 9` | Unlock All Doors |
| `K` | Toggle Chams |
| `L` | Toggle Weapon Info |

---

## Configuration

All settings are persisted to a BepInEx config file at:

```
SPT\BepInEx\config\com.master.tools.cfg
```

Settings are saved automatically when changed through the mod menu and are loaded on each game start.

---

## Project Structure

```
Master-Tool/
├── Directory.Build.props          # Shared build properties (TFM, version, metadata)
├── MasterTool.sln                 # Solution file
├── libs/                          # Game & BepInEx assemblies (not checked in)
├── src/
│   └── MasterTool/
│       ├── MasterTool.csproj      # Main plugin project
│       ├── Config/
│       │   └── PluginConfig.cs    # BepInEx configuration bindings
│       ├── Models/
│       │   ├── EspTarget.cs       # Player ESP data model
│       │   ├── ItemEspTarget.cs   # Item ESP data model
│       │   └── QuestEspTarget.cs  # Quest ESP data model
│       ├── Utils/
│       │   ├── PlayerUtils.cs     # Player helper methods
│       │   └── ReflectionUtils.cs # Reflection helper methods
│       ├── Features/
│       │   ├── GodMode/
│       │   │   └── DamagePatches.cs
│       │   ├── InfiniteStamina/
│       │   │   └── StaminaFeature.cs
│       │   ├── Performance/
│       │   │   └── CullingFeature.cs
│       │   ├── DoorUnlock/
│       │   │   └── DoorUnlockFeature.cs
│       │   ├── Speedhack/
│       │   │   └── SpeedhackFeature.cs
│       │   ├── Vision/
│       │   │   └── VisionFeature.cs
│       │   ├── BigHeadMode/
│       │   │   └── BigHeadFeature.cs
│       │   └── Teleport/
│       │       └── TeleportFeature.cs
│       ├── ESP/
│       │   ├── EspRenderer.cs     # Shared ESP drawing utilities
│       │   ├── PlayerEsp.cs       # Player & bot ESP
│       │   ├── ItemEsp.cs         # Loose item & container ESP
│       │   ├── QuestEsp.cs        # Quest objective ESP
│       │   └── ChamsManager.cs    # Chams material manager
│       └── UI/
│           ├── GuiStyles.cs       # IMGUI style definitions
│           ├── ColorPicker.cs     # RGB color picker widget
│           └── StatusWindow.cs    # Status overlay window
└── tests/
    └── MasterTool.Tests/
        └── MasterTool.Tests.csproj  # Unit tests (NUnit, net9.0)
```

---

## Development

### Building

```bash
dotnet build
```

### Running Tests

Tests cover pure logic only (models, utilities, config defaults). Game-dependent code requires Unity/EFT assemblies and cannot be unit-tested.

```bash
dotnet test
```

### Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-feature`).
3. Commit your changes.
4. Push to your fork and open a Pull Request.

---

## License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.
