# Feature Bug Audit — NV-Pattern Analysis

**Date:** 2026-02-07
**Scope:** Systematic audit of all feature modules for bugs similar to the night vision goggles bug (v2.1.2 → v2.2.0)
**Methodology:** Code review of every feature file against 5 bug patterns derived from the NV root cause

## Background

In v2.2.0 we fixed a critical bug in `VisionFeature` where:

1. Night/thermal vision state was **force-set every frame** regardless of the mod toggle state — overriding vanilla NVGs
2. There was **no state tracking** to distinguish mod-forced state from vanilla state
3. NV and BigHead were **gated behind a weapon check** and didn't work without a weapon equipped

This audit examines every feature module for the same three patterns, plus two additional patterns:

4. **Missing cleanup** — feature doesn't restore game state when disabled
5. **Frame-accumulating effects** — state drifts over time

---

## Summary of Findings

| # | Bug | Severity | File | Pattern |
|---|-----|----------|------|---------|
| 1 | CullingFeature forces all bots active when OFF | **HIGH** | `CullingFeature.cs:33-41` | Unconditional state override |
| 2 | BigHeadFeature forces head scale to 1x when OFF | **MEDIUM** | `BigHeadFeature.cs:32-35` | Unconditional state override |
| 3 | NoWeight toggle has no implementation | **MEDIUM** | Multiple files | Missing feature |
| 4 | ChamsManager leaks shader references | **LOW** | `ChamsManager.cs:73` | Resource leak |
| 5 | GameState.MainCamera never refreshes if changed | **LOW** | `GameState.cs:42-43` | Stale cache |
| 6 | ChamsIntensity config unused | **LOW** | `PluginConfig.cs:52` | Dead config |
| 7 | FovMelee config unused | **LOW** | `PluginConfig.cs:89` | Dead config |
| 8 | ColorQuestZone config unused | **LOW** | `PluginConfig.cs:79` | Dead config |
| 9 | GodMode patches fail silently | **LOW** | `DamagePatches.cs:36` | Silent failure |

---

## Detailed Analysis

### BUG-1: CullingFeature forces all bots active when disabled [HIGH]

**File:** `src/MasterTool/Features/Performance/CullingFeature.cs` lines 33–41
**Pattern:** Same as NV bug — unconditional state modification when toggle is OFF

#### Current Code

```csharp
bool shouldRender = true;
if (PluginConfig.PerformanceMode.Value)
{
    float dist = Vector3.Distance(localPlayer.Transform.position, p.Transform.position);
    shouldRender = dist <= PluginConfig.BotRenderDistance.Value;
}

if (botObj.activeSelf != shouldRender)
    botObj.SetActive(shouldRender);
```

#### Problem

When `PerformanceMode` is OFF, `shouldRender` defaults to `true`, so `SetActive(true)` is called for **every bot every frame**. This overrides any game-side deactivations (dead bot cleanup, LOD systems, scripted events, zone transitions).

Note: `PerformanceMode` defaults to `true` in config (line 239 of PluginConfig.cs), so this bug manifests when a user explicitly disables culling.

The code runs unconditionally from `MasterToolPlugin.Update()` (line 83):
```csharp
CullingFeature.Apply(gameWorld, localPlayer);  // Always called, not gated by toggle
```

#### Why This Is the NV Bug Pattern

- **NV bug:** Force-disabled NV every frame when toggle OFF → broke vanilla NVGs
- **Culling bug:** Force-enables all bots every frame when toggle OFF → overrides game deactivations

Both: no tracking of what the mod changed, so cleanup resets **all** state, not just mod-modified state.

#### Proposed Fix

Track which bots the mod deactivated. Only re-enable those on toggle-off:

```csharp
private static readonly HashSet<int> _modDeactivatedBots = new HashSet<int>();

public static void Apply(GameWorld gameWorld, Player localPlayer)
{
    if (gameWorld == null || localPlayer == null) return;
    try
    {
        var players = gameWorld.RegisteredPlayers;
        foreach (var p in players)
        {
            if (p == null || p.IsYourPlayer) continue;
            GameObject botObj = (p as Component)?.gameObject;
            if (botObj == null) continue;
            int id = p.GetHashCode();

            if (PluginConfig.PerformanceMode.Value)
            {
                float dist = Vector3.Distance(localPlayer.Transform.position, p.Transform.position);
                bool shouldRender = dist <= PluginConfig.BotRenderDistance.Value;

                if (!shouldRender && botObj.activeSelf)
                {
                    botObj.SetActive(false);
                    _modDeactivatedBots.Add(id);
                }
                else if (shouldRender && !botObj.activeSelf && _modDeactivatedBots.Contains(id))
                {
                    botObj.SetActive(true);
                    _modDeactivatedBots.Remove(id);
                }
            }
            else if (_modDeactivatedBots.Contains(id))
            {
                botObj.SetActive(true);
                _modDeactivatedBots.Remove(id);
            }
        }
    }
    catch { }
}
```

---

### BUG-2: BigHeadFeature forces head scale to Vector3.one when disabled [MEDIUM]

**File:** `src/MasterTool/Features/BigHeadMode/BigHeadFeature.cs` lines 27–35
**Pattern:** Same as NV bug — unconditional state modification when toggle is OFF

#### Current Code

```csharp
if (PluginConfig.BigHeadModeEnabled.Value && player.HealthController.IsAlive)
{
    float size = PluginConfig.HeadSizeMultiplier.Value;
    head.localScale = new Vector3(size, size, size);
}
else
{
    head.localScale = Vector3.one;
}
```

#### Problem

When `BigHeadModeEnabled` is OFF, the `else` branch sets `head.localScale = Vector3.one` for **every non-local player every frame**. This overrides any non-standard head scaling from other mods, gear effects, or animations.

#### Why This Is the NV Bug Pattern

- **NV bug:** Force-set `thermal.On = false` when mod toggle OFF → broke vanilla thermals
- **BigHead bug:** Force-set `head.localScale = Vector3.one` when mod toggle OFF → overrides any non-default head scale

#### Severity Assessment

Lower severity than the NV bug because EFT player head bones are normally always at scale 1,1,1. However, other SPT mods or future game updates could set non-standard scales, and this would silently override them.

#### Proposed Fix

Track which players the mod scaled, only reset those:

```csharp
private static readonly HashSet<int> _modScaledPlayers = new HashSet<int>();

public static void Apply(GameWorld gameWorld)
{
    if (gameWorld == null) return;
    foreach (var player in gameWorld.RegisteredPlayers)
    {
        if (player == null || player.IsYourPlayer) continue;
        var head = player.PlayerBones.Head.Original;
        if (head == null) continue;
        int id = player.GetHashCode();

        if (PluginConfig.BigHeadModeEnabled.Value && player.HealthController.IsAlive)
        {
            float size = PluginConfig.HeadSizeMultiplier.Value;
            head.localScale = new Vector3(size, size, size);
            _modScaledPlayers.Add(id);
        }
        else if (_modScaledPlayers.Contains(id))
        {
            head.localScale = Vector3.one;
            _modScaledPlayers.Remove(id);
        }
    }
}
```

---

### BUG-3: NoWeight toggle has no implementation [MEDIUM]

**Files:**
- `PluginConfig.cs:22` — `ConfigEntry<bool> NoWeightEnabled` declared
- `PluginConfig.cs:108` — Bound to config: `"Removes weight penalties."`
- `ModMenu.cs:88-91` — Toggle displayed in General tab
- `StatusWindow.cs:27` — Shows "Weight: ON/OFF" in status HUD
- `MasterToolPlugin.cs:162` — Hotkey toggles the value

**But:** There is no feature class, no logic in `Update()`, and `NoWeightEnabled` is never read anywhere to actually modify weight behavior.

#### Problem

Users see a "No Weight Penalties" toggle in the menu and status window. They can enable it, see "Weight: ON" in the HUD, and believe it's working — but it does absolutely nothing.

#### Proposed Fix

Either implement the feature (likely requires patching the weight calculation in `Physical` or the inventory system) or remove the toggle and config entry until it's implemented. Shipping a non-functional toggle is misleading.

---

### BUG-4: ChamsManager leaks shader references for despawned players [LOW]

**File:** `src/MasterTool/ESP/ChamsManager.cs` lines 73–74

#### Problem

The `_originalShaders` dictionary maps `Renderer → Shader`. When a player despawns and their `Renderer` component is destroyed by Unity, the dictionary entry becomes a dangling reference. Over a long raid:

1. Dictionary grows without bound (minor memory leak)
2. `ResetChams()` iterates entries for non-existent renderers
3. Null checks in `ResetChams` prevent crashes, but dead entries accumulate

#### Proposed Fix

Periodically purge entries where the Renderer key is null (destroyed by Unity):

```csharp
private float _nextCleanup;

// In Update(), after the main loop:
if (Time.time > _nextCleanup)
{
    _nextCleanup = Time.time + 30f;
    var dead = new List<Renderer>();
    foreach (var kv in _originalShaders)
        if (kv.Key == null) dead.Add(kv.Key);
    foreach (var r in dead)
        _originalShaders.Remove(r);
}
```

---

### BUG-5: GameState.MainCamera never refreshes if camera changes but isn't null [LOW]

**File:** `src/MasterTool/Plugin/GameState.cs` lines 42–43

#### Current Code

```csharp
if (MainCamera == null)
    MainCamera = Camera.main ?? GameObject.Find("FPS Camera")?.GetComponent<Camera>();
```

#### Problem

`MainCamera` is only refreshed when it becomes `null`. If the active camera changes (e.g., spectator cam, death screen, cutscene), the stale reference persists. ESP and other features would use the wrong camera for screen-space projection.

In practice, Unity cameras are usually destroyed on scene change (which makes the reference null), so this is low severity. But camera swaps within a scene (spectator mode) could cause issues.

#### Proposed Fix

Always re-check during refresh:

```csharp
MainCamera = Camera.main ?? GameObject.Find("FPS Camera")?.GetComponent<Camera>();
```

Remove the `if (MainCamera == null)` guard.

---

### BUG-6, 7, 8: Unused Config Entries [LOW]

Three config entries are declared and bound but never referenced in feature code:

| Config Entry | File:Line | Intended Use | Status |
|---|---|---|---|
| `ChamsIntensity` | `PluginConfig.cs:52,165` | Brightness of chams colors | Not used in `ChamsManager` |
| `FovMelee` | `PluginConfig.cs:89,231` | FOV for melee weapons | No `"melee"` case in `VisionFeature.GetFovForCurrentWeapon` switch |
| `ColorQuestZone` | `PluginConfig.cs:79,182` | Color for quest zones | No zone rendering in `QuestEsp` |

#### Impact

Users editing the BepInEx config file will see these options and may configure them, but the values have no effect. `ChamsIntensity` is particularly misleading since the Chams feature is actively used.

#### Proposed Fix

Either implement the functionality these config entries were meant to control, or remove them to avoid confusion.

---

### BUG-9: GodMode Harmony patches fail silently [LOW]

**File:** `src/MasterTool/Features/GodMode/DamagePatches.cs` line 36

```csharp
catch { }
```

#### Problem

If a Harmony patch fails to install (e.g., method signature changed in a game update, reflection fails), the exception is swallowed. The user sees "GodMode: ON" in the status window and believes they're protected, but damage is not being blocked.

#### Proposed Fix

Log a warning on patch failure:

```csharp
catch (Exception ex)
{
    MasterToolPlugin.Instance?.Logger?.LogWarning($"[GodMode] Failed to patch {type.Name}.{methodName}: {ex.Message}");
}
```

---

## Features Confirmed Clean

These features were audited and found to have no NV-like bugs:

| Feature | Why Clean |
|---|---|
| **GodMode** (DamagePatches) | Harmony prefix is passthrough when OFF — never modifies game state unless blocking damage |
| **InfiniteStamina** | Only runs when enabled; stamina naturally drains when OFF |
| **Speedhack** | Additive displacement per frame; no persistent state to clean up |
| **DoorUnlock** | One-shot operation (button press), no per-frame concerns |
| **Teleport** | One-shot operation (button press), no per-frame concerns |
| **VisionFeature** | Already fixed in v2.2.0 with state-tracking pattern |
| **PlayerEsp** | Read-only (builds display list), no game state modification |
| **ItemEsp** | Read-only (builds display list), no game state modification |
| **QuestEsp** | Read-only (builds display list), no game state modification |
| **Update() ordering** | All features correctly placed before/after weapon guard after v2.2.0 fix |

---

## Server-Side Considerations

The SPT server is primarily a **data server** that manages profiles, inventory, quests, and trader state. Key observations:

- **Health/Damage**: Calculated entirely client-side. The server stores health state in the profile on raid end but does not validate damage during gameplay. GodMode is safe.
- **Stamina**: Client-side `Physical` component. Server does not track real-time stamina. Infinite stamina is safe.
- **Movement/Speed**: Client-side `Transform` updates. The SPT server does not validate movement speed or position. Speedhack is safe.
- **Doors**: Door state is tracked client-side via Unity `Door` components. The server initializes door states at raid start but doesn't validate runtime changes. Door unlock is safe.
- **Bot Activation**: Bot lifecycle is managed client-side by `GameWorld.RegisteredPlayers`. The server spawns bots via the bot spawn service but doesn't track their active/inactive state. Culling is safe from a server perspective, but the client-side override bug (BUG-1) still affects gameplay.
- **Player Model/Bones**: Purely client-side rendering. Server has no awareness of bone transforms. BigHead is safe from a server perspective.

---

## Priority Recommendations

1. **Fix BUG-1** (CullingFeature) — HIGH priority. Same pattern as the NV bug with real gameplay impact.
2. **Fix BUG-2** (BigHeadFeature) — MEDIUM priority. Same pattern, lower real-world impact.
3. **Address BUG-3** (NoWeight) — MEDIUM priority. Either implement or remove the misleading toggle.
4. Fix remaining LOW severity bugs as time permits.

---

## Testing Checklist

- [ ] Toggle CullingFeature ON, verify bots beyond distance are deactivated
- [ ] Toggle CullingFeature OFF, verify only mod-deactivated bots are re-enabled (not all bots)
- [ ] Verify bots deactivated by the game (dead, despawned) are NOT re-enabled by the mod
- [ ] Toggle BigHead ON/OFF, verify only mod-scaled heads are reset
- [ ] Verify NoWeight toggle either works or is removed
- [ ] Long raid test: verify ChamsManager doesn't accumulate stale shader entries
- [ ] Camera transition test: verify ESP uses correct camera after spectator/death
- [ ] GodMode: verify patch installation is logged (success or failure)
