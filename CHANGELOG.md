# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [2.19.0] - 2026-02-08

### Added

- 12 new `MasterTool.Core` modules completing the extraction of all mirrored test logic:
  - `MathTypes`: `Vec3` (with +, \*, magnitude, normalized), `Vec2`, `Color` structs for platform-agnostic math
  - `ChamsLogic`: intensity/opacity scaling, material state queries, loot chams logic, anti-occlusion state, cleanup tracking
  - `DamageLogic`: damage blocking decisions, damage modification, local/enemy damage computation
  - `EspLogic`: layer mask computation, player/item ESP world position calculation
  - `MovementLogic`: fly mode movement calculation, teleport ray origin calculation
  - `VisionLogic`: FOV override decisions, weapon class to FOV mapping with constants
  - `WeightLogic`: weight blocking decisions, weight percentage computation
  - `SustenanceLogic`: energy/hydration value computation
  - `FallDamageDefaults`: safe height and default height constants
  - `RebindLogic`: rebind state machine, key acceptance, frame guard, hotkey labels
  - `StringExtraction`: reflection-based string field extraction
  - Extended `HealingLogic`: effect removal decisions, body part name constants

### Changed

- 20 test files now reference `MasterTool.Core` types directly instead of mirroring logic:
  - Chams tests (5 files): use `ChamsLogic.*` methods and `Color` struct
  - Damage tests (2 files): use `DamageLogic.*` methods
  - Feature tests (7 files): use `MovementLogic`, `VisionLogic`, `WeightLogic`, `SustenanceLogic`, `FallDamageDefaults`, `HealingLogic`
  - ESP tests (2 files): use `EspLogic.*` methods and `Vec3`/`Vec2` structs
  - UI tests (2 files): use `RebindLogic.*` and `StringExtraction.*`
- Plugin source files use `using Color = UnityEngine.Color;` aliases to resolve ambiguity with Core types

## [2.18.0] - 2026-02-08

### Added

- `MasterTool.Core` shared library (`netstandard2.0`): extracted pure logic from the main plugin so tests reference real code instead of mirrored copies
  - `KeyCode` enum: platform-agnostic key code matching Unity's int values, enabling tests to use real key codes without Unity assemblies
  - `KeyBindParser`: full key bind parsing logic (aliases, modifiers, formatting) — main plugin's version is now a thin wrapper that converts between Core and Unity KeyCode types
  - `HealingLogic`: ShouldHeal, CalculateHealAmount, ShouldHealBodyPart extracted from CodModeFeature
  - `ScreenLogic`: IsOnScreen extracted from EspRenderer
  - `ChamsMode` enum: moved from Models/ to Core
  - `TabDefinitions`: tab and sub-tab name arrays shared between ModMenu and tests
  - `ConfigSections`: config section name constants shared between PluginConfig and tests
  - `ReloadDefaults`: default reload timing constants shared between ReloadSpeedFeature and tests

### Changed

- All 8 affected test files now reference `MasterTool.Core` types directly instead of mirroring logic:
  - `KeyBindParserTests`: removed ~180 lines of mirrored parser code, aliases, and KeyCode constants
  - `CodModeTests`: calls `HealingLogic` methods instead of local copies
  - `EspScreenBoundsTests`: calls `ScreenLogic.IsOnScreen` instead of local copy
  - `ChamsModeTests` / `ChamsAntiOcclusionTests`: use `Core.ChamsMode` instead of local enum
  - `SubTabTests`: references `TabDefinitions` arrays instead of local copies
  - `ConfigSectionTests`: references `ConfigSections` constants instead of local string literals
  - `ReloadSpeedTests`: references `ReloadDefaults` constants instead of local copies
- Main plugin source files delegate to Core: CodModeFeature, EspRenderer, PluginConfig.Sections, ReloadSpeedFeature, KeyBindParser (thin wrapper)

## [2.17.0] - 2026-02-08

### Added

- Text input mode for hotkey rebinding — type key names like "ctrl + b", "shift + f1", "numpad5" using the [Type] button
- KeyBindParser utility: parses user-friendly key bind strings with 80+ aliases for modifiers, F-keys, numpad, arrow keys, mouse buttons, and special keys
- Draft/pending state for hotkey changes — changes are staged (marked with \*) until explicitly saved or cancelled
- Save All / Cancel All buttons for bulk hotkey operations
- Unit tests for KeyBindParser: key name parsing, key bind combos, modifier detection, pending changes logic (67 tests)
- "Open Mod Menu" button in the F12 ConfigurationManager panel (section "00. Mod Menu") to toggle the in-game mod menu

### Changed

- Hotkey rebinding no longer saves immediately on keypress — requires explicit Save to commit
- Hotkey tab UI redesigned: each entry shows current binding, [Rebind] for keypress capture, [Type] for text input, [Clear] to unbind
- `.editorconfig`: set `csharp_space_after_cast = false` to resolve conflict between CSharpier and dotnet format

## [2.16.0] - 2026-02-08

### Added

- Sub-tabs for crowded mod menu tabs:
  - **General** tab: "Damage", "Survival", "Weapons", "Utility" sub-tabs
  - **ESP Players** tab: "ESP", "Chams", "Colors" sub-tabs
  - **Extras** tab: "Movement", "Teleport", "Fun" sub-tabs
- Unit tests for sub-tab structure: tab counts, naming, uniqueness, conciseness (32 tests)

### Changed

- Renamed Portuguese tab names to English: "Geral" → "General", "ESP Itens" → "ESP Items", "ESP Quest/Wish" → "ESP Quests", "Troll" → "Extras"
- Removed bold section headers ("--- GENERAL ---", etc.) from sub-tabbed content — sub-tabs replace the need for in-content separators
- Main tab bar rendered as two rows of toggle buttons (4 + 3) instead of single-row `Toolbar` to prevent horizontal overlap
- Replaced `BeginArea` absolute positioning with pure `GUILayout` flow — tabs, sub-tabs, and content stack automatically without hardcoded pixel offsets

### Fixed

- Main tab buttons no longer overlap each other horizontally (7 tabs too wide for single row)
- Tab rows and sub-tabs no longer overlap content — eliminated `BeginArea` with hardcoded offset that didn't account for multi-row tab bar height

## [2.15.0] - 2026-02-08

### Changed

- Reorganized BepInEx config sections from 6 crowded sections into 14 focused sections with numbered prefixes for correct F12 menu ordering:
  - "General" (20+ entries) split into "01. Damage", "02. Survival", "03. Healing", "04. Weapons", "05. Movement"
  - "FOV Settings" renamed to "06. FOV"
  - "ESP Players" split into "07. ESP Players" (text ESP only) and "08. Chams" (all chams settings unified)
  - "ESP Items" and "ESP Containers" merged into "09. ESP Items"
  - "ESP Quests" renamed to "10. ESP Quests"
  - "Visuals" renamed to "11. Visual"
  - "Performance" renamed to "12. Performance"
  - New "13. UI" section for StatusWindow and WeaponInfo toggles
  - "Hotkeys" renamed to "14. Hotkeys"
- Section names centralized in `PluginConfig.Sections` static class with `const string` fields

### Added

- Unit tests for config section organization: naming conventions, sequential numbering, alphabetical sorting, entry counts, invalid character validation, and logical grouping (28 tests)

### Note

- Users upgrading from previous versions should delete their old config file (`com.master.tools.cfg`) — section renames orphan old values and BepInEx will regenerate defaults on next launch

## [2.14.0] - 2026-02-08

### Added

- Chams Rendering Mode: three selectable modes for both player and loot chams
  - **Solid** (default) — flat color overlay on all faces (existing behavior)
  - **CullFront** — renders only back faces, creating a hollow silhouette effect
  - **Outline** — normal character/item appearance with a colored edge outline using the inverted hull technique (scaled duplicate mesh with front-face culling)
- Config entries: `Chams Render Mode`, `Loot Chams Render Mode` (enum, default Solid), `Outline Scale` (1.01–1.15, default 1.04)
- Mode cycle buttons in mod menu: Player ESP tab (after Chams toggle) and Item ESP tab (when Loot Chams enabled)
- Outline Scale slider visible only when Outline mode is selected
- Mode change detection: switching modes while chams are active triggers cleanup of the previous mode's state before applying the new mode
- Outline duplicate tracking with `_outlineDuplicates` dictionary, cleaned up on reset and periodic purge
- Guard against infinite nesting: `GetComponentsInChildren` loops skip GameObjects named `_ChamsOutline`
- Unit tests for chams mode enum values, mode cycling, cull mode mapping, outline scale clamping, mode change detection, outline duplicate tracking, type-filtered cleanup, and outline naming guard (22 tests)
- Extended anti-occlusion tests with per-mode Cull property validation and outline duplicate material state (4 tests)

## [2.13.0] - 2026-02-08

### Added

- Chams Opacity: configurable transparency slider (0.1–1.0, default 1.0 fully opaque) for both player and loot chams. Uses alpha blending (`SrcAlpha`/`OneMinusSrcAlpha` blend mode) on the chams shader material. Slider in mod menu ESP Players tab alongside existing intensity slider
- Config entry `Chams Opacity` in ESP Players section
- Unit tests for opacity clamping, alpha override, intensity+opacity combination (10 tests)

## [2.12.1] - 2026-02-08

### Fixed

- Chams no longer persist on dead bot armor after disabling the chams toggle — dead/despawned bots removed from `RegisteredPlayers` were never visited by the cleanup loop, leaving their renderers with the chams shader. Now tracks toggle state transitions and calls `ResetAllPlayerChams()` / `ResetAllLootChams()` which iterate ALL tracked renderers (by type) and restore original shaders when the respective toggle goes from enabled to disabled

### Added

- Unit tests for reset-all-by-type cleanup logic and toggle transition detection (11 tests)

## [2.12.0] - 2026-02-08

### Fixed

- COD Mode (auto-heal) no longer fails to heal when bleeding or taking continuous damage — bleed/fracture tick damage previously reset the heal timer via `NotifyDamage()` in `DamagePatches`, preventing the heal delay from ever expiring. Now uses `Player.BeingHitAction` event which only fires on direct hits (bullets/melee), not on DOT effect ticks
- COD Mode now skips destroyed (blacked) body parts instead of attempting to heal them
- COD Mode heal timer now uses `Time.unscaledDeltaTime` instead of `Time.deltaTime`, preventing speedhack from affecting the heal delay countdown

### Changed

- Removed `CodModeFeature.NotifyDamage()` call from `DamagePatches.BlockDamagePrefix_ActiveHealthController` — heal timer reset moved to `Player.BeingHitAction` event subscription in `CodModeFeature`

### Added

- Unit tests for bleed-timer interaction, destroyed body part skipping, effect removal decision logic, and unscaled time accumulation (16 tests)

## [2.11.2] - 2026-02-07

### Fixed

- Plugin no longer crashes on startup in SPT 4.0 — `ActiveHealthController.DoBleed` has multiple overloads that caused `AmbiguousMatchException` during Harmony patching. All ActiveHealthController patches now use `TryPatchAllOverloads` which iterates methods by name and patches each overload individually with per-overload error handling
- Eliminated spurious `IL Compile Error` warning for inherited base-class method overloads — `TryPatchAllOverloads` now uses `BindingFlags.DeclaredOnly` to skip inherited methods with incompatible IL. Only warns if all overloads fail to patch
- Hotkey rebinding no longer immediately captures a spurious key (e.g., F13 from peripheral software) when clicking the Rebind button — one-frame grace period using `Time.frameCount` ensures only deliberate key presses are accepted

### Added

- `UnityEngine.InputLegacyModule.dll` assembly reference required for Fly Mode input handling (`Input.GetAxis`/`Input.GetKey`)
- Unit tests for hotkey rebind frame-delay guard logic (9 tests)

## [2.11.0] - 2026-02-07

### Fixed

- FOV changer no longer resets during crouch, stand, or ADS transitions — moved FOV update from `Update()` to `LateUpdate()` for correct frame ordering and uses direct assignment instead of lerp to prevent visible snap-back
- Removed stale `_originalFov` capture that could cache incorrect base FOV values across stance changes

### Added

- "Override FOV During ADS" toggle (default OFF) — when disabled, ADS uses the game's native zoom; when enabled, custom FOV is maintained even while aiming down sights
- ADS detection via `ProceduralWeaponAnimation.IsAiming` reflection with cached PropertyInfo for performance
- Unit tests for FOV override decision logic, ADS skip behavior, and LateUpdate frame ordering (15 tests)

### Changed

- FOV update moved from `Update()` to `LateUpdate()` to run after the game's camera system applies stance and ADS FOV changes
- Removed lerp-based FOV transition — direct assignment eliminates the visible "reset then snap back" during game state changes

## [2.10.0] - 2026-02-07

### Added

- Loot Chams: colored material overlays on loose loot items for through-wall visibility, using the same shader and anti-occlusion approach as player chams. Toggle and color picker in the Item ESP tab. Uses `MeshRenderer` (items) instead of `SkinnedMeshRenderer` (players). Distance limited by Item ESP max distance setting
- Screen bounds validation for all ESP modules: filters out targets with extreme off-screen coordinates that caused text to glitch across the screen when objects were near camera frustum edges. 50px margin allows smooth edge behavior
- `IsOnScreen` helper in EspRenderer for reusable screen bounds checking
- Config entries for Loot Chams (toggle, color)
- Unit tests for screen bounds validation (19 tests) and loot chams logic (12 tests)

### Fixed

- ESP text (player, item, quest, zone) no longer glitches or appears at wrong screen positions when targets are near the edge of the camera view — all 5 projection sites (PlayerEsp, ItemEsp, QuestEsp×3) now validate screen bounds before adding targets

## [2.9.0] - 2026-02-07

### Added

- In-game hotkey rebinding: the "Configs" tab (now "Hotkeys") shows all 22 configurable hotkeys with interactive [Rebind], [Clear], and [Cancel] buttons. Click Rebind, press any key to assign it. Press Escape to cancel. Changes are saved to the BepInEx config file automatically
- Unit tests for hotkey rebind state machine and key acceptance logic (20 tests)

### Changed

- Mod menu "Configs" tab renamed to "Hotkeys" — now shows all hotkeys (previously only listed 10 as read-only text)

## [2.8.1] - 2026-02-07

### Fixed

- Player ESP text now renders above players' heads instead of at ground level — uses actual head bone position (`PlayerBones.Head.Original`) with 0.2m offset, falling back to Transform + 1.8m when bone is unavailable
- Item/Container ESP text now floats 0.5m above item positions instead of rendering at ground level
- Chams no longer disappear through multiple walls on complex maps (e.g., Interchange) — disables Unity's occlusion culling per-renderer (`forceRenderingOff = false`, `allowOcclusionWhenDynamic = false`) and sets material render queue to 4000 (overlay) to ensure rendering after all geometry

### Added

- Unit tests for ESP world position calculation logic (12 tests)
- Unit tests for chams anti-occlusion property settings (9 tests)

## [2.8.0] - 2026-02-07

### Added

- Fly Mode (Noclip): spectator-style free flight — disables CharacterController for collision-free movement through walls and terrain. Camera-direction based controls: WASD for horizontal, Space for up, LeftControl for down. Configurable speed (1–50, default 10). State-tracked: restores CharacterController on disable
- Player Self-Teleport: save/load position system with 3 actions:
  - **Save Position**: stores current player position and rotation
  - **Load Position**: teleports back to saved position
  - **Teleport to Surface**: rescue button that raycasts from 500m above to find terrain surface — fixes falling under the map from speedhack
- Config entries for Fly Mode (toggle, speed slider) and 4 hotkeys (fly toggle, save/load/surface teleport), all default unbound
- Mod menu controls: Fly Mode section in Troll tab with toggle, speed slider, and control hints. Player Teleport section with Save/Load/Surface buttons
- Status window indicators for Fly Mode and saved position state
- Unit tests for fly movement calculation (13 tests) and teleport save/load state (10 tests)

## [2.7.0] - 2026-02-07

### Added

- COD Mode (auto-heal): after not taking damage for a configurable delay (default 10s), all 7 body parts regenerate HP at a configurable rate (default 10 HP/cycle). Damage resets the heal timer. Throttled to every 60 frames for performance
- Magazine Reload Speed: adjustable load/unload times via `Singleton<BackendConfigSettingsClass>`. State-tracked: restores defaults (0.85/0.3) when disabled
- COD Mode damage notification: `DamagePatches` now calls `CodModeFeature.NotifyDamage()` when the local player takes damage, resetting the heal timer
- Config entries for COD Mode (toggle, heal rate 1–100, heal delay 0–600s, remove effects toggle) and Reload Speed (toggle, load time 0.01–2.0, unload time 0.01–2.0)
- Mod menu controls: COD Mode section (toggle, heal rate slider, heal delay slider, effects toggle) and Reload Speed section (toggle, load/unload time sliders)
- Status window indicators for COD Mode and Reload Speed
- Hotkey bindings for COD Mode and Reload Speed (default: unbound)
- Unit tests for COD Mode heal logic (19 tests) and Reload Speed state machine (9 tests)

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
