# Breach and Bastion

> Working title.

A strategy game set in the gunpowder era (c. 1400–1600) with two layers:

- **Strategic layer** — a grand-strategy map in the spirit of *Crusader Kings 3* and
  *Europa Universalis 5*: provinces, economy, diplomacy, characters and dynasties.
- **Siege layer** — physics-based siege warfare. Battles are resolved by simulation,
  not dice. As the attacker you place and fire your guns (wind, air drag, powder
  charge, elevation). As the defender you design your walls. Structures fail under
  real loads: a truss model of nodes and members, where any member pushed past its
  limit breaks.

**Theme:** cannon made the tall medieval curtain wall obsolete and drove the shift to
low, angled star forts (*trace italienne*). The player lives through that change
inside the simulation.

**Scope principle:** solo development. The siege layer goes deep; the map layer
starts thin.

## Status

Pre-production. Requirements are being defined; no game code yet.

## Repository layout

| Path | Contents |
| --- | --- |
| `docs/requirements/` | One file per topic, filled in during the requirements interview |
| `docs/decisions/` | Architecture/design decision records (ADRs) |
| `docs/open-questions.md` | Everything not yet decided |
| `docs/references.md` | Inspirations, visual references and sources |
| `CLAUDE.md` | Working rules for AI-assisted sessions |
