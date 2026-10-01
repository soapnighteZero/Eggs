# Eggs Handoff

## Current Milestone

M1A - Drag and Work Zones. M1B has not started.

## Status

Terminology & Art Foundation Update completed. Art Foundation is established.

M1A previously Play-verified before terminology rename.
Post-rename Unity smoke test passed. M1A has been re-verified under Unit terminology, with gameplay behavior unchanged from before the rename.

Both the pre-rename Play result and the completed post-rename Unity smoke test were reported by the user. The manual results are recorded below. Codex only updated this handoff to record the results; it did not run or control Unity. M1B has not started.

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

**Art Foundation established:** Runtime uses generic Unit terminology, character identity belongs to Creature presentation, and prefab/Inspector references allow artwork to be replaced independently of gameplay code. No specific species is required.

The current prefab keeps `UnitActor`, `CircleCollider2D`, `SpriteRenderer`, `SortingGroup` and `M1ADebugLabel` on its root, with a TextMesh State Label child. Its SpriteRenderer sprite and material can already be changed through the Prefab/Inspector. There is no Animator dependency in gameplay code; artists can add/configure one in the prefab when animations are available. No visual hierarchy refactor was needed.

Final Creature identity belongs to presentation. Replacing a sprite or configuring an Animator must not require changes to `UnitActor`. Placeholder circles remain debug visuals. All playable population Units still share the same gameplay rules.

Existing art preparation remains available: nine production PNGs under `Assets/Eggs/Art/`, eight reference PNGs under `docs/art/reference/`, an overview, the source layout preview and a layer manifest. The original PSD and all exported image files/importer metadata were preserved. The archer and guardian remain references. See [ART_ASSETS.md](ART_ASSETS.md) for the exported-art inventory and import notes. No art was newly connected to the scene in this terminology update.

Current art todo: **Playable population Creature / Unit base visual still needs final confirmation or artwork.** No specific species is required.

## Static Verification During Terminology Migration

- Branch: `Dada`. HEAD: `1e947b6c090e3739646f80edb60820f4ef4e4c0e`. Unity version: `6000.6.3f1`.
- Offline C# compilation passed for five Runtime files and two Editor files using the installed Unity Roslyn compiler, Unity reference assemblies and local Input System assembly. Runtime was compiled separately without UnityEditor references. Only the normal unused serialized-field warning CS0649 was suppressed.
- Before/after comparisons confirmed that all seven source files and both scene/prefab texts differ only by the reviewed terminology mapping. Gameplay expressions, values and control flow were preserved.
- All four renamed `.meta` files retained their exact contents. Old asset paths are absent; asset GUIDs are unique.
- Strict YAML parsing passed. Scene/prefab file IDs are unchanged, nine custom Runtime script bindings resolve, and all four prefab instances and their override targets resolve. Debug-label, controller and zone references were checked; the Unit layer mask is unchanged.
- Compared against the pre-edit snapshot: 135 other pre-existing files are unchanged, including art, generated placeholders, all Packages and ProjectSettings files, SampleScene and README. The user's existing QualitySettings change remains intact.
- Current `Assets/Eggs` source and assets contain no old species terminology. Current GAME_SCOPE, README and ART_ASSETS contain none either. Retained occurrences elsewhere are the explicitly historical rename mapping/archive, the manifest's historical user decision, and AGENTS' required prohibition against assuming a species.
- Ignored Unity-generated IDE/cache files can still contain old source paths until Unity refreshes them. They were not hand-edited and are not the current source of truth.

## Post-rename Manual Unity Smoke Test — Passed

The user performed the real Unity smoke test and reported these results:

- The existing `Eggs_M1A` scene opened normally.
- `Unit 1` through `Unit 4` had no Missing Script.
- `Unit_M1A` prefab references were valid.
- Food → Love Nest → Defense → blank-space state transitions worked correctly.
- WorkZone Member Count was correct.
- Repeated dragging worked correctly.
- The Console contained zero new Errors from this update.
- `Tools > Eggs > Build M1A Prototype` rebuilt successfully.
- After rebuilding, there was still one Unit prefab and four Unit instances.
- Post-rename M1A gameplay behavior matched the pre-rename behavior.

**M1A is re-verified under Unit terminology.** The post-rename smoke test is complete. Art Foundation is established; the base visual remains the art todo above. M1B has not started.

This follow-up changed only `docs/HANDOFF.md` to record the user's results. No gameplay or asset changes were made by Codex in this follow-up.

## Files Changed in the Terminology Migration

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
- `docs/ART_ASSETS.md`: character art uses generic Creature terminology, with no prescribed species. The latest base-visual todo is recorded above.
- `docs/art/manifest.json`: current generic art decision; the original decision retained in an explicitly historical field.
- `docs/HANDOFF.md`: current names, migration, verification and the user's successful post-rename smoke test report.
- `docs/history/HANDOFF_BEFORE_UNIT_RENAME.md`: historical pre-update handoff, retained unchanged below an explanatory header.
- `README.md` was inspected and already used generic terminology; no change was needed.

## History and Working Tree

The complete previous handoff is preserved in [the historical handoff](history/HANDOFF_BEFORE_UNIT_RENAME.md). Its old filenames and then-pending Play statements describe earlier passes, before the user's later reports of successful pre-rename Play and post-rename smoke tests. They do not override the current status above.

At the start of the terminology migration, the working tree already contained an unrelated `ProjectSettings/QualitySettings.asset` modification and untracked exported art, M1A scene, prefab and generated placeholders. These were preserved; the existing scene and prefab were migrated as described above. Unstaged file renames appeared as deleted old paths plus untracked new paths; their Unity GUIDs were preserved independently of Git's display. At the start of this smoke-test documentation follow-up, `git status --short` was clean.

Git reports permission denied reading `C:/Users/95799/.config/git/ignore`; repository status remains readable. No Git configuration was changed.

Codex did not use Computer Use, launch/control Unity, change branches, merge, reset, stash, commit or push. The manual Unity test was performed by the user. The remaining art task is to confirm or create the playable population Creature / Unit base visual. M1B has not started.
