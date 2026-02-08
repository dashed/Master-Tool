# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [2.3.3] - 2026-02-07

### Fixed

- GameState.MainCamera now always re-fetches on refresh cycle — previously only fetched when null, causing stale camera references after spectator mode, death screen, or scene transitions

### Added

- Unit tests for camera refresh logic (7 tests)

## [2.3.2] - 2026-02-07

### Fixed

- ChamsManager no longer leaks shader references for despawned players — periodic cleanup every 30s purges dictionary entries for destroyed Unity Renderer objects

### Added

- Unit tests for chams dictionary cleanup logic (8 tests)

## [2.3.1] - 2026-02-07

### Fixed

- Silent Harmony patch failures now logged to BepInEx console — GodMode and NoWeight patch errors are always reported, per-frame feature errors (Stamina, Culling, QuestESP, GameState, StatusWindow) log once to avoid spam

## [2.3.0] - 2026-02-07

### Added

- No Weight feature: Harmony patch on `InventoryEquipment.smethod_1` sets equipment weight to zero when enabled, eliminating overweight penalties
- Unit tests for NoWeight prefix decision logic (4 tests)

### Fixed

- NoWeight toggle (config, hotkey, menu, status HUD) was previously non-functional — now wired to actual Harmony patch

## [2.2.2] - 2026-02-07

### Fixed

- Big head mode no longer forces head scale to 1x on all players when toggled OFF (previously overrode any non-standard bone scaling every frame — same root cause as NV and culling bugs)

### Added

- Unit tests for big head state machine logic (11 tests)

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
