# Eggs Handoff

## Current Milestone

M1B - Ring-based Regrowth Prototype

## Status

Implementation revised for concentric-ring design, pending Unity Play Verification.

The old rectangular M1B implementation was never Play Verified. Its three-zone layout has been replaced directly by the formal concentric-ring layout; it is not an acceptance target. M1A's historical Play Verified result remains preserved below. M2 rules were updated in documentation only; no M2 gameplay has been implemented.

## Baseline

- Branch: `Dada`.
- Expected and actual HEAD at the start of this revision: `5ec7b30897bbd15711f17f4f0b1601a62d385375`.
- Unity: `6000.6.3f1 (45d8eee7de74)`.
- Working tree was clean at the start of this revision.
- `Eggs_M1B.unity` and `Egg_M1B.prefab` did not exist. No serialized rectangular M1B scene/prefab needed migration. The existing M1A scene, Unit prefab and M1A builder were inspected and preserved.

## Formal Spatial Rules

Love Nest is centered at world `(0, 0)`. The M1B drag controller receives these four WorkZones in this order:

| Zone | Default radial interval | State | Purpose |
| --- | --- | --- | --- |
| Nest Core | `[0, 1.1)` | Breeding | Central breeding area and Egg spawn |
| Guard Ring | `[1.1, 2.5)` | Idle | Initial, returning and newborn Units |
| Food Ring A | `[2.5, 3.7)` | Gathering | Food production |
| Food Ring B | `[3.7, 4.9]` | Gathering | Food production |

Shared boundaries belong to the outer neighboring zone; the outermost edge belongs to Food B. This avoids both gaps and overlap rejection at shared seams. Generic Ring zones include their outer boundary by default; the M1B builder explicitly disables that option on the three inner zones.

Radii are serialized WorkZone Inspector fields. Named radius constants at the top of `M1BPrototypeBuilder` supply rebuild defaults. When editing radii in a saved scene, adjust adjacent inner/outer radii together; WorkZone geometry is measured in world units around its assigned center. Outlines track outer-radius and center changes, and selected-zone Gizmos show both bounds. The drag controller still rejects genuine overlapping valid zones with a warning.

A drop outside all valid rings becomes `CurrentZone = null`, `CurrentState = Idle`. Guard drops become `CurrentZone = Guard Ring`, `CurrentState = Idle`. No separate Guarding or Defending job was introduced.

`Defending = 3` is retained only for legacy M1A compatibility. Current formal gameplay derives automatic defense eligibility from Idle. Automatic defense is not implemented in M1B.

## What Changed and What Was Reused

### WorkZone and initial membership

- `WorkZoneShape` has `Collider = 0` and `Ring = 1`. Existing M1A YAML therefore continues to use Collider mode without asset migration. Its dropArea and snap behavior remain available.
- WorkZone accepts Units when its component is active, acceptance is enabled and geometry is valid. Idle is now a valid target state.
- Ring geometry requires a center, finite radii, innerRadius >= 0 and outerRadius > innerRadius. Invalid or disabled zones cannot accept drops.
- `GetSafePosition` preserves an existing in-ring point. Otherwise it projects the point's direction onto the ring's middle radius, preserving Z. A point at the center uses the fixed +X direction. `GetDropPosition` uses this placement in Ring mode while retaining optional snap behavior.
- `UnitActor` still owns exactly one zone/state through `AssignTo`. It now has an optional serialized initialZone applied once at Start. The builder sets it on each initial Unit instance to Guard; the shared prefab and M1A instances retain their empty default.
- `UnitDragController` was reused unchanged: locked selection filtering, single-Unit dragging, pickup removal, overlap rejection and invalid-drop handling remain intact.

### Multi-ring Food production

- FoodProductionSystem now reads `WorkZone[] foodZones`, wired to Food A and B.
- Each tick sums current MemberCount across valid, accepting Gathering zones with available resources. Null, disabled, invalid, wrong-state and unavailable entries are skipped. A reused HashSet prevents duplicate zone references from being counted twice; UnitActor's exclusive membership prevents duplicate Unit ownership.
- `resourceAvailable` / `SetResourceAvailable(bool)` is a small future per-zone availability hook. It gates Food production only, retaining geometry and membership. Both Food Rings start available.
- No random refresh, depletion, capacity, respawn timer or pickup animation was implemented.
- GameState remains the sole owner of Food and Population. Its source file is unchanged; defaults remain Food = 10 and Population = 4. The 1-second gather interval and 1 Food per Unit per tick are unchanged.

### Breeding return to Guard

- Existing two-participant selection, one-time 5 Food charge, interaction lock, 5-second timer and single-Egg restriction were reused.
- BreedingSystem now references standbyZone, optional postBreedReturnPointA/B and an optional newbornSpawnPoint to pass into the Egg.
- Completion still creates exactly one Egg and releases both participants. It now unlocks each, places it safely inside Guard and calls `AssignTo(standbyZone)`: Idle with Guard membership, removed from Nest membership.
- Builder return points use different Guard angles (45° and 135°). Invalid point locations are projected into Guard. If points are missing, participants use opposite positions at the Guard's middle radius, through `GetSafePosition`.
- Disabling/cancelling a cycle still clears locks without another charge or Egg. The already-paid Food cost is not refunded. A valid Guard is used for return; if Guard is disabled, assignment safely falls back to unassigned Idle. Invalid Guard configuration cannot start a new cycle.

### Newborn placement

- EggHatch retains its Unit prefab, 4-second timer, one-shot hatch guard, single Population increment and Egg destruction.
- Added defaultIdleZone and optional newbornSpawnPoint. BreedingSystem injects its scene Guard/spawn references alongside GameState before the Egg starts.
- The new Unit spawns at the safe Guard position, is unlocked, receives Guard membership and stays Idle. Without a spawn point, the existing relative offset is projected into Guard.
- The builder uses a newborn point at 225°, separate from both participant return points and the initial four cardinal positions. Placement uses fixed points, not collision avoidance; players can move Units if they later occupy those points.
- Incubation pauses while Guard is unavailable; it cannot increase Population by spawning an unassigned Unit during that condition.
- The existing Unit prefab is reused; there are no juvenile states or species-specific capabilities.

### Presentation and builder

Menu remains **Tools > Eggs > Build M1B Prototype**. There is only one formal M1B builder.

- Replaced the three rectangular work zones with Nest Core, Guard Ring, Food A and Food B; no Defense Zone is created.
- Four initial Unit instances are evenly placed at 0°, 90°, 180° and 270° on the Guard's middle radius, with serialized initial Guard membership.
- Four LineRenderer outer outlines and colored member-count labels show LOVE NEST, GUARD / IDLE, FOOD A and FOOD B. `RingZoneVisual` is optional presentation, initialized by the builder before saving and refreshed when radius/center changes.
- The builder creates/reuses a simple material using the installed URP Particles/Unlit shader for vertex-colored lines. No custom shader or mesh system was introduced.
- Nest, Egg and the two Chinese HUD labels still use existing art with white SpriteRenderer tint. Numeric text, counters and breeding/hatch feedback are reused. Ground and forest are left unconnected in this pass.
- M1BHud and GameState code are unchanged. The camera/HUD/legend positions were adjusted around the full ring layout; use a landscape 16:9 Game view.
- Existing dirty-scene protection, rebuild confirmation, prefab reuse and dependency errors remain. Missing art is reported in the Editor; runtime loads no asset paths.

On manual menu execution, the builder creates/rebuilds:

- `Assets/Eggs/Scenes/Eggs_M1B.unity`
- `Assets/Eggs/Prefabs/Egg_M1B.prefab`
- `Assets/Eggs/Generated/M1B/RingOutline.mat`

**These outputs have not been generated during this pass.** Codex did not run Unity or the builder.

Art Foundation remains established. **Playable population Creature / Unit base visual still needs final confirmation or artwork.** The current Unit placeholder remains in use; archer/guardian art remains reference material.

## Documentation and M2 Direction

AGENTS now lists only Idle, Gathering and Breeding as formal work states, retaining Defending solely for historical M1A compatibility.

GAME_SCOPE now records concentric space, active Food Rings and the tradeoff between Gathering, Breeding and remaining Idle defense capacity. It also records the confirmed M2 rules:

- Enemies approach from 360° around the Nest, spawning on an outer circle / near screen edges and moving straight toward the center.
- Enemies do not attack Units; Units have no HP. No NavMesh or pathfinding.
- There is no Defense Zone or manually assigned Defending work. Idle Units automatically attack the nearest Enemy in range; Gathering and Breeding Units do not attack.

These are design rules only. Enemy, combat, Nest HP, waves and win/lose remain unimplemented. M2 must wait for M1B Play acceptance.

## Actual Static / Offline Verification

- All 12 Runtime and four Editor source files compile with the installed Unity Roslyn compiler and local Unity/Input System references. Runtime is compiled separately without UnityEditor dependencies. Only ordinary unused serialized-field warning CS0649 is suppressed.
- 166 offline assertions passed against actual Runtime source with lightweight Unity API stubs. They cover retained economy/lock/single-Egg behavior, legacy enum/Collider compatibility, radial boundaries, genuine overlap rejection, invalid geometry, Idle membership, exclusive ownership, initial assignment, multi-ring totals/de-duplication, resource availability, safe placement, participant return, newborn membership, 4 → 5 → 6 growth and outline point generation.
- These checks do not run Unity import/serialization, actual lifecycle scheduling, mouse/physics interaction or rendering. They are not Unity Play verification. Harness/compiler files are in ignored `.utmp/m1b-ring/`.
- Existing M1A scene, prefab, builder and metadata are preserved. GameState, M1BHud, UnitDragController, existing art/generated resources, SampleScene, Packages and ProjectSettings are unchanged.
- Runtime contains no UnityEditor, AssetDatabase or hard-coded art paths. Formal M1B builder contains no Defense Zone or Defending assignment. No new work state, enemy/combat system or Food refresh system was added.
- `git diff --check` passes. New source metadata is supplied; existing GUIDs are retained.

## Historical Play Verification

### M1A — Play Verified before M1B

The user's post-rename Unity smoke test passed: the existing Eggs_M1A scene opened normally; Unit 1–4 had no Missing Script; Unit_M1A prefab references were valid; Food → Love Nest → Defense → blank-state changes and member counts worked; repeated dragging worked; the Console had zero new Errors; the M1A builder rebuilt successfully with one Unit prefab and four instances; gameplay matched the pre-rename behavior.

This is a historical result. The shared WorkZone extension and optional Unit startup assignment need the brief M1A regression below; no new Unity result is claimed.

### M1B — None / Pending Unity Play Verification

The rectangular implementation was never Play Verified and is superseded. Only the ring layout is the current M1B acceptance target.

## Pending Manual Acceptance

1. Allow Unity to import and compile: zero compile Errors. Save/close dirty scenes, then run **Tools > Eggs > Build M1B Prototype**.
2. Confirm central Love Nest and the three surrounding rings, four visible outlines/labels, no rectangular work zones and no Defense Zone.
3. Enter Play: Food = 10, Population = 4; four Units are evenly arranged in Guard, each Idle with CurrentZone = Guard Ring. Guard Member Count = 4; other counts = 0.
4. Drag Guard → Food A: Gathering. Drag A → B: Gathering, CurrentZone = B, and A loses that member. Drag Food → Guard: Idle with Guard membership. Drag Guard → Nest: Breeding. Outside every ring: Idle with no zone.
5. One Gathering Unit yields approximately +1 Food/second. Two Units split across Food A/B yield approximately +2/second. Pickup removes the old member's contribution from the next tick.
6. With Food >= 5, drop two Units in Nest. Exactly 5 Food is charged once and both Units lock. After about 5 seconds, exactly one Egg appears; both participants unlock, return to distinct Guard positions and become Idle members. Nest count clears. Population remains 4.
7. After about 4 seconds, the Egg disappears, Population changes 4 → 5 once, and the newborn is unlocked, Idle and inside Guard with Guard membership. Drag it to Food, Nest and Guard normally.
8. Repeat by dropping two Units into Nest again: Population 5 → 6. No repeat charge, duplicate Egg/hatch, stale membership or stuck lock. Units standing in Guard never automatically re-enter breeding.
9. During an Egg's incubation, further Nest assignments must not trigger a new cycle or charge. A new cycle may begin after hatching if two members remain assigned and Food is sufficient.
10. Test insufficient Food in a separate run with startingFood = 0: two Nest members remain unlocked with no charge/timer/Egg. Gathering to 5 allows one cycle. Restore startingFood = 10 afterward.
11. In a temporary scene copy, remove optional participant return/newborn points: all placements must still be inside Guard, not Nest. Try invalid point locations and confirm clamping. Check disabling BreedingSystem or a participant clears locks; disabling Guard prevents new breeding/hatching until available again.
12. Check exact/near shared boundaries for no normal overlap warning; intentionally overlapping rings should still warn and reject assignment. Verify visual outlines and ring geometry agree.
13. Exit Play, rebuild, save/reopen: one M1B scene, one Egg prefab, four initial Units and one economy system of each type. Check Cancel/dirty-scene protection and preserved references.
14. Open the unchanged M1A scene and check its original Collider zones, including Defending, plus single-Unit drag and membership changes.
15. Full flow: zero new Console Errors; inspect HUD, ring labels, count accuracy and countdown readability.

## Files Changed

Modified Runtime:

- `Assets/Eggs/Scripts/Core/UnitWorkState.cs`
- `Assets/Eggs/Scripts/Core/UnitActor.cs`
- `Assets/Eggs/Scripts/World/WorkZone.cs`
- `Assets/Eggs/Scripts/Gameplay/FoodProductionSystem.cs`
- `Assets/Eggs/Scripts/Gameplay/BreedingSystem.cs`
- `Assets/Eggs/Scripts/Gameplay/EggHatch.cs`

New Runtime/presentation files, each with metadata:

- `Assets/Eggs/Scripts/World/WorkZoneShape.cs`
- `Assets/Eggs/Scripts/World/RingZoneVisual.cs`

Modified Editor/documentation:

- `Assets/Eggs/Editor/M1BPrototypeBuilder.cs`
- `AGENTS.md`
- `docs/GAME_SCOPE.md`
- `docs/HANDOFF.md`

## Next

Run and record the manual ring-layout M1B acceptance flow. Fix any issues before acceptance. Do not start M2 before M1B passes.

Git still warns that it cannot read `C:/Users/95799/.config/git/ignore`; status is readable and Git configuration was not changed.

No Computer Use, Unity launch/control, branch change, merge, reset, stash, commit or push was performed.
