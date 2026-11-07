# 0006 — Deterministic outcomes, visual-only engine physics

- **Status:** Accepted
- **Date:** 2026-11-07
- **Topic:** 04 — Siege layer, 10 — Technical foundation

## Context

02-visual-style decided that large debris is simulated and forms the breach ramp.
Engine rigid-body physics is not guaranteed to be deterministic, but auto-resolve,
replays and tests need identical results for identical inputs.

## Decision

Every outcome-relevant quantity comes from the deterministic Core: broken members
deposit their mass into a core rubble model that builds the breach ramp and decides
practicability. Engine-physics debris chunks are visual only. Assaults are resolved
deterministically from simulated quantities, without dice.

## Consequences

- Visible debris and the core rubble pile can differ slightly; the visuals must stay
  close enough to read correctly (pillar 4).
- A determinism test (state hash after a scripted siege) guards this in CI.
