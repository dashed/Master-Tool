# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [2.6.0] - 2026-02-07

### Added

- Percentage Damage Reduction: configurable slider (0–100%) that scales all incoming damage to the local player
- Keep 1 Health: prevents lethal damage by clamping HP to 3 on protected body parts, with selection between "All" and "Head And Thorax"
- Headshot Ignore: toggle to completely zero head damage
- Head Damage Percentage: separate slider (0–100%) for fine-grained head damage scaling
- Enemy Damage Multiplier: configurable multiplier (1–20x) applied to all damage dealt to non-local players
- Weight Percentage Slider: replaces binary no-weight toggle with configurable weight scale (0–100%), default 0% (weightless)
- 7 new config entries for all Phase 2 features with BepInEx validation ranges
- Mod menu controls: sliders, toggles, and dropdown for all new damage/weight features in General tab
- Status window: conditional damage reduction percentage indicator
- Unit tests for damage reduction priority chain (22 tests) and weight percentage scaling (7 tests)

## [2.5.0] - 2026-02-07

### Added

- Infinite Energy feature: keeps player energy at maximum, no need to eat. Event-guard prevents unnecessary calls when already at max
- Infinite Hydration feature: keeps player hydration at maximum, no need to drink. Same event-guard pattern as Energy
- No Fall Damage feature: sets `ActiveHealthController.FallSafeHeight` to extreme value. State-tracked cleanup restores default 1.8m when disabled without interfering with game state
- Config entries for all 3 features with customizable hotkeys (default: unbound)
- Mod menu toggles in General tab for Energy, Hydration, and Fall Damage
- Status window indicators for all 3 new features
- Unit tests for energy/hydration value-clamping logic (10 tests) and fall damage state machine (9 tests)

## [2.4.0] - 2026-02-07

### Changed

- God Mode now patches 7 damage vectors instead of 3 — inspired by [Deminvincibility](https://github.com/hazelify/Deminvincibility) mod:
  - `ActiveHealthController.Kill` — blocks instant kills (headshots, fall death, out-of-bounds)
  - `ActiveHealthController.DestroyBodyPart` — blocks limb blacking (head/chest destruction = death)
  - `ActiveHealthController.DoFracture` — blocks fracture status effects
  - `ActiveHealthController.DoBleed` — blocks bleed status effects
- Improved player identification in ActiveHealthController patches: uses Harmony `___Player` private field injection instead of `ReferenceEquals` with cached health controller
- ApplyDamage patch now zeroes damage and lets original method run (better compatibility) instead of skipping the method entirely

### Added

- Unit tests for GodMode prefix decision logic (11 tests)

## [2.3.4] - 2026-02-07

### Fixed

- ChamsIntensity config now wired to chams color brightness — multiplies faction color RGB by configurable intensity value (0.1–1.0), previously had no effect
- FovMelee config now wired to weapon FOV system — melee items (knives) are detected by type name and apply the configured melee FOV, previously FOV updates were skipped entirely for non-weapon items
- ColorQuestZone config now wired to quest zone ESP rendering — scans for TriggerWithId objects matching active quest zone conditions (PlaceBeacon, VisitPlace, LeaveItemAtLocation, LaunchFlare) and renders them with the configured zone color

### Added

- Quest zone ESP: displays zone markers for active quest objectives (placement zones, visit locations, flare zones)
- Unit tests for chams intensity color scaling (7 tests)
- Unit tests for melee FOV mapping (2 tests)
- Unit tests for quest zone ID extraction logic (10 tests)

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
