# 05 — Links between layers

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Already stated (from earlier topics)

- From 00-vision: campaign length depends on how many sieges the player plays
  versus auto-resolves (OQ-003).
- From 03-strategic-map: every province (20–30 in the MVP) has a fortress that needs
  a design; enemy fortress designs stay hidden until scouted.

## Questions

### Economy → siege
What does the map-layer economy decide in the siege: number and type of guns, powder
stock, labour, time available, wall materials?

**Decision:** **Attacker** brings the guns built on the map, the powder carried by
the army (consumed per shot) and pioneers (labour for emplacements). **Defender**
fights with the fortress as designed and built, stored powder, the garrison and
labour for repairs. The season limits available time.

### Siege → map
What does the siege result change on the map: province ownership, damage that persists,
casualties, prestige, war score?

**Decision:** Province ownership changes on capture; fortress damage persists;
guns and powder are consumed; captured fortresses weigh the peace treaty.

### Persistent fortifications
Does a fortress designed by the player persist on the map exactly as designed, including
its damage and repairs?

**Decision:** **Yes.** The fortress persists exactly as designed, including
damage. Repairs cost resources and take turns.

### Entering the siege layer
When does the player enter a siege — every siege, only chosen ones, only their own?

**Decision:** For every siege involving the player, the player chooses:
**play it or auto-resolve it.** AI-vs-AI sieges are always auto-resolved.

### Auto-resolve
How are sieges the player does not play resolved? Must they be consistent with the
simulation (e.g. run headless)?

**Decision:** **Headless run of the same deterministic simulation**, AI on both
sides, possibly at reduced fidelity (fewer slices). Results stay consistent with
played sieges. Performance risk — budget in topic 10.

### Time conversion
How does siege time map to strategic-map time? Does the map pause during a siege?

**Decision:** The map pauses during a siege scene. One season turn holds up to
~12 rounds; an unfinished siege continues next turn and keeps its damage.

### Building fortifications on the map
Cost, build time, who designs them (player, AI, templates)?

**Decision:** Designed in the parametric editor (04-siege); cost
resources; build time in seasons grows with built volume. Ready-made templates exist
for the AI and for quick builds; the player can save their own designs as
blueprints.

### Information flow
What does the attacker know about the defender's walls before the siege (scouting,
spies, plans)?

**Decision:** Enemy fortress designs are hidden until scouted (a scout action
costs money and a turn) and fully visible when the siege scene begins.
