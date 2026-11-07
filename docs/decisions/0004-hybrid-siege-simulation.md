# 0004 — Hybrid (2.5D) siege simulation

- **Status:** Accepted
- **Date:** 2026-11-07
- **Topic:** 04 — Siege layer (proposed in 02 — Visual style)

## Context

The star fort's advantage shows in two views: the **profile** (low, thick,
earth-backed walls absorb shot) and the **plan** (angled faces deflect shot and leave
no dead ground). A pure 2D side view only shows the profile; full 3D structural
simulation is too heavy for a solo developer and for 60 FPS.

## Options considered

1. 2D cross-section only — cheapest; loses the angled bastion.
2. Full 3D truss — complete; very expensive in art, solver work and performance.
3. Hybrid — 3D scene; each wall segment cut into slices, each slice a 2D
   cross-section truss; plan-view obliquity splits each hit into normal and
   tangential parts.

## Decision

Option 3. Fortresses are a plan-view trace plus parametric profiles. Each segment is
sliced (≈2 m); slices are independent 2D pin-jointed trusses with explicit dynamics at
a fixed timestep. Impacts use plan-view obliquity (glancing blows and ricochets).

## Consequences

- Angled faces matter physically, which the core theme requires.
- Collapse across slice boundaries is approximate; weak slice coupling may be added.
- The parametric editor and AI templates map directly to this model.
- Milestone 1 must validate both the look and the performance of the approach.
