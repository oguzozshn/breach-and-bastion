# 0001 — Requirements-first workflow

- **Status:** Accepted
- **Date:** 2026-10-15
- **Topic:** Process

## Context

Solo project with an ambitious two-layer design (grand strategy + physics siege).
The main risk is scope growth and building the wrong thing.

## Options considered

1. Prototype first, document later — fast feedback, but decisions stay implicit and
   scope drifts.
2. Requirements first, then plan, then code — slower start, but every decision is
   explicit and scope is controlled.

## Decision

Option 2. Requirements are defined topic by topic (`docs/requirements/00–11`), then a
`PLAN.md` is written and approved. No game code before the plan is approved.
Milestone 1 is a single siege scene (tall wall vs. low bastion, same cannon) that
must prove the core idea is fun.

## Consequences

- Undecided items are tracked in `docs/open-questions.md` instead of being guessed.
- The first code written will be the Milestone 1 siege scene.
