using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Ballistics;

/// <summary>Environment a projectile flies through.</summary>
/// <param name="Gravity">Gravitational acceleration, m/s².</param>
/// <param name="AirDensity">Air density, kg/m³.</param>
/// <param name="SpeedOfSound">Speed of sound, m/s (used by Mach-dependent drag).</param>
/// <param name="Wind">Steady wind velocity, m/s.</param>
public sealed record Atmosphere(double Gravity, double AirDensity, double SpeedOfSound, Vec3 Wind)
{
    public static Atmosphere Standard { get; } = new(9.81, 1.225, 340.0, Vec3.Zero);

    public static Atmosphere Vacuum { get; } = new(9.81, 0.0, 340.0, Vec3.Zero);

    public Atmosphere WithWind(Vec3 wind) => this with { Wind = wind };
}
