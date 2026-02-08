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

1. Download the latest `MasterTool.dll` from the [Releases](https://github.com/dashed/Master-Tool/releases) page.
2. Copy `MasterTool.dll` into your SPT installation:
   ```
   SPT\BepInEx\plugins\MasterTool.dll
   ```
3. Launch SPT. The mod menu is accessible in-raid by pressing **Insert**.

## Building from Source

### 1. Clone the repository

```bash
git clone https://github.com/dashed/Master-Tool.git
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
| `UnityEngine.TextRenderingModule.dll` | `SPT/EscapeFromTarkov_Data/Managed/UnityEngine.TextRenderingModule.dll` |
| `UnityEngine.InputLegacyModule.dll` | `SPT/EscapeFromTarkov_Data/Managed/UnityEngine.InputLegacyModule.dll` |
| `Comfort.dll` | `SPT/EscapeFromTarkov_Data/Managed/Comfort.dll` |

### 3. Build

```bash
dotnet build
```

The compiled `MasterTool.dll` will be in `build/`.

---

## Features

### Combat

| Feature | Description |
|---------|-------------|
| **God Mode** | Comprehensive immunity: blocks damage, instant kills, limb destruction, fractures, and bleeds across 7 Harmony patches on both Player and ActiveHealthController. |
| **Damage Reduction** | Configurable percentage slider (0–100%) that scales all incoming damage to the local player. |
| **Keep 1 Health** | Prevents lethal damage by clamping HP to 3 on protected body parts. Selectable protection: "All" body parts or "Head And Thorax" only. |
| **Headshot Protection** | Toggle to completely ignore head damage, plus a separate head damage percentage slider (0–100%) for fine-grained control. |
| **Enemy Damage Multiplier** | Configurable multiplier (1–20x) applied to all damage dealt to non-local players (bots/AI). |
| **COD Mode** | Auto-heal: after not taking damage for a configurable delay (default 10s), all body parts regenerate HP at a configurable rate. Damage resets the timer. |
| **Teleport Enemies** | Teleport all enemies to 3 meters in front of you. |

### Movement

| Feature | Description |
|---------|-------------|
| **Infinite Stamina** | Keeps leg stamina, arm stamina (aiming), and oxygen at 100% at all times. |
| **Infinite Energy** | Keeps energy at maximum — no need to eat. Only calls `ChangeEnergy` when below max for zero unnecessary overhead. |
| **Infinite Hydration** | Keeps hydration at maximum — no need to drink. Same efficiency guard as Energy. |
| **No Weight Penalties** | Configurable weight scale (0–100%) — set to 0% for weightless, or fine-tune to reduce weight penalties without removing them entirely. |
| **No Fall Damage** | Eliminates fall damage by setting safe fall height to an extreme value. State-tracked: only resets when the mod forced the change, never interferes with game defaults. |
| **Fly Mode (Noclip)** | Spectator-style free flight through walls and terrain. Disables CharacterController for collision-free movement. WASD + Space (up) + Ctrl (down), configurable speed. |
| **Speedhack** | Adjustable movement speed multiplier. |
| **Reload Speed** | Adjustable magazine load/unload times. Lower values = faster reloads. State-tracked: restores game defaults when disabled. |

### ESP & Chams

**ESP** (Extrasensory Perception) draws on-screen text labels showing positions, distances, and details for players, items, and quest objectives. **Chams** (short for "chameleon") are colored shader overlays applied directly to 3D models, making them visible through walls and geometry by replacing the normal material with a flat-colored always-visible shader (depth testing disabled).

| Feature | Description |
|---------|-------------|
| **Player ESP** | Displays all players and bots with faction-based color coding (BEAR, USEC, Boss, Scav/Raider). Includes distance tracking, customizable colors via RGB sliders, adjustable update rate, distance filter, and optional **line-of-sight mode** that only shows players you can directly see (no wall visibility). |
| **Item ESP** | Shows loose items on the ground. Supports multi-filter search by name or ID with comma-separated lists (e.g., `LedX, GPU, Salewa`). |
| **Container ESP** | Reveals items inside containers, crates, jackets, safes, and bodies. Uses a smart caching system (10-second refresh) and squared-distance calculations for zero FPS impact. |
| **Quest ESP** | Highlights quest-related items in the world and quest zone markers (placement zones, visit locations, flare zones) with configurable colors for items and zones. |
| **Player Chams** | Applies colored material overlays to player models for enhanced visibility through geometry. Three rendering modes: **Solid** (flat color, all faces), **CullFront** (hollow silhouette, back faces only), and **Outline** (normal model + colored edge via inverted hull). Configurable color intensity (10%–100%), opacity (10%–100%), and outline thickness. Anti-occlusion: forces renderer visibility through multiple walls. |
| **Loot Chams** | Applies colored material overlays to loose loot items for through-wall visibility. Same three rendering modes as player chams. Configurable color via the Item ESP tab. Shares intensity and opacity settings with player chams. Distance limited by Item ESP max distance. |

### Visual

| Feature | Description |
|---------|-------------|
| **Thermal Vision** | Toggles thermal imaging overlay. Does not interfere with vanilla thermal goggles when disabled. |
| **Night Vision** | Toggles night vision overlay. Does not interfere with vanilla NVGs when disabled. |
| **Big Head Mode** | Scales enemy head bones for easier target acquisition. Only resets heads the mod scaled. |
| **Weapon FOV** | Per-weapon-category FOV adjustment (pistol, SMG, assault rifle, shotgun, sniper, machinegun, melee). Applied in LateUpdate for flicker-free operation during stance changes and ADS. Configurable "Override FOV During ADS" toggle (default OFF) lets ADS use native game zoom. |

### Utility

| Feature | Description |
|---------|-------------|
| **Unlock All Doors** | Instantly unlocks every locked door on the map. No keys required. |
| **Performance Culling** | Deactivates distant bots for a performance boost. Only re-enables bots the mod deactivated — does not interfere with the game's native bot sleep system. |
| **Teleport Items** | Teleports all loose loot matching the item ESP filter to your position. If no filter is set, all loose loot is teleported. |
| **Player Teleport** | Save/load position system: save your current spot, teleport back anytime. Includes a **Teleport to Surface** rescue button that finds the terrain above you — fixes falling under the map. |

### UI

| Feature | Description |
|---------|-------------|
| **Mod Menu** | Full in-game GUI with 7 tabs for configuring all features. |
| **Hotkey Rebinding** | In-game hotkey configuration — click [Rebind], press any key, done. All 22 hotkeys are customizable from the Hotkeys tab. Press Escape to cancel, [Clear] to unbind. Changes persist to the BepInEx config file. |
| **Status Window** | Compact overlay showing active feature toggles and current settings. |

---

## Hotkey Reference

All hotkeys below are defaults and can be rebound in-game from the **Hotkeys** tab in the mod menu.

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
| *(unbound)* | Toggle Infinite Energy |
| *(unbound)* | Toggle Infinite Hydration |
| *(unbound)* | Toggle No Fall Damage |

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
├── .editorconfig                  # C# coding conventions
├── .gitignore
├── Directory.Build.props          # Shared build properties (TFM, version, metadata)
├── Makefile                       # Build, test, lint, and format targets
├── MasterTool.sln                 # Solution file
├── libs/                          # Game & BepInEx assemblies (not checked in)
├── src/
│   └── MasterTool/
│       ├── MasterTool.csproj      # Main plugin project
│       ├── Plugin/
│       │   ├── MasterToolPlugin.cs # BepInEx entry point and orchestrator
│       │   └── GameState.cs       # Cached game references with periodic refresh
│       ├── Config/
│       │   └── PluginConfig.cs    # BepInEx configuration bindings
│       ├── Models/
│       │   ├── ChamsMode.cs       # Chams rendering mode enum
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
│       │   ├── Sustenance/
│       │   │   ├── EnergyFeature.cs     # Infinite energy
│       │   │   └── HydrationFeature.cs  # Infinite hydration
│       │   ├── FallDamage/
│       │   │   └── FallDamageFeature.cs # No fall damage (state-tracked)
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
│       │   ├── NoWeight/
│       │   │   └── NoWeightFeature.cs
│       │   └── Teleport/
│       │       └── TeleportFeature.cs
│       ├── ESP/
│       │   ├── EspRenderer.cs     # Shared ESP drawing utilities
│       │   ├── PlayerEsp.cs       # Player & bot ESP
│       │   ├── ItemEsp.cs         # Loose item & container ESP
│       │   ├── QuestEsp.cs        # Quest objective ESP
│       │   └── ChamsManager.cs    # Chams material manager
│       └── UI/
│           ├── ModMenu.cs         # Main mod menu window (7 tabs)
│           ├── GuiStyles.cs       # IMGUI style definitions
│           ├── ColorPicker.cs     # RGB color picker widget
│           └── StatusWindow.cs    # Status overlay window
└── tests/
    └── MasterTool.Tests/
        ├── MasterTool.Tests.csproj  # Unit tests (NUnit, net9.0)
        └── Tests/
            ├── Models/
            │   └── EspTargetTests.cs
            ├── Utils/
            │   ├── FovMappingTests.cs
            │   ├── PlayerTagTests.cs
            │   └── ReflectionUtilsTests.cs
            ├── Features/
            │   ├── BigHeadStateTests.cs
            │   ├── CodModeTests.cs
            │   ├── CullingStateTests.cs
            │   ├── DamageReductionTests.cs
            │   ├── EnergyHydrationTests.cs
            │   ├── FallDamageStateTests.cs
            │   ├── FlyModeTests.cs
            │   ├── FovOverrideTests.cs
            │   ├── GodModePrefixTests.cs
            │   ├── NoWeightPrefixTests.cs
            │   ├── PlayerTeleportTests.cs
            │   ├── ReloadSpeedTests.cs
            │   ├── VisionStateTests.cs
            │   └── WeightPercentageTests.cs
            ├── ESP/
            │   ├── ChamsAntiOcclusionTests.cs
            │   ├── ChamsCleanupTests.cs
            │   ├── ChamsModeTests.cs
            │   ├── ChamsIntensityTests.cs
            │   ├── EspPositionTests.cs
            │   ├── EspScreenBoundsTests.cs
            │   ├── LineOfSightTests.cs
            │   ├── LootChamsTests.cs
            │   └── QuestZoneExtractionTests.cs
            ├── UI/
            │   └── HotkeyRebindTests.cs
            └── GameStateRefreshTests.cs
```

---

## Development

A `Makefile` is provided for common tasks. Run `make help` to see all targets:

```bash
make test           # Run unit tests
make format         # Auto-format code with CSharpier
make format-check   # Check formatting (CI-safe, no changes)
make lint           # Check code style against .editorconfig
make lint-fix       # Auto-fix code style issues
make build          # Build plugin DLL (requires libs/)
make clean          # Remove build artifacts
make all            # format-check + lint + test
make ci             # Full CI pipeline
```

### Building

```bash
make build    # or: dotnet build
```

### Running Tests

367 tests cover pure logic: models, utilities, feature state machines, ESP extraction, and config defaults. Game-dependent code requires Unity/EFT assemblies and cannot be unit-tested — tests duplicate the pure logic with fake types.

```bash
make test     # or: dotnet test
```

### Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-feature`).
3. Commit your changes.
4. Push to your fork and open a Pull Request.

---

## License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.
