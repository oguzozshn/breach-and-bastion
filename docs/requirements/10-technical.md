# 10 — Technical foundation

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Already stated (initial pitch)

- Engine under consideration: Godot 4. GDScript vs. C# to be discussed.
- Data-driven design wanted: states, guns, materials etc. read from files.
- From 02-visual-style: target 1080p / 60 FPS in the siege scene on a GTX 1060-class
  GPU. Wall and fortress geometry is generated procedurally from the simulation
  model. Only assets with licences allowing commercial use.

## Questions

### Engine
Godot 4, or an alternative? Which version?

**Decision:** **Godot 4, .NET build,** latest stable at project setup, pinned in the repo
(ADR 0005).

### Language
GDScript, C#, or a native extension (GDExtension in C++ / Rust) for the simulation core?

**Decision:** **C# throughout.** The simulation core is a plain C# library with no
Godot references. GDExtension only if profiling ever demands it.

### Physics approach
Custom structural solver (truss / frame) vs. engine physics (e.g. Jolt) vs. hybrid
(custom solver for structure, engine physics for debris)?

**Decision:** **Custom solvers for everything outcome-relevant:** C# 2D slice truss
solver, custom ballistics integrator, deterministic rubble model. Godot physics is
used only for visual debris.

### Architecture
Separation of simulation core and presentation; headless simulation; ECS or
scene-based; layer boundaries.

**Decision:**

- `Core` (pure C#): ballistics, materials, structure, siege rules,
  later campaign.
- `Game` (Godot): scenes, rendering, input, UI; reads Core state, sends commands.
- A headless runner executes Core without Godot (auto-resolve, tests).

### Data format
JSON, YAML, TOML, CSV, Godot Resources (`.tres`)? Schema validation?

**Decision:** **JSON** (System.Text.Json) with schema validation, under `data/`
(materials, guns, projectiles, later states and provinces).

### Modding
Is modding support a goal? From the start or later?

**Decision:** **Not an MVP goal;** data-driven design keeps the door open.

### Save system
What is saved (map state, fortress designs, damaged structures)? Format, versioning,
backward compatibility?

**Decision:** **Versioned JSON** of Core state, including fortress designs and damage.
Saves carry a format version; old versions are migrated or rejected cleanly.

### Determinism and timestep
Fixed timestep? Deterministic simulation (needed for replays / tests / auto-resolve)?

**Decision:** **Fixed timesteps** (structure ≈ 1/240 s, ballistics ≈
1/120 s), seeded RNG, no engine physics in outcomes. A determinism test hashes the
state after a scripted siege.

### Performance targets
Node / member counts per scene, projectile counts, target frame rate.

**Decision:** 1080p / 60 FPS (02-visual-style). Budget to validate in
Milestone 1: ≈5,000 active members at full rate; slices far from impacts sleep.

### Test strategy
Unit tests for the simulation core, golden-file tests for ballistics / structures,
automated playtests? Test framework?

**Decision:**

- xUnit tests for Core.
- Golden tests: ballistics against vacuum and known drag solutions; truss against
  hand-solvable static cases.
- Determinism test (same seed → same state hash).
- Manual playtests for fun.

### Asset storage
Git LFS for binary assets?

**Decision:** **Git LFS from the start** for binary assets (png, glb, wav, ogg);
`.gitattributes` also fixes line endings.

### CI
Automated builds and tests (e.g. GitHub Actions)? From the start or later?

**Decision:** **GitHub Actions from Milestone 1:** build and `dotnet test` on Core.
