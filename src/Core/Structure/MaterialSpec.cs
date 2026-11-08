namespace BreachAndBastion.Core.Structure;

/// <summary>A building material, loaded from <c>data/materials/*.json</c>. Values are tuned for play.</summary>
/// <param name="Density">kg/m³.</param>
/// <param name="Stiffness">Young's modulus, Pa.</param>
/// <param name="TensileStrength">Stress, Pa, at which an undamaged member breaks in tension.</param>
/// <param name="CompressiveStrength">Stress, Pa, at which an undamaged member breaks in compression.</param>
/// <param name="DampingRatio">Axial dashpot damping as a fraction of critical damping.</param>
/// <param name="DamageOnset">Stress ratio (stress / strength) above which damage accumulates.</param>
/// <param name="DamageRate">Damage per second per unit of stress ratio above the onset.</param>
/// <param name="CrushEnergy">
/// Impact energy, J/m³, that pulverises a volume of the material: how a shot cuts its
/// crater, independent of the elastic stress wave.
/// </param>
public sealed record MaterialSpec(
    string Id,
    string Name,
    double Density,
    double Stiffness,
    double TensileStrength,
    double CompressiveStrength,
    double DampingRatio,
    double DamageOnset,
    double DamageRate,
    double CrushEnergy);
