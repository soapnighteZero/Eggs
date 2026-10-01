# Historical Handoff Snapshot — Before Unit Terminology

This is the unchanged handoff content captured before the 2026-10-01 terminology migration.
Old names and verification statuses below describe earlier passes only.
For current rules, paths and verification status, read `docs/HANDOFF.md` and `AGENTS.md`.
The user subsequently reported that M1A passed Play before the rename.

---

# Eggs Handoff

## Current Milestone

M1A - Drag and Work Zones

## Status

Implementation complete, pending Play verification.

**Pending Unity Compile / Play Verification** — 待人工 Play 验证。

## Completed

- Added `DogWorkState`: Idle, Gathering, Breeding, Defending.
- `DogUnit` owns its current state and current zone. All assignment changes go through `AssignTo`; the old zone removes the dog before the new zone registers it. Disabling a dog or zone clears its assignment.
- One generic `WorkZone` supplies the target state, collider drop area, optional snap point, read-only member list and member count. The generated zones do not overlap and leave snap points empty so dogs keep their individual drop positions.
- One `DogDragController` reads the existing Input System mouse and picks a single dog with Physics2D. It preserves the grab offset, immediately unassigns on pickup, and assigns by the dog's center on release.
- Drops outside all zones become Idle. Drops inside multiple zones warn and reject assignment. Losing application focus, disabling the controller, or releasing outside the camera viewport cancels assignment and leaves the dog Idle. Drag positions stay within the camera view.
- Physics2D transforms are explicitly synchronized before queries because the current project disables automatic transform synchronization.
- Optional TextMesh labels show dog names/states and zone member counts without TMP resource import. Inspectors show read-only state, current zone and member references. Zone configuration is locked in the normal Inspector during Play.
- Added `Tools > Eggs > Build M1A Prototype`. Its source creates a camera, controller, four colored dog prefab instances, three labeled zones and all references.
- The tool stops if any open scene has unsaved changes. It asks before replacing the M1A scene, then builds a fresh scene rather than appending objects. Existing generated prefab, textures and material are reused. Only newly created assets and the target scene are saved; no unrelated scene or project setting is saved by the tool.
- Sprites, font, materials and colors remain Inspector/Prefab references. Zone placeholder colors are on each zone's child SpriteRenderer. No gameplay code loads final art paths.
- Added `.meta` files for all new scripts and their folders, preserving existing metadata.
- No Food value/production, breeding process, eggs, population growth, combat or later-milestone systems were implemented. Breeding and Defending are work-state names only.

## Actual Verification

- Confirmed branch `Dada`, HEAD `e62f35382294d187110f46a40922110521b70787`, Unity `6000.6.3f1 (45d8eee7de74)`, and Input System package `1.20.0`.
- Inspected the actual SampleScene, renderer, Graphics/Quality/Physics2D settings, package manifest and installed URP sprite shader. Active input handling is Input System.
- Offline C# checks passed for the five runtime files and two Editor files using the installed Unity Roslyn compiler, Unity 6000.6.3f1 reference assemblies and local Input System assembly. Runtime and Editor were compiled separately; the runtime reference set excluded UnityEditor. Standard unused serialized-field warning CS0649 was suppressed for this external check.
- Reviewed namespace/file references, drag cancellation, repeated assignment, membership cleanup and overlapping-zone rejection. Runtime has no UnityEditor dependency, no GameObject.Find wiring and no legacy Input calls.
- This external compiler check does not execute Unity's asset import, scene builder, serialization, rendering or Play loop. Those results remain unverified.

## Play Verified

None. Pending Play Verification.

No Computer Use was used. Unity was not launched or controlled, the setup menu was not executed, and no Play test was performed.

## Pending Verification

### Manual setup

1. Open this project in Unity 6000.6.3f1 and wait for compilation/import. Check the Console for errors.
2. Save or close any unsaved scene yourself; the builder will stop while an open scene is dirty.
3. Click `Tools > Eggs > Build M1A Prototype`.
4. The tool saves and opens `Assets/Eggs/Scenes/Eggs_M1A.unity`. Use a landscape Game view (16:9 recommended), then enter Play.

### Required player flow

| Check | Action and expected result |
| --- | --- |
| A | Initially, all four dogs show Idle and each zone shows Members: 0. |
| B | Drag Dog 1 into FOOD and release: Gathering; Food Zone contains Dog 1 exactly once. |
| C | Pick up Dog 1: immediately Idle and removed from Food Zone. Drop in LOVE NEST: Breeding; only Love Nest Zone contains Dog 1. |
| D | Move Dog 1 to DEFENSE: Defending; Love Nest Zone no longer contains it. |
| E | Drop Dog 1 on blank space: Idle, Current Zone empty, absent from all zone lists. |
| F | Assign the other three dogs to different zones. Their states and membership remain independent. |
| G | Move the same dog between the three zones at least 10 times. No duplicate membership, stale member, stuck drag, duplicate object or exception. |
| H | Drag through another dog or overlap dogs: only one dog moves for a mouse press. |
| I | Click blank space: no error or unintended assignment. |
| J | Console: zero new errors from this task. |

Inspect `DogUnit` (Current State / Current Zone) and `WorkZone` (Member Count / Member) while playing to check identities as well as the on-screen counts.

### Setup and edge checks

- Exit Play, rerun the builder and confirm rebuilding. There should still be exactly four dogs, three zones, one camera and one drag controller. Reopen the saved scene and confirm references, labels and input still work.
- Verify the rebuild Cancel button and the dirty-scene guard preserve current work. Save a separate copy before rebuilding a customized M1A scene.
- While dragging, release outside the Game view or switch application focus: the dog must remain available for another drag and have no zone membership.
- If testing overlapping zone colliders, do so in a temporary scene copy: dropping a dog's center in both zones must warn and leave it Idle. The normal generated layout must produce no overlap warning.
- Record the actual compile/Play results here after manual testing. Visual readability, font rendering, asset import and the setup tool itself have not yet been run in Unity.

## Known Issues

- Git still reports permission denied reading `C:/Users/95799/.config/git/ignore`. Repository status is readable; no Git configuration was changed.
- At the start of M1A the working tree was clean, including `ProjectSettings/ProjectAuditorSettings.asset`. The earlier M0 modification was no longer reported. This task did not modify, restore or submit that file.
- No runtime result is claimed; Unity-specific failures, if any, must be reported from the pending manual checks.

## Files

Created source files:

- `Assets/Eggs/Scripts/Core/DogWorkState.cs`
- `Assets/Eggs/Scripts/Core/DogUnit.cs`
- `Assets/Eggs/Scripts/World/WorkZone.cs`
- `Assets/Eggs/Scripts/World/M1ADebugLabel.cs`
- `Assets/Eggs/Scripts/Interaction/DogDragController.cs`
- `Assets/Eggs/Editor/M1APrototypeBuilder.cs`
- `Assets/Eggs/Editor/M1ADebugInspectors.cs`
- Seven matching script `.meta` files and six folder `.meta` files (`Eggs`, `Scripts`, `Core`, `World`, `Interaction`, `Editor`).

Modified:

- `docs/HANDOFF.md`

Generated only when the user runs the menu; not generated during this pass:

- `Assets/Eggs/Scenes/Eggs_M1A.unity`
- `Assets/Eggs/Prefabs/Dog_M1A.prefab`
- `Assets/Eggs/Generated/M1A/Square.png`
- `Assets/Eggs/Generated/M1A/Circle.png`
- `Assets/Eggs/Generated/M1A/SpriteUnlit.mat`
- Unity-generated metadata for these assets and folders.

## Next

Perform and record M1A's manual Unity compilation, setup and Play checks. Fix any M1A issues before acceptance.

Only after M1A passes and is accepted may M1B begin. M1B has not started.

No branch change, merge, reset, stash, commit or push was performed.

## Art Export Pass (2026-10-01)

- User requested asset exports from `D:/01STUDY/2026gamejam/EGGART/EGGS.psd`. Inspected all 23 layers of the 1920x1080 source and preserved the source file.
- User confirmed that the ordinary dog still needs to be drawn. Existing archer and guardian characters remain references.
- Created 9 reusable PNGs under `Assets/Eggs/Art/`, with Sprite importer and folder metadata: ground, forest foreground, one enemy visual, nest base, one egg color, two fixed HUD labels and a shared bar background/fill pair.
- Created 8 reference PNGs under `docs/art/reference/`, plus an asset overview, original layout preview and `manifest.json` mapping every source layer to exports.
- Added `docs/ART_ASSETS.md` with sizes, usage, import settings, source limitations and missing assets. Ordinary dog art is the first missing item; Food/Defense zone identifiers, later animation/UI and a project font also need preparation or placeholders.
- Verified duplicate archer/egg/number layers and exact horizontal mirroring of the two enemy layers. Preserved effects and native resolution; small stroke rasterization differences from the PSD preview are documented.
- Export verification passed: 17 RGBA PNGs, all 23 source-layer mappings, unchanged source PSD hash, complete metadata, unique GUIDs and strict YAML parsing of all new importer metadata. Compared all 109 pre-existing repository files: only this HANDOFF changed.
- Only asset preparation was performed. No gameplay, scene, prefab or package integration occurred; M1A remains pending manual acceptance. No future milestone was started.
- Start-of-pass repository state: branch `Dada`, HEAD `1e947b6c090e3739646f80edb60820f4ef4e4c0e`; existing `QualitySettings.asset` modification and untracked M1A scene/prefab/generated assets were present and preserved. The M1A scene and prefab now exist on disk; their presence does not establish a Play result.
- Actual Unity import/rendering and Play remain **Pending Unity Import / Play Verification**. No Computer Use, Unity launch, commit or push was performed.
