# 11 — Scope and milestones

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Already stated (initial pitch)

- Solo development. Siege layer deep; map layer thin at first.
- Milestone 1 is fixed: a single siege scene — an old-style tall wall on one side,
  a low angled bastion on the other, the same cannon facing both. If the core idea is
  not fun here, the rest is not built.
- From 00-vision: not in MVP — multiplayer, naval warfare, deep trade economy,
  production chains, field battles. Never — real-time tactical unit control, city
  builder.
- From 01-setting: not in MVP — tutorial and standalone engineering-challenge
  scenarios, randomised map elements, playing any state (MVP: 3–5 curated states).

## Questions

### MVP contents
What exactly is in the MVP?

**Decision:**

- Campaign on a 20–30 province map, 3–5 playable states, turn-based seasons.
- Five resources, building slots, war / peace / alliance, ruler + heir, few events.
- Siege layer: 3 materials, 3 guns, 2 shot types, parametric fortress editor,
  attacker and defender actions (04-siege), auto-resolve.
- Save / load, basic audio, contextual hints for aiming and range tables.

### Explicitly out of MVP
What is deliberately left out of the MVP?

**Decision:** Multiplayer, naval warfare, field battles, trade and markets,
production chains, internal politics, marriage and vassalage, tutorial and challenge
scenarios, randomised maps, trenches and mining, simulated assaults, modding.

### Milestones after Milestone 1
Order and contents of later milestones.

**Decision:** M2 siege vertical slice → M3 campaign skeleton →
M4 MVP content and polish (itch.io demo) → M5 post-MVP. Details in `PLAN.md`.

### Fun test for Milestone 1
How do we judge that the first siege scene is "fun"? Who plays it, what do we measure?

**Decision:** The owner plus 3–5 playtesters. Passes when most testers:
fire 20+ shots without being asked; can explain why the tall wall fell and the bastion
held without being told; ask to try another design or gun position.

### Time budget
Hours per week available; target dates (if any).

**Decision:** _Open — see OQ-004._ Needed to put dates on the plan.

### Playtesting and public exposure
When to show the game (friends, demo, Steam page, Early Access)?

**Decision:** Private playtests from Milestone 1; public itch.io
demo after M4; Steam reconsidered after that (00-vision).

### Kill / pivot criteria
Under what conditions is a feature or the project cut or pivoted?

**Decision:** If Milestone 1 fails the fun test after two iteration rounds,
stop and pivot (e.g. a pure siege sandbox, or a different core loop) before building
the campaign.
