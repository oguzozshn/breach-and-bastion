namespace BreachAndBastion.Core.Ballistics;

/// <summary>A gunpowder type, loaded from <c>data/powders/*.json</c>.</summary>
/// <param name="SpecificEnergy">Chemical energy released per kg, J/kg.</param>
/// <param name="BurstFactor">
/// Multiplier on the gun's safe and burst charges. Corned powder burns faster, so it is
/// below 1 (07-technology).
/// </param>
public sealed record PowderSpec(string Id, string Name, double SpecificEnergy, double BurstFactor);
