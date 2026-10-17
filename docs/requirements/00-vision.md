# 00 — Vision

**Status:** Done

## Already stated (initial pitch)

- Gunpowder-era strategy game, c. 1400–1600.
- Two layers: a grand-strategy map (CK3 / EU5 style) and a physics-based siege layer.
- Theme: cannon making tall medieval walls obsolete; the shift to low, angled star
  forts (*trace italienne*), experienced by the player in simulation.
- Inspirations: Iron Nest: Heavy Turret Simulator, Kerbal Space Program, CK3, EU5.
- Solo development. Siege layer deep, map layer thin at first.

## Questions

### One-sentence pitch
What is the game, in one sentence a stranger understands?

**Decision:** Rule a Renaissance state as its chief engineer — and discover through
real physics why the star fort replaced the castle.

### Core fantasy and player role
Who is the player — a ruler, a military engineer, a dynasty, or a mix? What should
the player *feel* they are doing?

**Decision:** Player is a ruler with a engineer background. In this world all rulers are engineers. 

### Design pillars
3–4 pillars every feature must serve (and against which features are cut).

**Decision:**

1. **Physics decides, not dice.** Every combat outcome comes from simulation.
   Cut: hidden dice rolls, abstract modifiers like "+10% assault strength".
2. **The ruler is an engineer — and every design is bound by resources.** The
   player's power comes from their designs; forts and guns are built from resources
   the player must manage. Resources are the engineer's constraint, not a goal in
   themselves. Cut: court-intrigue systems unrelated to engineering.
3. **You rediscover history.** The player finds out *by experiment* why the star fort
   beats the tall wall; the game never simply tells them.
   Cut: buttons like "Research bastions: +20% defence".
4. **Collapse is both spectacle and lesson.** Every failure should be satisfying to
   watch *and* readable — the player can see why it fell.
   Cut: destruction whose cause cannot be understood or reproduced.

**MVP:** Resource management stays minimal — 3–5 resources (e.g. money, stone,
earth, timber, powder) that forts and guns cost. Production chains (e.g. saltpetre +
sulphur + charcoal → powder) come later; trade, prices and markets are out of scope
for the MVP. Details belong to topics 03 and 05.

### Target platform(s)
PC (Windows / Linux / macOS), Steam Deck, others? Primary platform?

**Decision:** windows

### Input
Mouse + keyboard only, or controller support?

**Decision:** Mouse + Keyboard

### Play time
Length of a single siege; length of a full campaign; typical play session.

**Decision:**

- **Single siege:** 15–30 min (≈5 min preparation / 10–15 min bombardment /
  5 min resolution). Fort design happens on the map layer and is not counted.
- **Typical session:** 1–2 h, 2–4 sieges plus map decisions; save anytime.
- **Campaign:** rough target 8–15 h for the first release. Exact length depends on
  auto-resolve (topic 05) — see OQ-003.

### Target audience
Grand-strategy players, physics/engineering sandbox players, history enthusiasts —
who is primary, who is secondary?

**Decision:** Primary: physics / engineering sandbox players (KSP, Poly Bridge,
Besiege). Secondary: history enthusiasts. Grand-strategy players are targeted only
once the map layer has matured.

### Differentiation
What does this game do that its references do not?

**Decision:** Battles are decided by physics, not dice — and the player rediscovers
the military-architecture revolution with their own hands.

### Tone
Serious historical simulation, light-hearted sandbox, or somewhere in between?

**Decision:** Serious, but failure is fun (KSP tone). A collapsing wall is a lesson
and a show, never a punishment.

### Realism vs. fun
When physical/historical accuracy and fun conflict, which wins, and how far?

**Decision:** Believable physics: real equations, values tuned for play. Rule:
*relative outcomes stay true to history* (tall walls lose to cannon, low angled
bastions hold); *absolute numbers may be tuned*.

### Commercial goal
Hobby project, free release, paid Steam release, Early Access?

**Decision:** Phased. Hobby for now. If Milestone 1 proves fun → free demo on
itch.io → then reconsider Steam Early Access. Note: affects asset and music
licensing (topics 02 and 09).

### Non-goals
What the game explicitly is *not* (e.g. not a tactical battle sim, not a city builder).

**Decision:**

| Item | Status |
| --- | --- |
| Real-time tactical unit control on a battlefield (Total War style) | Never |
| City builder | Never |
| Multiplayer | Not in MVP |
| Naval warfare | Not in MVP (revisit in topic 06) |
| Deep trade economy | Not in MVP (see Design pillars → MVP) |

### War model (vision level)
How does the player attack an enemy state without field battles that break
pillar 1?

**Decision:** **War is a chain of sieges.** Armies march on the map; every province
has a fortress, and taking the province means besieging it. An enemy army joins the
fight only by coming to relieve a besieged fortress, and that relief is part of the
siege scene. No dice-resolved field battles.

**Later (not MVP):** physics-resolved field battles without unit control — the
siege engine running in open terrain, the player only deploys and watches. Revisit
after the MVP. Details belong to topics 03 and 06.
