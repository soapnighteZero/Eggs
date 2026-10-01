# Eggs Handoff

## Current Milestone

M1A - Drag and Work Zones. M1B has not started.

## Status

Terminology & Art Foundation Update completed at the file level.

M1A previously Play-verified before terminology rename.
Post-rename Unity compile / Play smoke check pending.

The pre-rename Play result was reported by the user in this update request. This pass used code, files and terminal commands only; it did not run Unity import, compilation, the builder or Play. The successful offline C# checks below do not replace those Unity checks.

## Terminology & Art Foundation Update (2026-10-01)

- Runtime uses **Unit** (`UnitActor`, `UnitWorkState`, `UnitDragController`); design and art use **Creature / 小角色 / 小生物**. Runtime makes no assumption about the final character's species.
- Renamed the three scripts and existing population prefab together with their original `.meta` files. Each metadata file is byte-identical to its pre-rename version; no GUID was generated for a renamed asset.
- Updated all related types, parameters, collections, Inspector labels, debug text and builder output. `WorkZone` now exposes `CanAcceptUnits` and `IReadOnlyList<UnitActor>`.
- Migrated the existing prefab and scene in place: script class identifiers, serialized field keys, object names and label text now use Unit. Script GUID references, prefab GUID references, file IDs and instance override targets remain intact.
- The existing scene has `Unit 1` through `Unit 4` and one `Unit Drag Controller`. All four instances reference the same renamed prefab. No second population prefab was created, and no scene rebuild was needed for migration.
- M1A gameplay behavior is unchanged. All seven C# files and both serialized assets were compared against a pre-edit snapshot and differ only by the reviewed terminology replacements.
- No Food production, breeding process, egg logic, population growth, enemy behavior, nest HP, combat, wave, win/lose, training, career or species-specific ability was added. Existing exported enemy/egg artwork remains presentation data only.

### Historical rename mapping

The old names in this table describe the migration only. They are not current terminology or valid current asset paths.

| Historical name | Current name | Preserved GUID |
| --- | --- | --- |
| `DogWorkState.cs` | `Assets/Eggs/Scripts/Core/UnitWorkState.cs` | `5da76d80f9d04f1299885cdd1e0bc8d9` |
| `DogUnit.cs` | `Assets/Eggs/Scripts/Core/UnitActor.cs` | `0c7d909a39864aafabdb923ddcff9482` |
| `DogDragController.cs` | `Assets/Eggs/Scripts/Interaction/UnitDragController.cs` | `149e15f06283426c972c7194d57dc313` |
| `Dog_M1A.prefab` | `Assets/Eggs/Prefabs/Unit_M1A.prefab` | `852c52cd4b3879c4abe05e9dc93ce4c7` |

### Current gameplay behavior

- `UnitActor.AssignTo` owns state and zone assignment. A Unit belongs to at most one WorkZone; changing work removes its previous membership first. Disabling a Unit or zone clears the assignment.
- `WorkZone` provides a target state, collider drop area, optional snap point, read-only members and count. The normal generated zones do not overlap and leave snap points empty.
- `UnitDragController` uses the existing Input System mouse and Physics2D to drag one Unit at a time. It preserves the grab offset, clears assignment on pickup and uses the Unit's center for drop assignment.
- Dropping outside all zones leaves the Unit Idle. Overlapping zones reject assignment with a warning. Losing focus, disabling the controller or releasing outside the camera viewport cancels assignment. Drag positions stay within the camera view; transforms are synchronized before Physics2D queries.
- Optional TextMesh labels and read-only Inspector fields display states and membership. Zone configuration remains locked in the normal Inspector during Play.
- `Tools > Eggs > Build M1A Prototype` builds four Unit instances and three zones. Existing dirty-scene protection, rebuild confirmation and asset reuse behavior are unchanged.

### Art integration

The current prefab keeps `UnitActor`, `CircleCollider2D`, `SpriteRenderer`, `SortingGroup` and `M1ADebugLabel` on its root, with a TextMesh State Label child. Its SpriteRenderer sprite and material can already be changed through the Prefab/Inspector. There is no Animator dependency in gameplay code; artists can add/configure one in the prefab when animations are available. No visual hierarchy refactor was needed.

Final Creature identity belongs to presentation. Replacing a sprite or configuring an Animator must not require changes to `UnitActor`. Placeholder circles remain debug visuals. All playable population Units still share the same gameplay rules.

Existing art preparation remains available: nine production PNGs under `Assets/Eggs/Art/`, eight reference PNGs under `docs/art/reference/`, an overview, the source layout preview and a layer manifest. The original PSD and all exported image files/importer metadata were preserved. The ordinary Creature visual still needs to be drawn; the archer and guardian remain references. See [ART_ASSETS.md](ART_ASSETS.md) for the full missing-art list and import notes. No art was newly connected to the scene in this terminology update.

## Actual Verification

- Branch: `Dada`. HEAD: `1e947b6c090e3739646f80edb60820f4ef4e4c0e`. Unity version: `6000.6.3f1`.
- Offline C# compilation passed for five Runtime files and two Editor files using the installed Unity Roslyn compiler, Unity reference assemblies and local Input System assembly. Runtime was compiled separately without UnityEditor references. Only the normal unused serialized-field warning CS0649 was suppressed.
- Before/after comparisons confirmed that all seven source files and both scene/prefab texts differ only by the reviewed terminology mapping. Gameplay expressions, values and control flow were preserved.
- All four renamed `.meta` files retained their exact contents. Old asset paths are absent; asset GUIDs are unique.
- Strict YAML parsing passed. Scene/prefab file IDs are unchanged, nine custom Runtime script bindings resolve, and all four prefab instances and their override targets resolve. Debug-label, controller and zone references were checked; the Unit layer mask is unchanged.
- Compared against the pre-edit snapshot: 135 other pre-existing files are unchanged, including art, generated placeholders, all Packages and ProjectSettings files, SampleScene and README. The user's existing QualitySettings change remains intact.
- Current `Assets/Eggs` source and assets contain no old species terminology. Current GAME_SCOPE, README and ART_ASSETS contain none either. Retained occurrences elsewhere are the explicitly historical rename mapping/archive, the manifest's historical user decision, and AGENTS' required prohibition against assuming a species.
- Ignored Unity-generated IDE/cache files can still contain old source paths until Unity refreshes them. They were not hand-edited and are not the current source of truth.

**Post-rename actual Unity compile / Play: pending.** Visual rendering, Unity asset import and the builder have not been executed in this pass.

## Pending Manual Verification

Verify the migrated existing scene before rebuilding it, so rebuilding cannot hide a migration problem.

1. Open the project in Unity `6000.6.3f1`, allow import/compile to finish and check the Console. Open `Assets/Eggs/Scenes/Eggs_M1A.unity` directly.
2. Confirm no Missing Script, exactly four Units, one Unit Drag Controller and three zones. Inspect `UnitActor`, label Unit references, the controller's Unit Layers and its three zone references. Verify the four instances reference `Unit_M1A.prefab`.
3. Enter Play. Initially all four Units must be Idle with zero zone members. Move Unit 1 through Food → Love Nest → Defense → blank space: Gathering → Breeding → Defending → Idle. Pickup must immediately remove its old membership; it must never belong to two zones.
4. Assign the other Units independently. Move one Unit between zones at least ten times, drag through/overlap another Unit and click blank space. Confirm single-Unit drag, no duplicate or stale membership, no stuck drag and no new Console errors. Check both labels and the UnitActor/WorkZone Inspectors.
5. While dragging, release outside the Game view or switch focus. The Unit must be Idle with no zone membership and remain draggable. If checking overlapping zones, use a temporary scene copy and expect a warning with rejected assignment.
6. Exit Play. Save a separate copy before rebuilding any customized scene. Run `Tools > Eggs > Build M1A Prototype` and confirm rebuilding. Check that only `Unit_M1A.prefab` is used, with exactly four Unit instances, three zones, one camera and one controller. Save/reopen and recheck references, labels and input. The rebuild Cancel action and dirty-scene guard should preserve unsaved work.
7. Record the actual post-rename Unity compile and Play results here. Do not start M1B as part of this update.

## Files Changed in This Update

### Runtime

- `Assets/Eggs/Scripts/Core/UnitWorkState.cs` (renamed with metadata)
- `Assets/Eggs/Scripts/Core/UnitActor.cs` (renamed with metadata)
- `Assets/Eggs/Scripts/Interaction/UnitDragController.cs` (renamed with metadata)
- `Assets/Eggs/Scripts/World/WorkZone.cs`
- `Assets/Eggs/Scripts/World/M1ADebugLabel.cs`

### Editor

- `Assets/Eggs/Editor/M1APrototypeBuilder.cs`
- `Assets/Eggs/Editor/M1ADebugInspectors.cs`

### Existing assets

- `Assets/Eggs/Prefabs/Unit_M1A.prefab` (renamed with metadata, serialized terminology migrated)
- `Assets/Eggs/Scenes/Eggs_M1A.unity` (serialized terminology migrated; scene metadata preserved)
- Existing `Assets/Eggs/Generated/M1A/Circle.png`, `Square.png`, `SpriteUnlit.mat`, their metadata and all art resources were unchanged.

### Documentation

- `AGENTS.md`: current Unit terminology and the required Terminology and Art Asset Rules section.
- `docs/GAME_SCOPE.md`: generic character terminology; all gameplay rules retained.
- `docs/ART_ASSETS.md`: current missing character is a generic Creature, with no prescribed species.
- `docs/art/manifest.json`: current generic art decision; the original decision retained in an explicitly historical field.
- `docs/HANDOFF.md`: current names, migration, verification and pending smoke check.
- `docs/history/HANDOFF_BEFORE_UNIT_RENAME.md`: historical pre-update handoff, retained unchanged below an explanatory header.
- `README.md` was inspected and already used generic terminology; no change was needed.

## History and Working Tree

The complete previous handoff is preserved in [the historical handoff](history/HANDOFF_BEFORE_UNIT_RENAME.md). Its old filenames and then-pending Play statements describe earlier passes, before the user's later report of a successful pre-rename Play check. They do not override the current status above.

At the start of this update, the working tree already contained an unrelated `ProjectSettings/QualitySettings.asset` modification and untracked exported art, M1A scene, prefab and generated placeholders. These were preserved; the existing scene and prefab were migrated as described above. Unstaged file renames may appear in `git status` as deleted old paths plus untracked new paths until staging; their Unity GUIDs are preserved independently of Git's display.

Git reports permission denied reading `C:/Users/95799/.config/git/ignore`; repository status remains readable. No Git configuration was changed.

No Computer Use, Unity launch/control, branch change, merge, reset, stash, commit or push was performed. The next action is the short manual post-rename Unity smoke check. M1B has not started.
