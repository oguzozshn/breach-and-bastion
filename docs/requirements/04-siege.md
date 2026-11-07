# 04 — Siege layer

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Already stated (initial pitch)

- Sieges are resolved by simulation, not dice.
- Attacker places guns and fires: wind, air drag, powder charge, elevation matter.
- Defender designs their walls.
- Structures fail under real loads: truss model (nodes + members); a member that
  exceeds its threshold breaks.
- This layer is the deep one.
- From 02-visual-style: hybrid presentation proposed — 3D scene, each wall segment
  simulated as its own 2D cross-section truss, plan-view impact angle included.
  Confirm under "Simulation dimensionality".
- From 02-visual-style: large debris is simulated and forms a breach ramp; dust,
  smoke and cracks are visual only.
- From 02-visual-style: no full trajectory preview — ballistics learned from range
  tables and ranging shots. Cameras: free orbit, over-the-barrel, cross-section.

## Questions

### Simulation dimensionality
2D cross-section, 2.5D (2D structure in a 3D scene), or full 3D?

**Decision:** **Hybrid (2.5D), confirmed** (ADR 0004).

- The scene is 3D. A fortification is a plan-view trace (polyline of wall segments)
  plus a cross-section profile per segment.
- Each segment is cut into **slices** (≈2 m wide). Each slice is an independent 2D
  cross-section structure in its vertical plane.
- A 3D projectile hitting a slice is split into a **normal** component (applied to the
  slice) and a **tangential** component (glancing blow / ricochet), using the
  plan-view angle between shot and face. This is what makes angled bastion faces
  matter.
- **Later:** weak coupling between neighbouring slices (shear springs).

### Structural model
Pure truss (axial forces only), frame (bending too), or truss + voxel/chunk hybrid?
Resolution (nodes per wall)?

**Decision:** **2D pin-jointed truss per slice**, triangulated, solved with
explicit dynamics at a fixed timestep. Roughly 50–150 nodes per slice. Masonry, earth
and timber differ by stiffness, strength, density and damping per member.

### Failure model
Single stress threshold, separate tension / compression / shear limits, cumulative
damage (fatigue), impact energy absorption?

**Decision:** **Separate tension and compression limits per material, plus
cumulative damage.** Masonry is far weaker in tension than in compression. Every
impact permanently lowers member strength, so walls are battered down over many
shots. Shear is captured through triangulation.

### Materials
Which materials and their behaviour: stone masonry, brick, rammed earth, timber,
earthen rampart, fascines / gabions, rubble fill. Which in the MVP?

**Decision:** **MVP: stone masonry, earth (rampart / rubble fill), timber**
(gun platforms, palisades). **Later:** brick, fascines, gabions.

### Cannon types
E.g. bombard, culverin, saker, falconet, mortar. Which ones, and how do they differ?

**Decision:** **Milestone 1: one gun.** **MVP: three** — bombard (heavy stone
shot, slow, short range), culverin (long barrel, iron shot, accurate), saker (light,
mobile). **Later:** mortar, basilisk.

### Projectile types
E.g. stone ball, iron ball, chain shot, grapeshot, incendiary, explosive shell.

**Decision:** **MVP: stone ball, iron ball.** **Later:** explosive shell;
chain shot and grapeshot only if assaults are ever simulated.

### Ballistic parameters
Gravity, air drag, wind, powder charge, elevation, azimuth, ball mass/diameter, windage,
barrel wear, misfire / burst risk. Which are simulated, which are player-controlled?

**Decision:** **Simulated:** gravity, quadratic air drag on a sphere, wind
(steady + gusts), muzzle velocity from powder charge and ball mass (simple interior
ballistics with diminishing returns), elevation, azimuth, burst risk when overcharged.
**Player-controlled:** powder charge, elevation, azimuth, shot type.
**Later:** barrel wear, windage, temperature.

### Destruction behaviour
What happens after members break: debris physics, rubble that forms a breach ramp,
collapse chains, persistent damage?

**Decision:** **Broken members free their nodes as rubble.** A deterministic
core rubble model deposits the mass at the foot of the wall and builds the breach
ramp; engine-physics debris chunks are visual only (clarifies 02-visual-style for
determinism). Damage persists after the siege (topic 05).
A breach is **practicable** when the remaining wall height is below a threshold and
the rubble slope is shallow enough to climb.

### Attacker actions
Every action available to the attacker (e.g. placing guns, building gun emplacements,
trenches / saps, mining, choosing targets, bombardment schedule, calling an assault).

**Decision:** **MVP:** place guns inside the allowed approach zone; build gun
emplacements (labour + timber; protect against counter-battery fire); aim and fire
(charge, elevation, azimuth, shot); end the round; call the assault; withdraw.
**Later:** trenches / saps, mining.

### Defender actions
Every action available to the defender (e.g. wall design, counter-battery fire,
overnight repairs, counter-mining, retrenchments, sallies, surrender).

**Decision:** **MVP:** design the fortress (beforehand, on the map); counter-battery
fire from fortress guns; repairs between rounds (labour + stone restores members);
surrender. **Later:** counter-mining, retrenchments, sallies.

### Wall design editor
When can the defender design (peacetime only, also during siege)? Constraints:
budget, materials, build time, terrain?

**Decision:** **Parametric, used on the map layer in peacetime.** The trace is a
plan-view polyline; each segment gets a profile from parameters (height, thickness,
batter, earth backing, parapet, ditch). No freeform node editing in the MVP. Costs
resources and build time (topic 05). During a siege only repairs are allowed.

### Time model inside a siege
Real-time, turn-based (per shot / per day), or mixed? How long does a siege take
in real minutes?

**Decision:** **Rounds.** A siege is played in rounds (≈ one in-world week each).
Within a round the player fires shots in real time without time pressure; the
number of shots is limited by guns and powder. Between rounds: repairs and AI
responses. Up to ~12 rounds per season turn; a siege can continue into the next turn.

### Assault
Is the final assault simulated (soldiers), abstracted (breach practicability check),
or absent?

**Decision:** **Abstracted, deterministic.** When the attacker calls the assault, the
outcome is computed from simulated quantities: breach practicability, guns that can
still fire on the breach (flanking), and troop numbers. No dice. **Later:** an
assault visualisation.

### Siege end conditions
Practicable breach, surrender, starvation, relief army, attacker withdrawal?

**Decision:** Successful assault; defender surrender (breach practicable or
supplies exhausted); attacker withdrawal (powder exhausted or player choice); relief
army arrival forces the attacker to lift the siege in the MVP. **Later:** relief
battle inside the siege scene (ADR 0002).

### Terrain
Slopes, ditches, moats, water, rock vs. soil; does terrain affect structures and guns?

**Decision:** **MVP:** heightmap with gentle slopes and a dry ditch. **Later:** water
moats, rock vs. soil foundations.

### Determinism
Must the same inputs always give the same result (replays, testing, fairness)?

**Decision:** **Yes.** Fixed timestep, seeded randomness, no engine physics in any
outcome-relevant logic. Same inputs give the same result (replays, tests,
auto-resolve).

### Realism vs. fun in the simulation
Real-world units and values, or tuned "gamey" values?

**Decision:** **SI units internally, values in data files and tuned
for play** (00-vision rule: relative outcomes true to real-world engineering).
