# 04 — Siege layer

**Status:** Not started

## Already stated (initial pitch)

- Sieges are resolved by simulation, not dice.
- Attacker places guns and fires: wind, air drag, powder charge, elevation matter.
- Defender designs their walls.
- Structures fail under real loads: truss model (nodes + members); a member that
  exceeds its threshold breaks.
- This layer is the deep one.

## Questions

### Simulation dimensionality
2D cross-section, 2.5D (2D structure in a 3D scene), or full 3D?

**Decision:** _TBD_

### Structural model
Pure truss (axial forces only), frame (bending too), or truss + voxel/chunk hybrid?
Resolution (nodes per wall)?

**Decision:** _TBD_

### Failure model
Single stress threshold, separate tension / compression / shear limits, cumulative
damage (fatigue), impact energy absorption?

**Decision:** _TBD_

### Materials
Which materials and their behaviour: stone masonry, brick, rammed earth, timber,
earthen rampart, fascines / gabions, rubble fill. Which in the MVP?

**Decision:** _TBD_

### Cannon types
E.g. bombard, culverin, saker, falconet, mortar. Which ones, and how do they differ?

**Decision:** _TBD_

### Projectile types
E.g. stone ball, iron ball, chain shot, grapeshot, incendiary, explosive shell.

**Decision:** _TBD_

### Ballistic parameters
Gravity, air drag, wind, powder charge, elevation, azimuth, ball mass/diameter, windage,
barrel wear, misfire / burst risk. Which are simulated, which are player-controlled?

**Decision:** _TBD_

### Destruction behaviour
What happens after members break: debris physics, rubble that forms a breach ramp,
collapse chains, persistent damage?

**Decision:** _TBD_

### Attacker actions
Every action available to the attacker (e.g. placing guns, building gun emplacements,
trenches / saps, mining, choosing targets, bombardment schedule, calling an assault).

**Decision:** _TBD_

### Defender actions
Every action available to the defender (e.g. wall design, counter-battery fire,
overnight repairs, counter-mining, retrenchments, sallies, surrender).

**Decision:** _TBD_

### Wall design editor
When can the defender design (peacetime only, also during siege)? Constraints:
budget, materials, build time, terrain?

**Decision:** _TBD_

### Time model inside a siege
Real-time, turn-based (per shot / per day), or mixed? How long does a siege take
in real minutes?

**Decision:** _TBD_

### Assault
Is the final assault simulated (soldiers), abstracted (breach practicability check),
or absent?

**Decision:** _TBD_

### Siege end conditions
Practicable breach, surrender, starvation, relief army, attacker withdrawal?

**Decision:** _TBD_

### Terrain
Slopes, ditches, moats, water, rock vs. soil; does terrain affect structures and guns?

**Decision:** _TBD_

### Determinism
Must the same inputs always give the same result (replays, testing, fairness)?

**Decision:** _TBD_

### Realism vs. fun in the simulation
Real-world units and values, or tuned "gamey" values?

**Decision:** _TBD_
