# PLAN

**Status:** Draft — awaiting owner approval. No game code before approval (ADR 0001).

Built on `docs/requirements/` (topics 00–11) and ADRs 0001–0007. Dates are not set
yet: they depend on the owner's weekly time budget (OQ-004). Each milestone has a
definition of done (DoD); a milestone is finished only when every DoD item holds.

## Overview

| # | Milestone | Goal | Gate |
| --- | --- | --- | --- |
| M1 | Siege prototype | Prove the core idea is fun | **Fun test** — fail twice → pivot |
| M2 | Siege vertical slice | A complete, replayable siege | Siege playable start to end |
| M3 | Campaign skeleton | Map, turns, economy, sieges linked | One full campaign loop |
| M4 | MVP | Content, AI, polish | itch.io demo |
| M5 | Post-MVP | Deferred features, by priority | — |

---

## M1 — Siege prototype

One siege scene: a **tall medieval wall** on one side, a **low angled bastion** on the
other, and **the same cannon** facing both. If this scene is not fun, nothing else
is built (11-scope-milestones).

### Definition of done

1. The same gun, with the same controls, can fire at both fortifications.
2. **Relative outcome holds:** the tall wall reaches a practicable breach in clearly
   fewer shots than the bastion (target: the bastion needs at least 2× as many),
   without hard-coded rules — only materials, geometry and physics.
3. Glancing blows on angled faces are visible and readable (ricochet, lower damage).
4. Collapse is readable: the stress overlay shows *why* a slice failed (pillar 4).
5. 1080p at 60 FPS on a GTX 1060-class GPU during bombardment.
6. Core tests pass in CI, including the determinism test.
7. The fun test has been run and its result written to `docs/playtests/m1.md`.

### Tasks

**Setup**
1. Install Godot 4 .NET (latest stable) and pin its version in the repo.
2. Create the solution: `src/Core` (pure C#), `src/Core.Tests` (xUnit),
   `game/` (Godot project referencing Core).
3. Add `.gitattributes` (line endings + Git LFS for binary assets), `.gitignore`.
4. GitHub Actions: build + `dotnet test` on Core.

**Core — ballistics**
5. Fixed-timestep integrator: gravity, quadratic drag on a sphere, wind.
6. Simple interior ballistics: powder charge + ball mass → muzzle velocity, with
   diminishing returns and burst risk when overcharged.
7. Golden tests: vacuum trajectory vs. analytic solution; drag case vs. reference.
8. Range-table generator (used by the player UI and later by the AI).

**Core — structure**
9. Material data in JSON (stone masonry, earth, timber): density, stiffness, tension
   and compression limits, damping, damage per impact.
10. 2D pin-jointed truss solver with explicit dynamics (fixed timestep); member
    failure on tension / compression limits; cumulative damage.
11. Golden tests on hand-solvable static trusses; stability test (no explosion at rest).
12. Parametric profile → slice mesh (height, thickness, batter, earth backing, parapet).
13. Fortress model: plan-view trace (polyline) + profile per segment → slices.
14. Impact model: plan-view obliquity splits each hit into normal / tangential parts;
    ricochet above a critical angle.
15. Deterministic rubble model: broken members deposit mass; breach practicability
    check (remaining height, rubble slope).
16. Sleeping slices far from impacts (performance).
17. Determinism test: scripted bombardment → state hash is stable.

**Game — scene**
18. Terrain plane with a dry ditch; two fortresses built from data (tall wall,
    bastion with angled faces); one gun.
19. Procedural meshes from slices, updated as members break.
20. Cameras: free orbit, over-the-barrel, toggleable cross-section view.
21. Stress overlay (ink red / ink blue) and wind indicator.
22. Visual-only debris chunks (Godot physics), dust and smoke.

**Game — play**
23. Aiming controls: charge, elevation, azimuth, shot type (stone / iron).
24. Range-table panel; shot counter; breach status readout.
25. Reset and switch target (tall wall ↔ bastion).
26. Playtest build (Windows export).

**Fun test**
27. Owner plus 3–5 playtesters; pass criteria in 11-scope-milestones.
28. Record results; iterate at most twice; decide go / pivot.

### Risks

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Explicit truss solver unstable or too slow | Blocks M1 | Small timestep, damping, sleeping slices; fall back to position-based dynamics |
| Bastion does not outperform the wall without fudging | Theme fails | Tune material data and profiles first; review against fortification literature |
| Slicing looks wrong (jagged, unconvincing collapse) | Pillar 4 fails | Narrower slices near impacts; weak slice coupling (ADR 0004) |
| Not fun even if physically right | Project gate | Fun test early; two iteration rounds; pivot criteria |
| Scope creep inside M1 | Delay | Only the tasks above; everything else goes to M2+ |

### Dependencies

- Godot 4 **.NET** build. This machine has the standard Godot 4.7.2 (winget), which
  cannot run C#; the .NET build (`GodotEngine.GodotEngine.Mono`) is needed.
- .NET SDK 8+ (present: 8, 9, 10).
- No dependency on any later milestone.

---

## M2 — Siege vertical slice

A complete siege from arrival to result.

**DoD:** rounds with limited shots and powder; attacker builds emplacements; defender
counter-battery fire and repairs between rounds; assault resolved deterministically;
surrender / withdrawal; three guns, two shot types, three materials; parametric
fortress editor saving blueprints; siege AI on both sides; sieges reproducible from a
seed.

**Tasks:** round structure and resource limits; emplacements; counter-battery AI;
repair action; assault resolution; editor UI and blueprint files; attacker / defender
AI; headless siege runner; contextual aiming hints.

**Risks:** editor usability; AI aiming feeling unfair; headless run too slow for
auto-resolve. **Depends on:** M1 passed.

---

## M3 — Campaign skeleton

**DoD:** a 20–30 province 2D parchment map; seasonal turns; five resources; building
slots; army tokens moving between provinces; entering a siege scene (play or
auto-resolve) and returning with persistent damage; war / peace / alliance; ruler and
heir; versioned save / load.

**Tasks:** map data and rendering; turn loop; economy; buildings; armies; siege
entry / exit; auto-resolve via the headless runner; diplomacy; succession; saves.

**Risks:** auto-resolve performance with many sieges per turn; map layer growing
beyond "thin". **Depends on:** M2.

---

## M4 — MVP

**DoD:** 3–5 playable states with distinct starting blueprints; strategic AI; era
templates for AI fortresses; small event set; fog of war and three map modes;
victory / defeat; basic audio; ink-sketch portraits; itch.io demo build.

**Risks:** content volume for a solo developer; balance. **Depends on:** M3.

---

## M5 — Post-MVP (by priority, revisited after M4)

Relief battles inside the siege scene; trenches and mining; brick, fascines, gabions;
production chains; tutorial and engineering challenges; marriage and vassalage;
randomised map elements; physics-resolved field battles; naval elements.
