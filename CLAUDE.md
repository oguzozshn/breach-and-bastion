# CLAUDE.md — Working rules

Read this file at the start of every session. It summarizes how this project is run.

## Current phase

**Pre-production: requirements interview.** No game code is written until `PLAN.md`
exists and the owner has approved it. Check `docs/requirements/README.md` for the
status of each topic and continue from the first topic that is not `Done`.

## Language

- Talk with the owner in **Turkish**.
- **Everything committed to the repo is in English**: files, docs, code, identifiers,
  comments, commit messages.
- Commit messages follow **Conventional Commits** (`docs:`, `feat:`, `fix:`, `chore:` …).
- Commits are authored as `Oguz Ozsahin <oguzozshn@gmail.com>`. Do not add any AI
  attribution (no `Co-Authored-By: Claude`, no "Generated with …").

## Phase 2 — Requirements interview rules

Topics are covered **one at a time**, in the order of `docs/requirements/` (00 → 11).

1. **Before starting a topic, read its requirements file.** The owner may have edited
   it directly. Anything the owner wrote is authoritative: do not ask it again, only
   ask about what is missing.
2. **3–5 questions per message, max.** No open-ended questions: for each, give 2–4
   options with pros/cons and a recommendation.
3. **Never decide on the owner's behalf.** If the owner is unsure, record the item in
   `docs/open-questions.md` and move on.
4. **When a topic ends:** summarize the decisions, get explicit approval, write them
   into the topic file, commit + push, then move to the next topic.
5. **Flag contradictions immediately** when a new decision conflicts with an earlier one.
6. **Flag scope growth explicitly** and ask whether the feature belongs in the MVP or
   later.
7. Significant decisions also get a short ADR in `docs/decisions/`.

## Phase 3 — Plan

When all topics are done and the owner approves, create `PLAN.md` with:

- Milestones, each with a "definition of done".
- **Milestone 1 is fixed:** a single siege scene — an old-style tall wall on one side,
  a low angled bastion on the other, the same cannon facing both. If the core idea is
  not fun in this scene, nothing else gets built.
- Per milestone: task list, risks, dependencies.
- **No game code before the plan is approved.**

## General

- When changing a file, show the owner only the changed part, not the whole file.
- Keep `docs/open-questions.md` and `docs/requirements/README.md` up to date.
