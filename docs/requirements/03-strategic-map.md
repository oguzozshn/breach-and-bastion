# 03 — Strategic map layer

**Status:** Done

## Already stated (initial pitch)

- CK3 / EU5-style map: provinces, economy, diplomacy, characters / dynasty.
- Starts thin; the siege layer is the deep one.
- From 00-vision: war is a chain of sieges — every province has a fortress; no
  dice-resolved field battles (ADR 0002).
- From 00-vision: MVP economy has 3–5 resources that forts and guns cost; no
  production chains, trade or markets in the MVP.
- From 01-setting: one hand-crafted fictional map inspired by Renaissance Italy;
  3–5 curated playable states in the MVP; cultures and religions are flavour only.
- From 02-visual-style: 2D ink-on-parchment map, flat top-down camera with 2–3 zoom
  levels and map modes; ink-sketch ruler portraits.

## Questions

### Number of provinces
Rough count for the MVP and for the full game.

**Decision:** **MVP: 20–30 provinces. Later: 60–100.** Every province has a
fortress, so each needs a design (templates or AI — topics 05 and 08).

### Time flow
Turn-based or pausable real-time? Time unit (day / week / month / season)?

**Decision:** **Turn-based, one season per turn** (4 turns per year). Seasons
matter for campaigning (e.g. winter sieges — topic 06).

### Economy model
Sources of income, expenses, how detailed (single treasury vs. multiple goods)?

**Decision:** **Single money treasury plus stockpiled physical resources.**
Provinces produce resources every turn. No prices, no markets.

### Resources and goods
Which resources exist (e.g. saltpetre, sulphur, charcoal, copper, tin, iron, stone,
timber, labour, money)? Which matter for sieges?

**Decision:** **MVP: money, stone, timber, powder, labour.**

- Powder also covers shot.
- Labour makes earthworks possible: earth is free but costs labour, creating a real
  trade-off between stone walls and earthen ramparts.

### Province / city development
How do provinces and cities grow: buildings (workshops, foundries, quarries,
markets), population, upgrading fortifications? How deep can it go without becoming
a city builder (a non-goal in 00-vision)?

**Decision:** **Building slots.** Each province has a few building slots;
MVP has 4–6 building types (e.g. quarry, timber yard, powder mill, foundry). No city
layout or street planning — that would be a city builder (non-goal).

### Diplomacy
Which actions (war, peace, alliance, marriage, vassalage, tribute)? How deep?

**Decision:** **MVP: war, peace, alliance.** Declaring war, peace treaties
(ceding provinces or paying money) and alliances. **Later:** marriage, vassalage.

### Characters and dynasty
Do characters exist? Rulers only, or also heirs, generals, engineers? Lifespan, traits,
succession?

**Decision:** **Ruler + heir.** The ruler ages and dies; the heir succeeds.
Dynasties carry a scientist's name (01-setting). No traits that give abstract bonuses
(pillar 1). Engineer characters are discussed in topic 07.

### Internal politics
Nobles, estates, rebellions, legitimacy — any of these?

**Decision:** **None in the MVP.** Pillar 2 cuts court systems unrelated to
engineering. **Later (idea):** simple unrest leading to revolts, so the player must
besiege their own fortress.

### War on the map
How are wars declared, fought and ended? Army movement on the map?

**Decision:** **Army tokens, province by province.** Armies are tokens that
move between adjacent provinces each turn. An army entering an enemy province with a
fortress starts a siege. Wars end with a peace treaty weighted by captured fortresses.

### Field battles
Field battles (not sieges): simulated, auto-resolved, or absent?

**Decision:** Decided in 00-vision / ADR 0002: no dice-resolved field battles.
Enemy armies fight by relieving a besieged fortress, inside the siege scene.

### Events
Scripted / random events system? How much narrative?

**Decision:** **A small set of random events** tied to engineering and the
economy (e.g. quarry collapse, powder-mill explosion, disease in a siege camp).
Events never decide a combat outcome (pillar 1).

### Fog of war and map modes
Information limits; which map modes (political, terrain, fortification, economy)?

**Decision:** **Own, neighbouring and allied provinces are visible.** Enemy
fortress designs stay hidden until scouted (details in topic 05).
Map modes: political, resources, fortification.

### Victory and defeat
Win conditions, loss conditions, or open sandbox?

**Decision:** **Goal, then free play.** Defeat: losing every province or the
dynasty dying out. Victory: regional hegemony (e.g. holding most provinces). Play can
continue after victory.
