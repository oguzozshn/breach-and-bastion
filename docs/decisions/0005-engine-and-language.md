# 0005 — Godot 4 (.NET) with C#, engine-independent simulation core

- **Status:** Accepted
- **Date:** 2026-11-07
- **Topic:** 10 — Technical foundation

## Context

The game's value is in a custom structural and ballistic simulation that must be
fast, deterministic, testable and runnable headless (auto-resolve, tests).

## Options considered

1. Godot 4 + GDScript — fastest iteration; too slow for a truss solver with
   thousands of members; weak tooling for unit tests.
2. Godot 4 (.NET) + C# — fast enough, strong typing, standard test tooling
   (xUnit), the core can live in a plain .NET library.
3. GDExtension (C++ / Rust) core — fastest; much slower iteration for a solo
   developer.

## Decision

Option 2. Godot 4 .NET build, latest stable at setup, pinned in the repo. All
outcome-relevant code (ballistics, structure, rubble, siege rules) lives in a pure C#
`Core` library with no Godot references, tested with xUnit and runnable headless.
Godot physics is used only for visual debris. Data lives in JSON files.

## Consequences

- No web export (Godot .NET limitation) — acceptable, the target is Windows.
- Determinism is under our control: fixed timesteps, seeded RNG.
- A hot path can still move to GDExtension later if profiling demands it.
