namespace BreachAndBastion.Core.Ballistics;

/// <summary>A gun type, loaded from <c>data/guns/*.json</c>.</summary>
/// <param name="Bore">Bore diameter, m. Shot must not be larger.</param>
/// <param name="BarrelLength">Barrel length, m. The muzzle sits this far along the aim direction.</param>
/// <param name="SaturationCharge">
/// Charge mass, kg, around which extra powder stops paying off (the barrel is too short
/// to burn it usefully).
/// </param>
/// <param name="Efficiency">Fraction of the powder's chemical energy that reaches the shot.</param>
/// <param name="SafeCharge">Largest charge, kg, with no burst risk.</param>
/// <param name="BurstCharge">Charge, kg, at which the barrel always bursts.</param>
/// <param name="MaxElevation">Highest elevation the carriage allows, degrees.</param>
public sealed record GunSpec(
    string Id,
    string Name,
    double Bore,
    double BarrelLength,
    double SaturationCharge,
    double Efficiency,
    double SafeCharge,
    double BurstCharge,
    double MinElevation,
    double MaxElevation);
