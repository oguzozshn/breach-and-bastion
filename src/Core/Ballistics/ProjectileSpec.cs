namespace BreachAndBastion.Core.Ballistics;

/// <summary>A spherical shot, loaded from <c>data/projectiles/*.json</c>.</summary>
/// <param name="Id">Stable identifier, e.g. <c>iron-ball</c>.</param>
/// <param name="Diameter">Ball diameter, m.</param>
/// <param name="Density">Material density, kg/m³ (iron ≈ 7870, stone ≈ 2650).</param>
public sealed record ProjectileSpec(string Id, string Name, double Diameter, double Density)
{
    public double Radius => Diameter / 2;

    public double CrossSectionArea => Math.PI * Radius * Radius;

    public double Mass => Density * (4.0 / 3.0) * Math.PI * Radius * Radius * Radius;
}
