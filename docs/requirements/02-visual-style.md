# 02 — Visual style

**Status:** Done

## Already stated (from earlier topics)

- From 00-vision: Windows, mouse + keyboard; tone is serious but failure is fun.
- From 01-setting: fictional world inspired by Renaissance Italy, nothing
  supernatural; cultures are shown through names, architectural style and visuals.

## Questions

### 2D or 3D
For each layer separately: strategic map 2D/3D; siege scene 2D/3D.

**Decision:** **Hybrid siege, 2D map.**

- **Siege scene:** 3D visuals; each wall segment is simulated with its own 2D
  cross-section truss, and the plan-view impact angle (obliquity) is taken into
  account. Captures both the profile (height, thickness) and the plan (angled
  bastion faces) at a fraction of full-3D cost. To be confirmed in topic 04
  (simulation dimensionality).
- **Strategic map:** painted 2D map, old-map / parchment look.
  **Later:** light depth (slight tilt, city and fortress sprites).

### Strategic map camera
Top-down flat, tilted 3D, globe, zoom levels?

**Decision:** **Flat top-down with 2–3 zoom levels** (region → province) and
map modes.

### Siege scene camera
Side-on cross-section (Iron Nest–like), isometric, free 3D orbit, first-person at the gun?

**Decision:** **Three modes:** a free-orbit 3D camera for placement and
design, an over-the-barrel view for aiming, and a toggleable cross-section view for
reading stresses in a wall segment.

### Art direction
Pixel art, hand-drawn, old map / parchment, engraving / woodcut, low-poly, realistic?

**Decision:** **"The engineer's notebook."** The map is ink on parchment, like
Renaissance engineering sketches. The siege scene is clean low-poly 3D with muted
Renaissance colours. Engineering overlays (stress, trajectories) are drawn as ink
lines, tying both layers to the ruler-engineer theme.

### Colour palette
Overall palette and mood for each layer.

**Decision:** **Sepia map, natural siege.**

- Map: sepia and ink tones, with a few accent colours for states.
- Siege: muted natural tones; stone, earth and timber must be clearly distinct.
- Stress overlay: readable ink red / ink blue.

### UI style
Diegetic (paper, ink, brass instruments), flat modern, period-inspired with modern
readability?

**Decision:** **Period-inspired, modern readability.** Parchment, ink and brass
textures over a clean modern layout with readable fonts. Not fully diegetic.

### Map vs. siege distinction
How do the two scenes look and feel different while still belonging to one game?

**Decision:** Follows from the art direction: the map is a 2D ink-on-parchment
drawing; the siege is a low-poly 3D scene in natural tones. The shared ink line
work (overlays, UI, portraits) ties them into one game.

### Destruction visuals
Debris, rubble piles, dust, smoke, cracks; how much is visual-only vs. simulated?

**Decision:** **Large debris is simulated; small effects are visual only.**
Large fragments fall under physics and pile up into a breach ramp (relevant for the
assault). Dust, smoke and cracks are visual only.

### Engineering overlays
Stress / load heatmaps, trajectory preview, wind indicator, range tables —
which are visible, and when?

**Decision:** **Stress on, aiming limited.**

- Stress / load heatmap and wind indicator can be toggled at any time (pillar 4:
  collapse must be readable).
- No full trajectory preview. The player learns ballistics from range tables and
  ranging shots (pillar 3).

### Character portraits
Style of character / dynasty portraits (if any).

**Decision:** **Ink-sketch portraits** in the notebook style. MVP: a few
static portraits for the rulers of the 3–5 playable states.

### Asset sourcing
Self-made, asset packs, commissioned, procedural, generated? Licensing constraints?

**Decision:** **Permissive (CC0-style) asset packs plus self-made key pieces**
(e.g. Kenney, Quaternius, Poly Haven). Wall and fortress geometry is generated
procedurally from the simulation model, not modelled by hand. Only licences that
allow commercial use are accepted (commercial goal is phased — 00-vision).

### Target resolution and performance
Minimum resolution, target frame rate, minimum hardware.

**Decision:** **1080p at 60 FPS in the siege scene** on a
roughly five-year-old mid-range GPU (GTX 1060 class). Node / member budgets are set
in topic 10.
