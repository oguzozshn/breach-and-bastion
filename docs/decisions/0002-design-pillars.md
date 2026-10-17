# 0002 — Design pillars and war model

- **Status:** Accepted — pillar 3 renamed by [0003](0003-fictional-world.md)
- **Date:** 2026-10-17
- **Topic:** 00 — Vision

## Context

A solo project with two layers needs a short, fixed test that every feature must
pass, so scope can be cut without re-arguing the vision each time.

## Options considered

1. No explicit pillars — decide case by case. Fast, but scope drifts.
2. 3–4 pillars, with resource management as a separate fifth pillar.
3. 4 pillars, with resource management folded into "the ruler is an engineer".

## Decision

Option 3. The pillars are:

1. **Physics decides, not dice.**
2. **The ruler is an engineer — and every design is bound by resources.**
3. **You rediscover history.**
4. **Collapse is both spectacle and lesson.**

As a direct consequence of pillar 1, **war is a chain of sieges**: there are no
dice-resolved field battles. Enemy armies fight by relieving a besieged fortress,
inside the siege scene. Physics-resolved field battles without unit control may be
revisited after the MVP.

## Consequences

- Any feature that serves none of the pillars is cut or deferred.
- Resources are a constraint on design, not a goal: MVP keeps 3–5 resources.
- The map layer needs no field-battle resolver in the MVP; the siege layer must
  model relief armies.
- Real-time tactical unit control (Total War style) is a permanent non-goal.
