# Eggs Development Rules

## 1. Project

- Project: Eggs
- Game Jam theme: Regrowth
- Engine: Unity 6000.6.3f1
- Type: 2D single-screen real-time population / nest defense prototype
- Target platform: Windows
- Development window is extremely short.
- Playability and completion have priority over production-grade architecture.

Every new Codex task must first read:

1. AGENTS.md
2. docs/GAME_SCOPE.md
3. docs/HANDOFF.md
4. the actual relevant source / scene / prefab files

Actual repository state has priority over assumptions.

Do not reuse assumptions, interfaces, task IDs or architecture from other projects.

---

## 2. Scope Discipline

This is a strict Game Jam scope.

Only implement features explicitly allowed by:

docs/GAME_SCOPE.md

Do not invent additional gameplay systems.

If a requested implementation conflicts with GAME_SCOPE.md:
stop and report the conflict.

Do not implement future milestones before the current milestone has been
actually Play-tested and accepted.

Do not expand the project because an implementation “would be cleaner”,
“more complete” or “more game-like”.

Small, reliable and playable is preferred.

---

## 3. Computer Use

Default development mode is:

CODE / FILE / TERMINAL ONLY.

Codex must NOT control the user's:

- mouse
- keyboard
- Unity Editor UI
- browser
- Photoshop
- Figma
- Illustrator
- other desktop applications

unless the user explicitly authorizes Computer Use for that specific task.

During normal development Codex should:

- inspect and edit repository files
- write C# scripts
- write Unity Editor tooling
- prepare scene / prefab generation tools
- use terminal / Git read-only checks
- perform non-GUI validation where possible

When real Unity Play verification is required:

- stop before taking desktop control;
- explain exactly what needs to be tested;
- wait for explicit permission.

Never claim Unity Play verification passed unless it was actually performed.

---

## 4. Development Principles

- Prefer simple MonoBehaviour / C# solutions.
- Avoid unnecessary frameworks.
- Avoid unnecessary abstraction layers.
- Avoid complex event buses.
- Avoid dependency injection frameworks.
- Avoid service locators unless absolutely necessary.
- Do not add third-party packages without explicit approval.
- Do not upgrade existing packages.
- Do not modify ProjectSettings unless explicitly authorized.

Gameplay values that designers may tune should be serialized and Inspector-editable.

Examples:

- starting population
- starting food
- gather rate
- breeding food cost
- breeding duration
- egg hatch duration
- nest HP
- enemy speed
- enemy damage
- attack interval
- wave timing
- win population requirement

Do not scatter important balance numbers as unexplained hard-coded constants.

---

## 5. Source of Truth

There must be only one authoritative runtime value for each of:

- Population
- Food
- Nest HP
- current game phase
- victory / defeat state

Do not create multiple competing managers that each own the same value.

A Unit may have one current assignment/state and belong to at most one WorkZone.

Current formal work states:

- Idle
- Gathering
- Breeding

When a Unit changes work:

1. remove it from the previous work assignment;
2. assign the new state;
3. ensure old zones no longer retain it.

Gathering and Breeding Units are not eligible for automatic defense.
Idle Units are the future automatic-defense pool: an Idle Unit is available for
automatic defense in M2, whether assigned to Guard Ring or unassigned on blank space.

Defending enum value remains only for legacy M1A compatibility. It is not a
formal M1B/M2 work assignment. Do not delete or renumber it while M1A uses it.

Formal space is concentric around Love Nest: Nest Core (Breeding), Guard / Standby
Ring (Idle), then Food Rings (Gathering). Guard Ring is a real WorkZone with Idle
membership. Food production reads current members of valid, available Food Rings.
Future per-ring food availability must not create another Food state owner.

---

## 6. Gameplay / Presentation Separation

The team already has reusable art assets.

Gameplay must not depend on placeholder visuals.

Sprites, Animator, UI images, fonts and visual feedback must remain replaceable
through Prefab / Inspector where practical.

Do not hard-code final art asset paths inside gameplay C#.

Do not require editing gameplay code merely to swap:

- Creature sprite
- egg sprite
- nest sprite
- enemy sprite
- UI artwork
- fonts
- simple animation clips

Prefer Inspector references over GameObject.Find for formal scene wiring.

---

## 7. Scene Strategy

The game uses one main playable scene.

Do not create:

- additive scene architecture
- level loading framework
- multiple gameplay scenes
- procedural map system

unless explicitly approved.

Reusable runtime objects should use Prefabs where useful:

- Unit
- Egg
- Enemy

For setup-heavy work, Codex may create simple Editor tools such as:

Tools > Eggs > Build Prototype Scene

so that the user does not need to manually create many GameObjects.

Editor tools must remain simple and reproducible.

---

## 8. Verification

Compilation is not enough.

A milestone is complete only after the required real player flow has been
performed in Unity Play mode.

For each milestone:

- compile successfully
- open the intended scene
- enter Play mode
- perform the real interaction
- check the Console
- save relevant scene / prefabs
- reload when relevant
- record the real result in docs/HANDOFF.md

If Play was not actually performed, record:

“Pending Play Verification”.

Do not fabricate test results.

---

## 9. Git

Codex must not automatically:

- commit
- push
- merge
- reset
- stash
- change branches
- discard changes

unless explicitly authorized.

Preserve Unity .meta files.

Never commit:

- Library
- Temp
- Logs
- UserSettings
- Obj
- local editor caches

Do not modify another user's unrelated working files.

---

## 10. Handoff

After every meaningful development pass update:

docs/HANDOFF.md

Record facts only:

- current milestone
- current status
- completed work
- actual Play verification
- files created / modified
- known issues
- pending verification
- next recommended action

HANDOFF must not introduce new gameplay rules.

---

## Terminology and Art Asset Rules

- Runtime gameplay code uses generic Unit terminology.
- Final visual characters are Creatures and are not tied to a specific species.
- Gameplay code must never assume a Unit is a dog or any other specific animal.
- Character identity belongs to presentation / art, not core gameplay logic.
- Replacing character artwork must not require changing gameplay code.
- Unit Prefabs expose SpriteRenderer / Animator / visual child references through Prefab or Inspector.
- Final sprites, animations and VFX must not be loaded through hard-coded asset paths in gameplay code.
- Runtime logic must operate on UnitActor regardless of the final visual character.
- Placeholder circles / sprites are debug presentation only.
- Art can replace visual children without replacing core gameplay components.
- Creature visuals may vary, but all playable population units currently share the same core gameplay rules unless GAME_SCOPE.md explicitly changes this.

Do not introduce species-specific names into core gameplay classes, fields,
prefabs or systems unless the species itself becomes a confirmed gameplay mechanic.
