# Night Vision Bug — Investigation & Root Cause Analysis

## Report Summary

**Reported symptom:** "After updating, vanilla night vision stopped working."

**Root cause:** The mod's `VisionFeature` force-sets the game's `NightVision.On` and `ThermalVision.On` properties to match the config toggle **every frame**. When the mod toggles are OFF (the default), this continuously overwrites the game's native vision state, forcibly disabling vanilla NVGs the instant the game enables them.

**Severity:** Critical — the mod breaks core game functionality even when its vision features are not in use.

**Fix complexity:** ~25 lines changed across 2 files. No new files or dependencies.

---

## How EFT/SPT Handles Night Vision Natively

Understanding the native NV architecture is essential to diagnosing why the mod breaks it.

### Server-Side: Item State Only

The SPT server tracks NVG on/off state via a simple boolean on the item's `Upd` object:

```csharp
// Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs
public record UpdTogglable
{
    [JsonPropertyName("On")]
    public bool? On { get; set; }
}
```

When the player presses N to toggle NVGs, the client sends an `InventoryToggleRequestData` to the server:

```csharp
// InventoryController.cs:649-676
public ItemEventRouterResponse ToggleItem(PmcData pmcData, InventoryToggleRequestData request, ...)
{
    var itemToToggle = playerData.Inventory.Items.FirstOrDefault(x => x.Id == request.Item);
    if (itemToToggle is not null)
    {
        itemToToggle.Upd.Togglable = new UpdTogglable { On = request.Value };
    }
}
```

The server has **zero** references to `NightVisionComponent`, camera effects, or any rendering code. All NV camera effects are entirely client-side.

### Client-Side: Camera Component

The native toggle flow:

1. Player presses N → client checks if NVG item is equipped
2. Client toggles `Togglable.On` on the NVG item
3. Client sends `InventoryToggleRequestData` to server
4. Client updates the camera's `NightVision` component based on the item state
5. Server persists the `Togglable.On` state in the profile

### Desynchronization Risk

When a mod directly manipulates the camera's `NightVision.On` property **without** going through the item toggle system, it creates a state desynchronization:

- Camera `NightVision.On` = value set by mod
- Item `Togglable.On` = whatever the game last set

This means pressing N to toggle NVGs may appear inverted or broken because the camera state doesn't match what the game expects.

### Key Server-Side References

| Concept | Location | Detail |
|---------|----------|--------|
| NVG item category | `BaseClasses.cs:83` | `NIGHT_VISION = "5a2c3a9486f774688b05e574"` |
| Item defaults | `ItemHelper.cs:227-230` | NVGs default `Togglable.On = false` |
| Bot NVG chance | `BotGeneratorHelper.cs:143-153` | 90% night / 15% day active chance |
| Toggle handler | `InventoryController.cs:649-676` | Sets `Upd.Togglable.On` on item |
| NVG properties | Item templates | `HasHinge`, `Intensity`, `Mask`, `NoiseIntensity`, etc. |

---

## Bugs Found

### Bug 1 (Critical): Vision methods override vanilla NV/Thermal state every frame

**Location:** `src/MasterTool/Features/Vision/VisionFeature.cs:36-44`

```csharp
public void UpdateNightVision(Camera mainCamera)
{
    if (mainCamera == null) return;
    var nv = mainCamera.GetComponent<NightVision>();
    if (nv != null && nv.On != PluginConfig.NightVisionEnabled.Value)
    {
        nv.On = PluginConfig.NightVisionEnabled.Value;  // BUG: forces state every frame
    }
}
```

The same pattern exists for thermal vision at lines 22-30.

**What happens:**

```
Frame 1: Player equips NVGs in-game → game sets NightVision.On = true
Frame 2: UpdateNightVision() runs → nv.On (true) != config (false) → sets nv.On = false
Result:  Vanilla NVGs appear instantly broken
```

The design flaw: the mod treats "toggle OFF" as "force NV disabled" when it should mean "don't interfere with game state."

### Bug 2 (Major): NightVision update gated behind weapon check

**Location:** `src/MasterTool/Plugin/MasterToolPlugin.cs:94-100`

```csharp
_vision.UpdateThermalVision(mainCamera);              // Line 94: ALWAYS runs

if (localPlayer.HandsController == null) return;      // Line 96: EARLY RETURN
if (!(localPlayer.HandsController.Item is Weapon)) return; // Line 97: EARLY RETURN

_vision.UpdateNightVision(mainCamera);                // Line 99: SKIPPED if no weapon
_vision.UpdateWeaponFov(mainCamera, localPlayer);     // Line 100: weapon-specific (correct)
```

**Impact:**
- **Thermal vision** is force-set every frame unconditionally — always breaks vanilla thermal
- **Night vision** is only force-set when holding a weapon — creating confusing inconsistent behavior:
  - Holding a weapon → vanilla NVGs broken
  - Holding meds/keys/food → vanilla NVGs work fine
  - Users report "NVGs sometimes work and sometimes don't"

Night vision has nothing to do with whether a weapon is equipped. This is an ordering bug — `UpdateNightVision()` was placed after the weapon guard that exists for `UpdateWeaponFov()`.

### Bug 3 (Minor): BigHeadFeature also gated behind weapon check

**Location:** `src/MasterTool/Plugin/MasterToolPlugin.cs:102-103`

```csharp
if (gameWorld != null)
    BigHeadFeature.Apply(gameWorld);  // After weapon guard — stops working with non-weapons
```

`BigHeadFeature` operates on all registered players' head bones. It has no relation to the local player's equipped item. Being placed after the weapon guard means big head mode stops working when the player switches to meds, keys, or any non-weapon item.

---

## Reproduction Steps

1. Install the mod (default config — both vision toggles are OFF)
2. Enter a night raid with NVGs equipped
3. Toggle NVGs on using the in-game keybind
4. **Expected:** NVGs activate normally
5. **Actual:** NVGs flicker on for one frame then get disabled by the mod

For the inconsistent behavior (Bug 2):
1. Same setup as above
2. Switch to a medical item (Salewa, etc.)
3. Toggle NVGs → they now work (because the weapon guard skips `UpdateNightVision`)
4. Switch back to weapon → NVGs instantly disabled

---

## Proposed Fix

### Fix 1: VisionFeature.cs — State-tracking approach

Replace the force-set-every-frame pattern with a state machine that tracks whether the **mod** forced vision on:

```csharp
public class VisionFeature
{
    private float _originalFov = 75f;
    private bool _fovInitialized;
    private bool _modForcedNvOn;
    private bool _modForcedThermalOn;

    public void UpdateThermalVision(Camera mainCamera)
    {
        if (mainCamera == null) return;
        var thermal = mainCamera.GetComponent<ThermalVision>();
        if (thermal == null) return;

        if (PluginConfig.ThermalVisionEnabled.Value)
        {
            if (!thermal.On)
                thermal.On = true;
            _modForcedThermalOn = true;
        }
        else if (_modForcedThermalOn)
        {
            thermal.On = false;
            _modForcedThermalOn = false;
        }
        // else: toggle OFF and mod didn't force it — don't touch game state
    }

    public void UpdateNightVision(Camera mainCamera)
    {
        if (mainCamera == null) return;
        var nv = mainCamera.GetComponent<NightVision>();
        if (nv == null) return;

        if (PluginConfig.NightVisionEnabled.Value)
        {
            if (!nv.On)
                nv.On = true;
            _modForcedNvOn = true;
        }
        else if (_modForcedNvOn)
        {
            nv.On = false;
            _modForcedNvOn = false;
        }
        // else: toggle OFF and mod didn't force it — don't touch game state
    }

    // ... UpdateWeaponFov unchanged ...
}
```

**State transitions:**

| Mod Toggle | `_modForced` Flag | Action |
|------------|-------------------|--------|
| OFF | false | Do nothing — game controls vision |
| ON | * | Force vision ON, set flag = true |
| OFF | true | Force vision OFF, set flag = false (one-time cleanup) |

**Edge case:** If the player has real NVGs equipped AND the mod forced NV on, turning the mod toggle OFF will disable NV for one frame. The game's NVG system will re-enable it on the next tick. This is a harmless single-frame transition.

### Fix 2: MasterToolPlugin.cs — Reorder Update() calls

Move `UpdateNightVision()` and `BigHeadFeature.Apply()` before the weapon guard:

```csharp
// Before (buggy):
_vision.UpdateThermalVision(mainCamera);
if (localPlayer.HandsController == null) return;
if (!(localPlayer.HandsController.Item is Weapon)) return;
_vision.UpdateNightVision(mainCamera);
_vision.UpdateWeaponFov(mainCamera, localPlayer);
if (gameWorld != null) BigHeadFeature.Apply(gameWorld);

// After (fixed):
_vision.UpdateThermalVision(mainCamera);
_vision.UpdateNightVision(mainCamera);
if (gameWorld != null) BigHeadFeature.Apply(gameWorld);

if (localPlayer.HandsController == null) return;
if (!(localPlayer.HandsController.Item is Weapon)) return;
_vision.UpdateWeaponFov(mainCamera, localPlayer);
```

Only `UpdateWeaponFov` genuinely needs the weapon guard — it reads `weapon.Template.weapClass` to determine FOV.

---

## Files to Modify

| File | Change |
|------|--------|
| `src/MasterTool/Features/Vision/VisionFeature.cs` | Add `_modForcedNvOn` / `_modForcedThermalOn` fields; rewrite both Update methods |
| `src/MasterTool/Plugin/MasterToolPlugin.cs` | Move `UpdateNightVision()` and `BigHeadFeature.Apply()` before weapon guard |

**Estimated diff size:** ~25 lines changed across 2 files.

---

## Testing Checklist

- [ ] Vanilla NVGs work when mod vision toggles are OFF (both with weapon and non-weapon equipped)
- [ ] Vanilla thermal vision works when mod toggle is OFF
- [ ] Mod NV toggle ON forces night vision regardless of NVG equipment
- [ ] Mod NV toggle ON → OFF properly disables mod-forced NV
- [ ] Mod NV toggle ON → OFF does not break equipped vanilla NVGs (game re-enables them)
- [ ] Mod thermal toggle behaves identically to NV toggle
- [ ] Big head mode works when holding meds/keys (not just weapons)
- [ ] Weapon FOV still only activates when holding a weapon
- [ ] No visual flicker when toggling mod vision on/off rapidly
