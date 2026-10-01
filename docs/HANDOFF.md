# Eggs Handoff

## Current Milestone

M1B - Food, Breeding, Egg and Population Growth

## Status

Implementation complete, pending Unity Play verification.

M1A was Play-verified under Unit terminology by the user before M1B started. M1B has not been run in Unity: **None / Pending Play Verification**. M2 has not started.

## Baseline and Working Tree

- Branch: `Dada`.
- Expected and actual baseline HEAD: `2a21badccea3bc50fab3380ebc65d5da610ac560`.
- Unity: `6000.6.3f1 (45d8eee7de74)`; existing Input System package: `1.20.0`.
- At the start of M1B, nine existing PNG `.meta` files under `Assets/Eggs/Art/` were modified. Their diffs add Sprite subasset names, IDs and rectangles. These pre-existing changes were reported and preserved byte-for-byte.
- Existing M1A scene, Unit prefab, M1A builder, art files, generated placeholders, SampleScene, Packages and ProjectSettings were preserved. Shared Unit interaction scripts received only the interaction-lock additions described below.

## Completed

### GameState and Food

- `GameState` is the only runtime owner of Food and Population. Serialized defaults are startingFood = 10 and startingPopulation = 4; public runtime values have private setters.
- `AddFood`, `TrySpendFood` and `AddPopulation` reject invalid negative changes or overspending and avoid integer overflow. Values cannot become negative. Disabling/re-enabling the component does not reset the economy.
- `FoodProductionSystem` references the existing Gathering WorkZone. Every 1-second tick adds its current MemberCount × foodPerUnitPerTick (default 1). It does not cache prior members, so dragging a Unit away removes its contribution from the next tick. Disabling the system resets the partial tick; an unavailable zone or GameState stops production.
- No Unit upkeep or additional resource was introduced.

### Interaction Lock and Breeding

- `UnitActor` exposes `IsInteractionLocked` and `SetInteractionLocked`. Disabling the Unit clears its lock and assignment. The Unit Inspector displays the lock read-only.
- `UnitDragController` ignores locked Units when selecting from overlapping colliders. If a Unit becomes locked during a drag, the controller stops dragging without clearing the newly locked assignment, and restores its sorting order.
- `BreedingSystem` uses the existing Breeding WorkZone. With no active cycle or unhatched Egg, it selects two active, unlocked members and calls `GameState.TrySpendFood` once. Default cost: 5 Food; duration: 5 seconds.
- Insufficient Food leaves both Units assigned to Breeding and unlocked, with no charge, timer or Egg. A cycle can start automatically once Food becomes sufficient.
- An active cycle holds the two participant references and a timer. It cannot charge again or start another cycle. Completion creates one Egg, unlocks both Units and calls `AssignTo(null)` on each, removing their memberships and returning them to Idle. Their positions are retained; standing inside the Nest does not reassign them.
- The active Egg reference blocks all further cycles until that Egg is destroyed after hatching. Disabling/re-enabling BreedingSystem preserves this reference. A disabled Egg still occupies the slot and can resume incubation when re-enabled.
- Disabling BreedingSystem immediately cancels its active cycle and releases both Units. If a participant, the Nest or GameState becomes unavailable, the next system update cancels the cycle and releases its remaining participants. An interrupted cycle creates no Egg and does not refund its already-paid Food; no further Food is charged.

### Egg, Hatch and Population

- `EggHatch` uses a serialized Unit prefab reference. The builder connects the existing `Unit_M1A.prefab`, preserving the same UnitActor, Collider2D, SpriteRenderer, SortingGroup and debug label behavior.
- BreedingSystem injects its scene GameState reference into the spawned Egg before Start/Update; runtime code performs no asset-path lookup.
- Default incubation is 4 seconds. The one-shot hatch flag is set before spawning, so repeated calls cannot create another Unit or add Population again.
- Hatching spawns one ordinary Unit, explicitly clears its lock/assignment, increments GameState Population once and destroys the Egg. The Unit starts Idle and can use all existing work assignments.
- The default spawn offset is `(2.6, -2.6, 0)` relative to the Egg: near the Nest, outside its center and away from the initial Unit row. The offset is Inspector-editable. There is no juvenile or species-specific logic.
- Disabling EggHatch pauses incubation; re-enabling retains its timer and hatch guard.

### HUD and Art Foundation

- `M1BHud` only reads GameState and writes Food/Population into numeric TextMesh labels. It owns no economy state or counting logic.
- Builder wiring uses the existing `food_label_zh.png` and `population_label_zh.png` for fixed Chinese labels; numbers use built-in `LegacyRuntime.ttf`. No font package or fixed-number image was added.
- Builder wiring uses `egg_neutral.png` and `love_nest_base.png` for Egg/Nest visuals. All four formal-art SpriteRenderers use white color. Colored zone rectangles remain separate placeholders.
- Optional TextMesh feedback shows `BREEDING 3.2s`, `EGG 3.5s`, and insufficient-Food feedback. GameState and BreedingSystem Inspectors show read-only runtime values.
- Art Foundation remains established: Runtime uses Unit; visual identity uses Creature and can be replaced through Prefab/Inspector without changing economy or assignment code.
- **Playable population Creature / Unit base visual still needs final confirmation or artwork.** M1B continues using the existing Unit placeholder. Archer and guardian remain references, with no abilities or careers added.

### M1B Builder

Menu: **Tools > Eggs > Build M1B Prototype**.

On manual execution it creates or rebuilds:

- `Assets/Eggs/Scenes/Eggs_M1B.unity`
- `Assets/Eggs/Prefabs/Egg_M1B.prefab` (created once, then reused)

The scene contains one camera, one UnitDragController, one GameState, FoodProductionSystem, BreedingSystem, four initial Unit prefab instances, the three WorkZones, Egg Spawn Point, HUD and debug labels. The initial Unit count comes from the newly created GameState's startingPopulation configuration, default 4. Runtime Population is not inferred from Hierarchy searches.

The builder validates existing prefab and Sprite dependencies before creating a fresh scene. Missing or ambiguous Sprites produce an error with the asset path. It supports a single Sprite subasset without modifying import settings. It stops for dirty open scenes, asks before replacing M1B, reuses existing prefabs and saves only its target scene/new Egg prefab. It does not invoke the M1A builder or modify M1A assets, project settings or packages.

**The M1B scene and Egg prefab have not been generated by Codex.** The builder has not been run; they will be created when the user executes the menu. Use a landscape 16:9 Game view for the prototype layout. Ground and forest art are not required or connected by this builder.

## Actual Static and Offline Verification

- Offline C# compilation passed for all ten Runtime and four Editor files using the installed Unity Roslyn compiler, Unity reference assemblies and existing Input System DLL. Runtime was compiled separately without UnityEditor references. The normal unused serialized-field warning CS0649 was suppressed.
- 49 offline logic assertions passed against the actual Runtime source with lightweight Unity API stubs. Checks covered safe economy mutations, current-membership Food ticks, Food scaling, insufficient-Food waiting, one charge per cycle, locked selection filtering, completion cleanup, one Egg at a time, single hatch/population increment, two successive cycles (4 → 5 → 6), normal newborn work assignment, disable cleanup and HUD reads.
- These logic checks do not execute real Unity lifecycle scheduling, asset import/serialization, physics, mouse input or rendering. They do not establish Unity Play verification. Harness/compiler files remain in ignored `.utmp/m1b/`.
- Runtime contains no UnityEditor, AssetDatabase, Resources.Load, hard-coded asset paths, population scene scans or old species terminology. No M2 gameplay was introduced.
- All new scripts/folders have metadata; 73 asset GUIDs were parsed and checked for uniqueness. Existing metadata was retained.
- Compared against the start-of-pass snapshot: 146 unrelated existing files are byte-identical, including all nine pre-existing art metadata modifications and all protected M1A assets/settings/packages.
- `git diff --check` passes for this task's tracked changes; new source/metadata files were also checked for trailing whitespace. The whole-worktree check reports 18 pre-existing trailing-space lines in the nine modified art `.meta` files (`customData` and `indices`). Those files were intentionally preserved.

## Play Verified

### M1A — Passed before M1B changes

The user performed the post-rename Unity smoke test and reported:

- The existing `Eggs_M1A` scene opened normally.
- `Unit 1` through `Unit 4` had no Missing Script.
- `Unit_M1A` prefab references were valid.
- Food → Love Nest → Defense → blank-space state transitions worked correctly.
- WorkZone Member Count was correct.
- Repeated dragging worked correctly.
- The Console contained zero new Errors from the terminology update.
- `Tools > Eggs > Build M1A Prototype` rebuilt successfully.
- After rebuilding, there was still one Unit prefab and four Unit instances.
- Post-rename M1A gameplay behavior matched the pre-rename behavior.

This preserves the M1A success record; it does not claim that the new shared interaction-lock code has already received a Unity regression test.

### M1B — None / Pending Play Verification

No Computer Use was used. Codex did not launch/control Unity, run either builder, enter Play or test real mouse input.

## Pending Manual Verification

| Check | Action and required result |
| --- | --- |
| A. Build | Let Unity import/compile with zero compile Errors. Save/close dirty scenes, then run `Tools > Eggs > Build M1B Prototype`. Confirm the M1B scene and Egg prefab are generated with valid references. |
| B. Initial | Enter Play: Population = 4, Food = 10; all four Units are draggable. Inspect the two GameState runtime values and the HUD. |
| C. Gathering | Drop one Unit into Food: Gathering, Food increases about once per second. After roughly three seconds, drag it away: further growth stops. |
| D. Scaling | Assign two Units to Food: growth is about twice the one-Unit rate. Drag either away and confirm the next tick uses the new count. |
| E. Breeding | With Food >= 5, drop two Units into Love Nest. Food drops by exactly 5 once; both Units lock and cannot be dragged; the countdown lasts about five seconds. To inspect the debit clearly, move other Units out of Food first. No repeated breeding charge occurs while waiting. |
| F. Egg | Completion creates exactly one Egg. Both participating Units unlock, become Idle and leave the Nest member list. They are draggable again. Population is still 4. Leaving them physically inside the Nest must not automatically reassign them. |
| G. Hatch | About four seconds later, the Egg disappears, exactly one new Unit appears near the Nest, and Population changes 4 → 5 once. |
| H. New Unit | Drag the new Unit into Food → Gathering, Love Nest → Breeding, Defense → Defending, blank space → Idle. Check membership as well as labels; it has the same normal Unit components. |
| I. Insufficient Food | Before a separate Play run, set GameState startingFood to 0, keeping startingPopulation at 4. With two Units in the Nest: no debit, timer, lock or Egg. Use another Unit to gather; once Food reaches 5, one cycle starts automatically. Restore startingFood = 10 after this check. |
| J. Repeat | Complete a second Breed → Egg → Hatch cycle by dropping Units into the Nest again. Population changes 5 → 6. No double hatch, duplicate debit, stuck lock or unexpected additional Egg. |
| K. Console | The full flow produces zero new Errors from M1B. Check formal-art colors, both Chinese HUD labels, numeric text and countdown readability. |

Additional regression checks:

- While an Egg exists, drop two other Units into the Nest with enough Food. They remain unlocked without a new charge; the next cycle may start only after the Egg hatches.
- In a temporary test copy, disable BreedingSystem or its GameObject mid-cycle. Both participants must be unlocked and Idle, with no later Egg from that cancelled cycle. Re-enable it and confirm fresh drops can start a new cycle. Also check disabling a participant or the Nest clears remaining locks by the next update.
- Disable/re-enable EggHatch during incubation: it pauses/resumes and never duplicates a hatch. Re-enable BreedingSystem while that Egg remains: the occupied slot must still block a new cycle.
- Exit Play, rebuild M1B, then save/reopen. Confirm one Egg prefab, four initial Units and one instance of each economy system; check rebuild Cancel and dirty-scene protection. Customized scene changes should be saved in a separate copy before rebuilding.
- Open the untouched M1A scene and do a short drag/state/member-count regression with the shared updated scripts. Its Units should remain unlocked unless explicitly locked by a system.

## Files Created / Modified

New Runtime source (each with `.meta`):

- `Assets/Eggs/Scripts/Core/GameState.cs`
- `Assets/Eggs/Scripts/Gameplay/FoodProductionSystem.cs`
- `Assets/Eggs/Scripts/Gameplay/BreedingSystem.cs`
- `Assets/Eggs/Scripts/Gameplay/EggHatch.cs`
- `Assets/Eggs/Scripts/UI/M1BHud.cs`
- New `Gameplay.meta` and `UI.meta` folder metadata.

Modified Runtime source:

- `Assets/Eggs/Scripts/Core/UnitActor.cs`
- `Assets/Eggs/Scripts/Interaction/UnitDragController.cs`

Editor source:

- New `Assets/Eggs/Editor/M1BPrototypeBuilder.cs` and `.meta`.
- New `Assets/Eggs/Editor/M1BDebugInspectors.cs` and `.meta`.
- Modified `Assets/Eggs/Editor/M1ADebugInspectors.cs` to display Interaction Locked.

Documentation:

- Updated `docs/HANDOFF.md` only. Existing design/art documents were read and preserved.

## History and Next

The earlier [historical handoff](history/HANDOFF_BEFORE_UNIT_RENAME.md) preserves pre-Unit migration records and historical names. Current runtime terminology remains Unit / Creature.

Next: the user performs and records the M1B manual Unity acceptance flow above. Fix any M1B issues before acceptance. **M2 is allowed only after M1B Play verification passes; M2 has not started.** Defending remains a work state only. No enemy, Nest HP, combat, wave, win/lose, Unit death, training or career system was added.

Git continues to warn that it cannot read `C:/Users/95799/.config/git/ignore`; repository status is readable and Git configuration was not changed.

No Computer Use, Unity launch/control, branch change, merge, reset, stash, commit or push was performed.
