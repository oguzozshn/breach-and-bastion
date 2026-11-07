# 09 — Sound and music

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Questions

### Music style
Period-authentic Renaissance music, modern orchestral, ambient, minimal?

**Decision:** **Renaissance-flavoured, calm and sparse;** quieter on the map, more
tension in sieges.

### Music source
Self-composed, licensed / royalty-free, commissioned, public-domain period pieces?

**Decision:** **Public-domain compositions in recordings with permissive licences
(CC0 / CC-BY).** Commissioned music only if the game goes commercial.

### Adaptive music
Does music change with game state (peace, war, siege, collapse)?

**Decision:** **MVP:** separate tracks for map and siege only.

### Sound effects priorities
Which sounds matter most (gun report, impact, collapse, ambience, UI)?

**Decision:** 1. Gun report 2. Impact 3. Collapse rumble 4. Ambience
5. UI (quill, paper).

### Physically driven sound
Does impact sound depend on material, energy, distance?

**Decision:** **Yes, simply:** the impact sample is chosen by material and
scaled by impact energy and distance.

### Voice
Any voice acting or barks? Which language(s)?

**Decision:** **None.**

### Budget and licensing
Budget for audio; licensing constraints.

**Decision:** **Zero budget;** CC0 / CC-BY only, attributions kept in a
credits file.
