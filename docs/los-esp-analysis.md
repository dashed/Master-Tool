# Line-of-Sight Player ESP — Feasibility Analysis

## Executive Summary

Adding a **line-of-sight (LOS) only** mode for Player ESP is **fully feasible** with minimal code changes. The implementation uses `Physics.Linecast` per target per update cycle, inserted into the existing `PlayerEsp.Update()` filter pipeline. Performance impact is negligible (~0.01-0.03ms per frame for a typical 20-bot raid at the existing 20Hz update rate).

The feature would be exposed as a config toggle (`EspLineOfSightOnly`) in the "ESP Players" section, defaulting to `false` to preserve current behavior.

**Key technical findings:**
- `player.PlayerBones.Head.Original` is a proven bone accessor (used by `BigHeadFeature.cs`) — ideal raycast destination
- `Physics.Linecast` with a targeted LayerMask (HighPolyCollider, LowPolyCollider, Terrain) is the standard approach used by existing SPT/EFT modding tools
- No existing raycasting or LOS code exists in the codebase — this is a new capability
- The existing ESP update throttle (20Hz) naturally limits raycast frequency

---

## Current ESP Architecture

### Update Pipeline (`PlayerEsp.Update`)

```
Throttle gate (EspUpdateInterval, default 0.05s = 20Hz)
  → Clear Targets list
  → Guard: gameWorld, mainCamera, config enabled
  → For each player in gameWorld.RegisteredPlayers:
      → Skip if local player (IsYourPlayer)
      → Skip if dead (!IsAlive)
      → Distance check: camera → player > EspMaxDistance (400m) → skip
      → Get faction color via PlayerUtils.GetPlayerColor()
      → WorldToScreenPoint(player.position + Vector3.up * 1.8f)
      → If in front of camera (screenPos.z > 0), add EspTarget
```

**Key references available at the insertion point:**

| Reference | Type | Source |
|-----------|------|--------|
| `mainCamera` | `UnityEngine.Camera` | Parameter, cached in `GameState` |
| `mainCamera.transform.position` | `Vector3` | Camera/eye world position |
| `playerClass.Transform.position` | `Vector3` | Target world position |
| `playerClass.PlayerBones.Head.Original` | `Transform` | Target head bone (proven in `BigHeadFeature.cs:24`) |
| `playerClass` | `EFT.Player` | Full player instance with components |
| `dist` | `float` | Already computed camera→target distance |

### Render Pipeline (`PlayerEsp.Render`)

Iterates the `Targets` list and calls `EspRenderer.DrawTextWithShadow`. No changes needed — if a target fails the LOS check, it simply isn't added to `Targets`.

### Data Model (`EspTarget`)

```csharp
public class EspTarget
{
    public Vector2 ScreenPosition;
    public float Distance;
    public string Nickname;
    public string Side;
    public Color Color;
}
```

Could optionally add `bool IsVisible` for UI differentiation (dimmed labels for occluded players), but not required for the basic implementation.

---

## Unity LOS/Raycasting Technical Deep-Dive

### API Choice: `Physics.Linecast`

| Method | Signature | Use Case |
|--------|-----------|----------|
| `Physics.Raycast` | `(origin, direction, out hit, maxDist, layerMask)` | Directional ray with distance control |
| `Physics.Linecast` | `(start, end, out hit, layerMask)` | Point-to-point — simpler for LOS |

**Recommendation: `Physics.Linecast`** — cleaner for point-to-point checks since we have both positions.

```csharp
bool blocked = Physics.Linecast(
    start, end,
    out RaycastHit hitInfo,
    layerMask,
    QueryTriggerInteraction.Ignore  // skip trigger volumes
);
```

Returns `true` if any collider intersects the line. `false` = clear LOS.

### Raycast Origin and Destination

**Origin — Camera position with self-collision offset:**
```csharp
Vector3 origin = camera.transform.position;
Vector3 direction = (destination - origin).normalized;
origin += direction * 0.15f;  // Offset to avoid local player collider self-hit
```

The camera position is ideal because:
- It represents exactly what the player sees
- It accounts for leaning, crouching, and other stance changes
- It's already available as a method parameter

**Destination — Target head bone position:**
```csharp
Vector3 destination = target.PlayerBones.Head.Original.position;
```

Using the actual head bone transform (proven in `BigHeadFeature.cs:24`) is better than a fixed offset because:
- Accurately tracks the target's head regardless of stance (crouching, prone)
- More precise than `Transform.position + Vector3.up * 1.5f`
- Already used elsewhere in the codebase

**Fallback** if `PlayerBones.Head.Original` is null:
```csharp
Vector3 destination = target.Transform.position + Vector3.up * 1.6f;
```

### LayerMask Strategy

Unity physics layers control what the raycast hits. We need to hit **static geometry** (terrain, buildings, walls) but **not** players, triggers, or invisible colliders.

**Recommended approach — Named layers with hardcoded fallback:**

```csharp
private static int _losLayerMask = -1;

private static void InitLayerMask()
{
    if (_losLayerMask != -1) return;

    int mask = 0;
    int hp = LayerMask.NameToLayer("HighPolyCollider");
    int lp = LayerMask.NameToLayer("LowPolyCollider");
    int terrain = LayerMask.NameToLayer("Terrain");

    if (hp >= 0) mask |= 1 << hp;
    if (lp >= 0) mask |= 1 << lp;
    if (terrain >= 0) mask |= 1 << terrain;

    // Fallback: known EFT collision layer bitmask (from EFT Trainer)
    _losLayerMask = mask != 0 ? mask : 0x02251800;
}
```

This approach:
- Resolves layer names dynamically (handles SPT version differences)
- Falls back to a known working bitmask from the EFT Trainer project
- Only runs once (cached after first call)

**Alternative — Hit everything, check what was hit:**
```csharp
if (Physics.Linecast(origin, dest, out RaycastHit hit))
{
    return hit.transform.root == target.Transform.Original;
}
return true;
```

This is simpler but less efficient (hits player colliders unnecessarily).

---

## Proposed Implementation

### 1. Config Entry (`PluginConfig.cs`)

Add to the "ESP Players" section (after `EspMaxDistance`):

```csharp
// Field declaration
public static ConfigEntry<bool> EspLineOfSightOnly;

// In Initialize():
EspLineOfSightOnly = config.Bind("ESP Players", "Line of Sight Only", false,
    "Only show ESP labels for players you have direct line of sight to.");
```

### 2. LOS Check in `PlayerEsp.cs`

Add the LayerMask initialization and LOS check method:

```csharp
private static int _losLayerMask = -1;

private static void InitLayerMask()
{
    if (_losLayerMask != -1) return;

    int mask = 0;
    int hp = LayerMask.NameToLayer("HighPolyCollider");
    int lp = LayerMask.NameToLayer("LowPolyCollider");
    int terrain = LayerMask.NameToLayer("Terrain");

    if (hp >= 0) mask |= 1 << hp;
    if (lp >= 0) mask |= 1 << lp;
    if (terrain >= 0) mask |= 1 << terrain;

    _losLayerMask = mask != 0 ? mask : 0x02251800;
}

private static bool HasLineOfSight(Camera camera, Player target)
{
    Vector3 origin = camera.transform.position;

    var headBone = target.PlayerBones?.Head?.Original;
    Vector3 destination = headBone != null
        ? headBone.position
        : target.Transform.position + Vector3.up * 1.6f;

    // Offset origin slightly toward target to avoid camera collider self-hit
    origin += (destination - origin).normalized * 0.15f;

    InitLayerMask();

    if (!Physics.Linecast(origin, destination, out RaycastHit hit,
        _losLayerMask, QueryTriggerInteraction.Ignore))
    {
        return true;  // Nothing blocked the path
    }

    // Something was hit — visible only if we hit the target itself
    return hit.transform.root == target.Transform.Original;
}
```

Insert in `Update()` after the distance filter (line 45), before color/projection:

```csharp
if (dist > PluginConfig.EspMaxDistance.Value) continue;

// LOS filter
if (PluginConfig.EspLineOfSightOnly.Value && !HasLineOfSight(mainCamera, playerClass))
    continue;

Color textColor = PlayerUtils.GetPlayerColor(playerClass);
```

### 3. UI Toggle (`ModMenu.cs`)

Add a checkbox in the ESP Players tab:

```csharp
PluginConfig.EspLineOfSightOnly.Value =
    GUILayout.Toggle(PluginConfig.EspLineOfSightOnly.Value, "Line of Sight Only");
```

### 4. Optional: Apply to Chams (`ChamsManager.cs`)

The same LOS check could gate chams rendering so players behind walls don't get colored shaders. Add the config check to `shouldChams` in `ChamsManager.Update()`:

```csharp
bool shouldChams = PluginConfig.ChamsEnabled.Value &&
                   !playerClass.IsYourPlayer &&
                   playerClass.HealthController.IsAlive &&
                   dist <= PluginConfig.EspMaxDistance.Value &&
                   (!PluginConfig.EspLineOfSightOnly.Value
                    || PlayerEsp.HasLineOfSight(mainCamera, playerClass));
```

This would require making `HasLineOfSight` and `InitLayerMask` internal or extracting to a shared utility.

---

## Performance Analysis

### Raycast Cost

| Metric | Value |
|--------|-------|
| Cost per `Physics.Linecast` | ~0.001-0.002ms |
| Typical bots per raid | 10-30 |
| ESP update rate | 20Hz (0.05s interval) |
| Raycasts per second | 200-600 |
| Total time per second | 0.2-1.2ms |
| **Per-frame cost (at 60fps)** | **~0.003-0.02ms** |

This is negligible. Unity's bot AI already performs thousands of raycasts per frame.

### Optimization Strategies (if needed)

These are **not required** for the initial implementation but available if profiling shows issues on large maps like Streets:

1. **Already throttled:** The existing `EspUpdateInterval` (default 0.05s) means raycasts only run at 20Hz, not every frame.

2. **Distance pre-filter:** Raycasts only happen for players that pass the `EspMaxDistance` check, already bounding the set.

3. **LOS result caching:** Cache per-target results with a TTL. Players don't teleport behind walls between 0.05s intervals.

   ```csharp
   private readonly Dictionary<int, (bool visible, float time)> _losCache
       = new Dictionary<int, (bool, float)>();
   private const float LosCacheDuration = 0.15f;

   // In the check:
   int id = target.GetHashCode();
   if (_losCache.TryGetValue(id, out var cached)
       && Time.time - cached.time < LosCacheDuration)
       return cached.visible;

   bool result = /* ... linecast ... */;
   _losCache[id] = (result, Time.time);
   return result;
   ```

4. **Staggered checks:** Check 5 players per frame in round-robin instead of all at once.

5. **SphereCast fallback:** Use `Physics.SphereCast` with 0.1m radius to be more forgiving of thin geometry gaps.

---

## Edge Cases and Limitations

| Edge Case | Impact | Mitigation |
|-----------|--------|------------|
| Glass/transparent surfaces | Raycast hits glass but player is visually visible | Accept as limitation; glass is rare in EFT maps |
| Thin walls / mesh gaps | Ray might pass through gaps in collision mesh | Use SphereCast with small radius if problematic |
| Vegetation / bushes | Some have colliders, some don't | Inconsistent behavior — document as known |
| Player crouching behind low cover | Head visible but body ray blocked | Head bone destination handles this well |
| `PlayerBones.Head.Original` is null | Potential NRE | Null check with fallback to `position + Vector3.up * 1.6f` |
| Local player collider blocking ray | Ray starts inside local player | Origin offset 0.15 units toward target avoids this |
| Scene transitions | GameWorld/players become null | Already guarded by existing null checks |
| Layer names differ across SPT versions | LayerMask resolves to 0 | Hardcoded fallback mask `0x02251800` |

### Multi-Point Enhancement (Optional)

For higher accuracy with partial cover, check both head and chest:

```csharp
private static bool HasLineOfSight(Camera camera, Player target)
{
    Vector3 origin = camera.transform.position;
    var headBone = target.PlayerBones?.Head?.Original;

    Vector3[] checkPoints = {
        headBone != null ? headBone.position : target.Transform.position + Vector3.up * 1.6f,
        target.Transform.position + Vector3.up * 0.9f,  // chest
    };

    foreach (var dest in checkPoints)
    {
        Vector3 offsetOrigin = origin + (dest - origin).normalized * 0.15f;
        if (!Physics.Linecast(offsetOrigin, dest, out RaycastHit hit,
            _losLayerMask, QueryTriggerInteraction.Ignore))
            return true;
        if (hit.transform.root == target.Transform.Original)
            return true;
    }
    return false;
}
```

Costs 2x raycasts but handles partial cover (e.g., head peeking over a wall).

---

## Recommended Implementation Steps

1. **Add config entry** `EspLineOfSightOnly` in `PluginConfig.cs`
2. **Add `InitLayerMask` and `HasLineOfSight` methods** to `PlayerEsp.cs`
3. **Insert LOS filter** in `PlayerEsp.Update()` after distance check
4. **Add UI toggle** in `ModMenu.cs` ESP Players tab
5. **Test in-raid:**
   - ESP labels disappear when target goes behind wall
   - Labels reappear when target re-emerges
   - Performance is acceptable on Streets (large map, many bots)
   - Toggling the option on/off works correctly mid-raid
6. **Optional:** Apply same LOS gate to `ChamsManager` for consistency
7. **Update README** Player ESP feature description
8. **Update CHANGELOG** with new feature entry

### Files to Modify

| File | Change |
|------|--------|
| `Config/PluginConfig.cs` | Add `EspLineOfSightOnly` config entry |
| `ESP/PlayerEsp.cs` | Add `InitLayerMask()`, `HasLineOfSight()`, filter in `Update()` |
| `UI/ModMenu.cs` | Add checkbox toggle in ESP Players tab |
| `README.md` | Update Player ESP feature description |
| `CHANGELOG.md` | Add entry under `[Unreleased]` |

**Estimated diff size:** ~40 lines of new code across 3 source files.
