# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [2.2.1] - 2026-02-07

### Fixed

- Performance culling no longer force-enables all bots when toggled OFF (previously overrode game-side deactivations for dead/despawned bots every frame — same root cause as the v2.2.0 night vision bug)

### Added

- Unit tests for culling state machine logic (12 tests)

## [2.2.0] - 2026-02-07

### Added

- Line-of-sight mode for Player ESP: optional toggle that only shows ESP labels for players with direct line of sight (uses `Physics.Linecast` against terrain/wall geometry layers)
- `EspLineOfSightOnly` config entry in "ESP Players" section (defaults to off)
- "Line of Sight Only" toggle in mod menu ESP Players tab
- Unit tests for LOS layer mask computation logic
- Unit tests for vision state machine logic (11 tests)

### Fixed

- Night vision and thermal vision toggles no longer override vanilla NVG/thermal state when mod toggles are OFF (previously force-disabled vanilla NVGs every frame)
- Night vision update no longer gated behind weapon check — NVGs now work correctly regardless of equipped item
- Big head mode no longer gated behind weapon check — works with meds, keys, and all items

## [2.1.2] - 2026-02-07

### Added

- Modular project structure with 25 source files replacing monolithic plugin
- .NET solution with separate plugin and test projects
- 45 unit tests (NUnit) for models, utils, and feature logic
- Makefile with build, test, format, and lint targets
- CSharpier code formatting integration
- .editorconfig with C# coding conventions
- CLAUDE.md project instructions
- Comprehensive README with features, hotkeys, and build instructions
- UnityEngine.TextRenderingModule assembly reference

### Changed

- Reorganized 1,343-line monolithic plugin into modular architecture:
  - `Plugin/` — entry point and game state manager
  - `Config/` — centralized BepInEx configuration bindings
  - `Models/` — ESP data models
  - `Utils/` — player and reflection helpers
  - `Features/` — GodMode, Stamina, Speedhack, Vision, BigHead, DoorUnlock, Culling, Teleport
  - `ESP/` — PlayerEsp, ItemEsp, QuestEsp, ChamsManager, EspRenderer
  - `UI/` — ModMenu (7 tabs), StatusWindow, GuiStyles, ColorPicker
- Build output redirected to repo-root `build/` directory

### Fixed

- Missing UnityEngine.TextRenderingModule reference (FontStyle/TextAnchor types)
- Unused exception variable compiler warning (CS0168)

## [2.1.1] - 2025

- Added chams (colored material overlays on player models)
- Added additional features (teleport, big head mode, door unlock, performance culling)
- Various bug fixes and feature tweaks

## [2.0.0] - 2025

- Initial public release as monolithic BepInEx plugin
- God Mode, Infinite Stamina, Speedhack
- Player ESP with faction-based color coding
- Item and Container ESP with filtering
- Quest ESP
- Thermal/Night Vision toggles
- Weapon FOV adjustment
- In-game mod menu with IMGUI
- Status window overlay
- Full hotkey support (Numpad + Insert)
