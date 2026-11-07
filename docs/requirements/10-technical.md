# 10 — Technical foundation

**Status:** Not started

## Already stated (initial pitch)

- Engine under consideration: Godot 4. GDScript vs. C# to be discussed.
- Data-driven design wanted: states, guns, materials etc. read from files.
- From 02-visual-style: target 1080p / 60 FPS in the siege scene on a GTX 1060-class
  GPU. Wall and fortress geometry is generated procedurally from the simulation
  model. Only assets with licences allowing commercial use.

## Questions

### Engine
Godot 4, or an alternative? Which version?

**Decision:** _TBD_

### Language
GDScript, C#, or a native extension (GDExtension in C++ / Rust) for the simulation core?

**Decision:** _TBD_

### Physics approach
Custom structural solver (truss / frame) vs. engine physics (e.g. Jolt) vs. hybrid
(custom solver for structure, engine physics for debris)?

**Decision:** _TBD_

### Architecture
Separation of simulation core and presentation; headless simulation; ECS or
scene-based; layer boundaries.

**Decision:** _TBD_

### Data format
JSON, YAML, TOML, CSV, Godot Resources (`.tres`)? Schema validation?

**Decision:** _TBD_

### Modding
Is modding support a goal? From the start or later?

**Decision:** _TBD_

### Save system
What is saved (map state, fortress designs, damaged structures)? Format, versioning,
backward compatibility?

**Decision:** _TBD_

### Determinism and timestep
Fixed timestep? Deterministic simulation (needed for replays / tests / auto-resolve)?

**Decision:** _TBD_

### Performance targets
Node / member counts per scene, projectile counts, target frame rate.

**Decision:** _TBD_

### Test strategy
Unit tests for the simulation core, golden-file tests for ballistics / structures,
automated playtests? Test framework?

**Decision:** _TBD_

### Asset storage
Git LFS for binary assets?

**Decision:** _TBD_

### CI
Automated builds and tests (e.g. GitHub Actions)? From the start or later?

**Decision:** _TBD_
