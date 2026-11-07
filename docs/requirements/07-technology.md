# 07 — Technology and progression

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Already stated (from earlier topics)

- From 01-setting: the game starts when tall walls are still standard and cannon
  are only just becoming effective (real-world equivalent ≈ 1494).
- From 01-setting: dynasties are named after real scientists / engineers. Idea to
  discuss here: a dynasty's expertise follows its namesake's field (Terzaghi → earth
  works, Tartaglia → ballistics). Caution: as an abstract bonus this would break
  pillar 1.

## Questions

### Progression model
Tech tree, research points, gradual diffusion, ideas / innovations, or emergent from
player designs?

**Decision:** **Geometry is never locked; production capabilities are.** The
player can design any fortress shape from day one and discovers what works by
experiment (pillar 3). Technology unlocks manufacturing capabilities through building
upgrades (powder mill, foundry), which change physical parameters — never abstract
combat modifiers.

### Gunpowder technology
E.g. serpentine vs. corned powder, saltpetre refining. How does it change ballistics?

**Decision:** **Serpentine powder at start → corned powder upgrade:** more energy
per kg, and a higher burst risk when overcharged.

### Casting technology
E.g. wrought iron vs. bronze vs. cast iron, trunnions, wheeled carriages. Effects on
guns (weight, range, rate of fire, burst risk)?

**Decision:** **Start:** wrought-iron bombards, bronze guns. **MVP upgrades:**
cast-iron shot, trunnions + carriages (faster aiming, better mobility).

### Fortification evolution
Stages to model (e.g. tall curtain walls → lowered / thickened walls → earth-backed
walls → bastions → ravelins / outworks). Are they unlocked or simply discovered by
the player in the simulation?

**Decision:** **Not unlocked — emergent from the editor.** AI states follow
era templates over time (tall walls → thickened walls → bastioned traces).

### Engineer characters
Do engineer characters exist? What do they do (design bonuses, unlocks, speed)?
How do they relate to ruling dynasties (all rulers are engineers — 00-vision)?

**Decision:** **MVP: none beyond ruler and heir** — the ruler is the engineer.
Dynasty namesake idea: each dynasty starts with a distinct starting blueprint set
(e.g. Terzaghi → earthwork templates), not a bonus. **Later:** hired engineers.

### Player-made designs
Can the player's own fort / gun designs be saved and reused as blueprints?

**Decision:** **Yes, in the MVP.** Fortress designs are saved as blueprints;
the same mechanism provides AI templates.

### Ahead of the curve
Can a player reach star forts much earlier than the rest of the world? Are there
limits?

**Decision:** **No limit on geometry;** materials and guns are limited by
buildings and upgrades.

### Diffusion between states
Does technology spread between states (trade, war, captured engineers)?

**Decision:** **MVP:** AI templates advance over time. **Later:** AI copies
designs that defeated it.
