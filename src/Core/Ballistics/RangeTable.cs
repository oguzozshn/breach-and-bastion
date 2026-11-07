using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Ballistics;

/// <summary>One line of a range table: where a shot lands on flat ground in still air.</summary>
public sealed record RangeTableEntry(double ElevationDegrees, double Range, double FlightTime, double ImpactSpeed);

/// <summary>
/// The gunner's range table (02-visual-style: no trajectory preview; the player learns from
/// tables and ranging shots). Computed in still air, so wind is the player's job.
/// </summary>
public static class RangeTable
{
    public static IReadOnlyList<RangeTableEntry> Build(
        GunSpec gun, PowderSpec powder, ProjectileSpec projectile, double charge,
        Atmosphere atmosphere, DragModel dragModel, double stepDegrees = 1.0)
    {
        var simulator = new TrajectorySimulator(atmosphere.WithWind(Vec3.Zero), dragModel);
        var entries = new List<RangeTableEntry>();
        var steps = (int)Math.Floor((gun.MaxElevation - stepDegrees) / stepDegrees + 1e-9);
        for (var i = 0; i <= steps; i++)
        {
            var elevation = stepDegrees * (i + 1);
            if (elevation < gun.MinElevation) continue;
            var shot = new Shot(gun, powder, projectile, Vec3.Zero, charge, elevation, 0);
            var impact = simulator.Fire(shot).Impact;
            var range = Math.Sqrt(impact.Position.X * impact.Position.X + impact.Position.Z * impact.Position.Z);
            entries.Add(new RangeTableEntry(elevation, range, impact.Time, impact.Velocity.Length));
        }
        return entries;
    }
}
