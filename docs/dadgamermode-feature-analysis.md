# DadGamerMode Feature Analysis

Deep investigation of [DadGamerMode](https://github.com/dvize/DadGamerMode) (v1.10.2, SPT 3.x) for feature adoption into Master-Tool (v2.4.0, SPT 4.x).

---

## Executive Summary

DadGamerMode is a BepInEx plugin offering 12 configurable features across health, QOL, and hideout categories. After comparing with Master-Tool's existing feature set and researching the underlying EFT game mechanics in server-csharp, we identified **10 new features** worth adopting and **2 existing features** that could be enhanced.

**Key finding:** All in-raid mechanics (damage, health, energy, hydration, weight, reload speed, fall damage) are **client-side** — fully patchable via Harmony. Hideout production/construction are **server-managed** — harder to patch and heavily dependent on obfuscated class names.

**SPT version concern:** DadGamerMode targets SPT 3.x and uses obfuscated class names (`GClass2813`, `Class2112`, `GClass2193`, etc.) that **will differ** in SPT 4.x. All implementations must be adapted using SPT 4.x Assembly-CSharp decompilation. Public API surface (`ActiveHealthController`, `Player`, `EBodyPart`, `EFT.Hideout`) should be stable.

---

## Feature-by-Feature Analysis

### 1. Infinite Energy

| | |
|---|---|
| **DadGamerMode** | `Features/Energy.cs` — MonoBehaviour subscribing to `ActiveHealthController.EnergyChangedEvent`, resets to `Energy.Maximum` |
| **Master-Tool** | No equivalent |
| **Behavior** | Prevents energy from draining — no need to eat |
| **EFT types** | `ActiveHealthController`, `EnergyChangedEvent`, `Player` |
| **Server-csharp** | Energy drain is client-side. Max values configured in `Globals.HealthFactorsSettings.Energy.Maximum`. Off-raid regen is server-side (60/hr base + hideout bonuses) |
| **SPT 4.x concern** | `EnergyChangedEvent` is a public event on `ActiveHealthController` — should be stable |
| **Implementation** | Event-driven: subscribe to health controller event, reset to max. ~30 lines. Could share a `SustenanceFeature` class with Hydration |
| **Priority** | **HIGH** — simple, high-value, zero FPS impact (event-driven, not polling) |

### 2. Infinite Hydration

| | |
|---|---|
| **DadGamerMode** | `Features/Hydration.cs` — MonoBehaviour subscribing to `ActiveHealthController.HydrationChangedEvent`, resets to `Hydration.Maximum` |
| **Master-Tool** | No equivalent |
| **Behavior** | Prevents hydration from draining — no need to drink |
| **EFT types** | `ActiveHealthController`, `HydrationChangedEvent`, `Player` |
| **Server-csharp** | Hydration drain is client-side. Max values in `Globals.HealthFactorsSettings.Hydration.Maximum`. Destroyed stomach accelerates drain (`DestroyedStomachHydrationTimeFactor`) |
| **SPT 4.x concern** | `HydrationChangedEvent` is public — should be stable |
| **Implementation** | Identical pattern to Energy. ~30 lines. Bundle together |
| **Priority** | **HIGH** — simple, high-value, event-driven |

### 3. COD Mode (Auto-Heal)

| | |
|---|---|
| **DadGamerMode** | `Features/CODMode.cs` — MonoBehaviour with configurable heal rate, heal delay, and optional bleed/fracture removal |
| **Master-Tool** | No equivalent |
| **Behavior** | After not taking damage for N seconds, all body parts regenerate HP at a configurable rate. Optionally auto-removes negative effects (bleeds, fractures, pain, tremor) |
| **EFT types** | `ActiveHealthController`, `EBodyPart`, `HealthValue`, `Player`, `DamageInfoStruct`. Effect removal uses obfuscated types (`GClass2813`, `Class2112`) |
| **Server-csharp** | Health regeneration exists server-side only for off-raid (456.6 hp/hr base). In-raid healing normally requires med items. COD mode is a purely client-side invention |
| **SPT 4.x concern** | Core healing logic (iterating body parts, checking `Current`/`Maximum`) is stable. **Effect removal uses obfuscated types** — `ActiveHealthController.GClass2813` (effect interface) and `Class2112` (effect check class) will need SPT 4.x equivalents. `Player.BeingHitAction` event needs verification |
| **Config** | `CODModeToggle` (bool), `CODModeHealRate` (float, 0–100, default 10), `CODModeHealWait` (float, 0–600s, default 10), `CODBleedingDamageToggle` (bool — if true, bleeds/fractures persist) |
| **Implementation** | Two-part: (1) heal timer + per-body-part healing is straightforward, (2) effect removal requires finding SPT 4.x obfuscated equivalents via decompilation. Start with healing only, add effect removal later |
| **Priority** | **HIGH** — unique feature, no other mod offers this in Master-Tool |

### 4. Percentage Damage Reduction

| | |
|---|---|
| **DadGamerMode** | `Patches/ApplyDamage.cs` — modifies `damage` by `CustomDamageModeVal / 100` in the `ActiveHealthController.ApplyDamage` prefix |
| **Master-Tool** | Partial — GodMode is all-or-nothing (damage = 0 or full damage) |
| **Behavior** | Receive only a percentage of incoming damage (e.g., 50% = half damage). More nuanced than GodMode |
| **EFT types** | `ActiveHealthController`, `Player`, `DamageInfoStruct` |
| **Server-csharp** | Damage application is entirely client-side. Server only receives post-raid health state |
| **SPT 4.x concern** | We already patch `ActiveHealthController.ApplyDamage` with `___Player` injection — adding percentage logic is trivial |
| **Implementation** | Add `DamageReductionPercent` config (int, 0–100). In our existing `BlockDamagePrefix_ActiveHealthController`, when GodMode is off but reduction is < 100: `damage *= (percent / 100f)`. ~5 lines added to existing code |
| **Priority** | **HIGH** — enhances existing system, very useful for difficulty tuning |

### 5. Keep 1 Health (Anti-Lethal)

| | |
|---|---|
| **DadGamerMode** | `Patches/ApplyDamage.cs` — clamps body part health to minimum 3 HP after damage, with selection for "All" or "Head And Thorax" only |
| **Master-Tool** | No equivalent |
| **Behavior** | Prevents any body part from being destroyed by clamping post-damage HP to a minimum. You take damage but can't die |
| **EFT types** | `ActiveHealthController`, `EBodyPart`, `ValueStruct` (health values per body part) |
| **Server-csharp** | Server applies `HealthMultipliers.Blacked` when limbs reach zero. Keeping them above zero avoids blacked-limb penalties |
| **SPT 4.x concern** | Accessing per-body-part health values requires `ActiveHealthController` API. Pattern should be stable |
| **Implementation** | In ApplyDamage prefix: after applying damage percentage, check if resulting HP would go below threshold and clamp. Add `Keep1Health` (bool) and `Keep1HealthSelection` (string) configs. ~15 lines |
| **Priority** | **HIGH** — unique middle-ground between GodMode and no protection |

### 6. Headshot Ignore / Reduction

| | |
|---|---|
| **DadGamerMode** | `Patches/ApplyDamage.cs` — `IgnoreHeadShotDamage` zeros head damage entirely; `PercentageHeadShotDamageOnly` applies separate head-specific percentage |
| **Master-Tool** | No equivalent |
| **Behavior** | Either fully ignore headshot damage or apply a separate damage % to head hits |
| **EFT types** | `EBodyPart.Head`, `ActiveHealthController.ApplyDamage` (already patched) |
| **SPT 4.x concern** | `EBodyPart.Head` is stable. Body part parameter already available in ApplyDamage |
| **Implementation** | Add `IgnoreHeadshots` (bool) and `HeadDamagePercent` (int, 0–100) configs. Check `bodyPart == EBodyPart.Head` in prefix. ~10 lines |
| **Priority** | **MEDIUM** — useful but niche |

### 7. Enemy Damage Multiplier

| | |
|---|---|
| **DadGamerMode** | `Patches/ApplyDamage.cs` — when `___Player` is NOT `IsYourPlayer`, multiplies damage by `enemyDamageMultiplier` (1x–20x) |
| **Master-Tool** | No equivalent |
| **Behavior** | Increases damage dealt to enemies (bots/players). Makes weapons more lethal against targets |
| **EFT types** | `ActiveHealthController.ApplyDamage`, `Player` |
| **Server-csharp** | Damage is client-side. Ammo `Damage` values defined in item templates but applied client-side |
| **SPT 4.x concern** | We already have `___Player` injection in our prefix. Adding multiplier for non-local players is trivial |
| **Implementation** | Add `EnemyDamageMultiplier` config (float, 1–20). In prefix: if `!___Player.IsYourPlayer`, `damage *= multiplier`. ~3 lines |
| **Priority** | **MEDIUM** — fun feature, easy to add alongside damage system changes |

### 8. No Falling Damage

| | |
|---|---|
| **DadGamerMode** | `Features/FallingDamage.cs` — MonoBehaviour sets `ActiveHealthController.FallSafeHeight = 999999f` each frame |
| **Master-Tool** | No standalone toggle (GodMode blocks all damage including falls) |
| **Behavior** | Eliminates fall damage without needing full GodMode |
| **EFT types** | `ActiveHealthController.FallSafeHeight` |
| **Server-csharp** | Fall damage formula: `(fallHeight - SafeHeight) * DamagePerMeter`. Default `SafeHeight` is ~1.8m. Setting to 999999 means no fall ever exceeds safe height |
| **SPT 4.x concern** | `FallSafeHeight` is a public property on `ActiveHealthController` — should be stable |
| **Implementation** | Set `FallSafeHeight = 999999f` in Update() when enabled, reset to `1.8f` when disabled. ~15 lines. Could be in `Features/FallDamage/` |
| **Priority** | **MEDIUM** — useful standalone, but GodMode already covers this |

### 9. Magazine Reload Speed

| | |
|---|---|
| **DadGamerMode** | `Features/MagReloadSpeed.cs` — MonoBehaviour modifies `BackendConfigSettingsClass.BaseLoadTime` and `BaseUnloadTime` |
| **Master-Tool** | No equivalent |
| **Behavior** | Adjustable magazine reload/unload speed. Lower values = faster. Default load: 0.85, default unload: 0.3 |
| **EFT types** | `BackendConfigSettingsClass` (via `Singleton<>`), `BaseLoadTime`, `BaseUnloadTime` |
| **Server-csharp** | Reload speed is client-side. Server stores per-magazine `LoadUnloadModifier` and `MagDrills` skill progress but animation timing is client-controlled |
| **SPT 4.x concern** | **`BackendConfigSettingsClass` may be renamed** in SPT 4.x. Need to verify via decompilation. The `Singleton<>` pattern should be stable |
| **Implementation** | Modify singleton properties when enabled, reset to defaults when disabled. ~40 lines. New `Features/ReloadSpeed/` folder |
| **Priority** | **MEDIUM** — new mechanic, but requires verifying 4.x class names |

### 10. Weight Percentage Slider

| | |
|---|---|
| **DadGamerMode** | `Patches/OnWeightUpdatedPatch.cs` — patches `InventoryEquipment.smethod_1`, multiplies result by `totalWeightReductionPercentage / 100` |
| **Master-Tool** | Partial — `NoWeightFeature` patches same method but binary on/off (`__result = 0f`) |
| **Behavior** | Percentage-based weight reduction (0% = weightless, 100% = normal) |
| **EFT types** | `InventoryEquipment.smethod_1` (already patched by Master-Tool) |
| **Server-csharp** | Weight is entirely client-side. Server stores per-item `Weight` values. Overweight thresholds in `Stamina` settings |
| **SPT 4.x concern** | We already patch this method successfully in 4.x |
| **Implementation** | Change `NoWeightFeature` prefix to use percentage: `__result *= (weightPercent / 100f)`. Add `WeightReductionPercent` config (int, 0–100, default 0). When 0 = current behavior (weightless). ~5 lines modified |
| **Priority** | **LOW** — enhancement to existing feature |

### 11. Instant Hideout Production

| | |
|---|---|
| **DadGamerMode** | `Patches/InstantProductionPatch.cs` — patches obfuscated `GClass2193.Update`, sets production progress to 1.0 via reflection |
| **Master-Tool** | No equivalent |
| **Behavior** | Instantly completes hideout crafting (except bitcoin farm and fuel) |
| **EFT types** | `GClass2193`, `GClass2200`, `GClass2200.Class1821`, `ProductionBuildAbstractClass` — **ALL OBFUSCATED** |
| **Server-csharp** | Production is fully server-managed: `HideoutHelper.RegisterProduction()`, progress updated by `UpdateProductionTimers()`. Craft time adjusted by skills. Bitcoin farm has special GPU-based timing |
| **SPT 4.x concern** | **HIGH RISK.** Every class name is obfuscated and WILL differ. Heavy use of reflection on private fields. SPT version updates will break this frequently |
| **Implementation** | Would require: (1) decompiling SPT 4.x to find equivalent obfuscated classes, (2) rewriting all reflection targets, (3) maintaining across SPT updates. Very fragile |
| **Priority** | **LOW** — high complexity, high maintenance burden, hideout is out-of-raid |

### 12. Instant Hideout Construction

| | |
|---|---|
| **DadGamerMode** | `Patches/InstantConstructionPatch.cs` — patches `AreaData.method_0(int)`, replaces async result to immediately complete construction |
| **Master-Tool** | No equivalent |
| **Behavior** | Instantly completes hideout area upgrades |
| **EFT types** | `EFT.Hideout.AreaData`, `EFT.Hideout.Stage`, `EFT.Hideout.EAreaStatus` — partially stable namespace, but `method_0` is obfuscated |
| **Server-csharp** | Construction managed by `HideoutController.StartUpgrade()`. `ConstructionTime` per stage. `UpgradeComplete()` applies bonuses and stash upgrades |
| **SPT 4.x concern** | `EFT.Hideout` namespace types may be stable, but `method_0` is obfuscated and will change |
| **Implementation** | Moderate complexity. `AreaData` is in a stable namespace but the target method name changes per version |
| **Priority** | **LOW** — same concerns as Instant Production |

---

## Overlap Analysis

| Feature | Master-Tool | DadGamerMode | Comparison |
|---------|-------------|--------------|------------|
| **God Mode** | 7 Harmony patches (ApplyDamage, Kill, DestroyBodyPart, DoFracture, DoBleed) | ApplyDamage + DestroyBodyPart only | Master-Tool is **more comprehensive** |
| **Infinite Stamina** | `StaminaFeature` — sets Stamina, HandsStamina, Oxygen to max | `MaxStaminaComponent` — identical behavior | **Equivalent** |
| **No Weight** | `NoWeightFeature` — binary on/off (weight = 0) | `OnWeightUpdatedPatch` — percentage slider (0–100%) | DadGamerMode is **more flexible** |
| **Speedhack** | `SpeedhackFeature` — speed multiplier | Not present | Master-Tool only |
| **ESP** (Player, Item, Quest, Container, Chams) | Full ESP suite | Not present | Master-Tool only |
| **Vision** (Thermal, NV, Weapon FOV) | Full vision suite | Not present | Master-Tool only |
| **Big Head Mode** | State-tracked bone scaling | Not present | Master-Tool only |
| **Door Unlock** | Unlock all doors | Not present | Master-Tool only |
| **Performance Culling** | Deactivate distant bots | Not present | Master-Tool only |
| **Teleport** | Teleport enemies/items | Not present | Master-Tool only |
| **Mod Menu UI** | 7-tab IMGUI menu | BepInEx F12 config manager | Master-Tool is **more polished** |

---

## EFT API Findings (from server-csharp)

### Client-Side Systems (Harmony-patchable)
- **Energy/Hydration drain**: Tick-based with configurable `EnergyLoopTime`/`HydrationLoopTime`. Destroyed stomach accelerates drain. Events fired on change.
- **Fall damage**: `(fallHeight - SafeHeight) * DamagePerMeter`. Overweight reduces `SafeHeight` to `SafeHeightOverweight`.
- **Reload speed**: `BaseLoadTime`/`BaseUnloadTime` on backend config singleton. Per-magazine `LoadUnloadModifier`. `MagDrills` skill adds bonuses.
- **Weight**: Per-item `Weight` values summed client-side. Container `WeightMultipliers` reduce effective weight. Overweight thresholds in `Stamina` globals.
- **Damage**: Entirely client-side. `DamageInfo` struct carries damage value, body part, damage type. No server-side damage processing.

### Server-Side Systems (harder to modify client-side)
- **Hideout production**: Server-managed timers with skill-based craft time reduction. Bitcoin farm has GPU-based timing. Generator on/off affects speed.
- **Hideout construction**: Server-managed with `ConstructionTime` per stage. Bonuses applied on completion.
- **Off-raid healing**: Server calculates HP/energy/hydration regeneration based on time elapsed and hideout bonuses.

### Useful Constants Discovered
- Default `FallSafeHeight`: ~1.8m
- Default `BaseLoadTime`: 0.85
- Default `BaseUnloadTime`: 0.3
- Health factors enum: `None, Health, Hydration, Energy, Radiation, Temperature, Poisoning, Effect`
- Body parts: `Head, Chest, Stomach, LeftArm, RightArm, LeftLeg, RightLeg`
- Off-raid regen: 456.6 HP/hr, 60 energy/hr, 60 hydration/hr

---

## Implementation Roadmap

### Phase 1 — Quick Wins (v2.5.0)

Simple, high-value features with minimal code. All client-side, no obfuscated types.

| Feature | Estimated Complexity | New Files | Config Entries |
|---------|---------------------|-----------|----------------|
| Infinite Energy | ~30 lines | `Features/Sustenance/EnergyFeature.cs` | `InfiniteEnergyEnabled` (bool) |
| Infinite Hydration | ~30 lines | `Features/Sustenance/HydrationFeature.cs` | `InfiniteHydrationEnabled` (bool) |
| No Falling Damage | ~20 lines | `Features/FallDamage/FallDamageFeature.cs` | `NoFallDamageEnabled` (bool) |

**Also needed:** Config entries in `PluginConfig.cs`, hotkey bindings, mod menu tab integration, status window entries, unit tests.

### Phase 2 — Damage System Enhancements (v2.6.0)

Enhance the existing `DamagePatches.cs` with graduated damage control. All modifications to existing code.

| Feature | Estimated Complexity | Changes To | Config Entries |
|---------|---------------------|------------|----------------|
| Percentage Damage Reduction | ~5 lines | `DamagePatches.cs` | `DamageReductionPercent` (int, 0–100) |
| Keep 1 Health | ~15 lines | `DamagePatches.cs` | `Keep1HealthEnabled` (bool), `Keep1HealthSelection` (string) |
| Headshot Ignore/Reduce | ~10 lines | `DamagePatches.cs` | `IgnoreHeadshots` (bool), `HeadDamagePercent` (int, 0–100) |
| Enemy Damage Multiplier | ~3 lines | `DamagePatches.cs` | `EnemyDamageMultiplier` (float, 1–20) |
| Weight Percentage Slider | ~5 lines | `NoWeightFeature.cs` | `WeightReductionPercent` (int, 0–100) |

### Phase 3 — New Mechanics (v2.7.0)

More complex features requiring new systems.

| Feature | Estimated Complexity | New Files | Notes |
|---------|---------------------|-----------|-------|
| COD Mode (auto-heal) | ~100 lines | `Features/CodMode/CodModeFeature.cs` | Heal timer + body part iteration. Effect removal deferred until obfuscated types identified in 4.x |
| Mag Reload Speed | ~40 lines | `Features/ReloadSpeed/ReloadSpeedFeature.cs` | Requires verifying `BackendConfigSettingsClass` in SPT 4.x |

### Phase 4 — Hideout (Optional, v2.8.0+)

Complex features with high maintenance burden. Only pursue if there's strong demand.

| Feature | Estimated Complexity | Notes |
|---------|---------------------|-------|
| Instant Production | ~80 lines + reflection | Requires decompiling SPT 4.x to find obfuscated class names. Will break on updates |
| Instant Construction | ~60 lines + reflection | Same concerns. `EFT.Hideout` namespace partially stable |

---

## Recommendation

**Start with Phase 1** (Energy, Hydration, Fall Damage) — these are the lowest-effort, highest-value additions. They use stable public APIs, require no obfuscated types, and fill obvious gaps in Master-Tool's feature set.

**Phase 2** is also low-risk since it enhances code we already own (`DamagePatches.cs`). The percentage damage system alone would be a major quality-of-life improvement over binary GodMode.

**Phase 3** requires SPT 4.x decompilation work for COD Mode effect removal and reload speed class verification. Implement healing first (stable API), defer effect removal.

**Phase 4** should be deprioritized unless users specifically request hideout features. The maintenance burden of tracking obfuscated class names across SPT versions is significant.
