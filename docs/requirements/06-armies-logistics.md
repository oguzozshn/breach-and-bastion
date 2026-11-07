# 06 — Armies and logistics

**Status:** Done

_Decided by Claude at the owner's request (2026-11-07, see ADR 0007). The owner may override any item._

## Already stated (from earlier topics)

- From 00-vision: war is a chain of sieges; enemy armies fight by relieving a
  besieged fortress, inside the siege scene (ADR 0002).
- From 00-vision: naval warfare is not in the MVP.
- From 03-strategic-map: armies are tokens moving between adjacent provinces, one
  season per turn; seasons matter for campaigning. Resources: money, stone, timber,
  powder (incl. shot), labour.

## Questions

### Unit types
Which unit types exist (e.g. pike, arquebus, cavalry, artillery train, sappers / pioneers)?

**Decision:** **MVP: infantry, artillery train, pioneers.** **Later:** cavalry,
specialised troops.

### Army representation
Abstract strength numbers, regiments, or individual companies?

**Decision:** **An army token with numbers:** men, guns (list), powder
carried, pioneers.

### Recruitment
Levies, mercenaries, standing armies? Costs and limits?

**Decision:** **MVP:** men cost money to raise and upkeep. **Later:** mercenaries.

### Supply model
How are armies fed and supplied? Foraging, supply lines, depots?

**Decision:** **MVP:** upkeep in money every turn; outside own territory the army
suffers attrition each season, higher in winter. **Later:** supply lines and depots.

### Artillery train
Moving heavy guns: oxen, roads, rivers, speed penalties?

**Decision:** Army speed equals its slowest gun; heavy bombards slow the army
(e.g. one province per two turns).

### Gunpowder supply
Production, storage, transport and consumption of powder and shot.

**Decision:** Produced by powder mills, stored as a resource; an army carries a
fixed amount and resupplies next to an own province; each shot consumes powder.

### Long sieges
Starvation, disease, morale, desertion, weather / seasons for both sides?

**Decision:** A siege running over several turns causes attacker attrition each
season; the garrison's supplies run down and it surrenders when empty. Disease can
strike via events.

### Relief armies
Can a relief army break a siege? How is that resolved?

**Decision:** **MVP:** a relief army arriving forces the attacker to lift the siege.
**Later:** relief battle inside the siege scene (ADR 0002).

### Commanders
Do commanders / engineers lead armies and affect outcomes?

**Decision:** **MVP:** the ruler or heir may lead an army, with no stats.
**Later:** commanders with effects grounded in the simulation.

### Naval
Any naval element (coastal forts, ship-borne guns, blockades)?

**Decision:** Not in the MVP (00-vision).
