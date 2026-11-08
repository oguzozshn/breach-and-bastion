using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Ballistics;

/// <summary>
/// Aiming by trial trajectories on the real ballistics model. The siege AI uses it
/// (08-ai: it "knows" its range table); so do tests and tools.
/// </summary>
public static class Gunnery
{
    public static double AzimuthTowards(Vec3 from, Vec3 to) =>
        Math.Atan2(to.X - from.X, to.Z - from.Z) * 180 / Math.PI;

    /// <summary>
    /// Elevation, degrees, at which <paramref name="shot"/> (its elevation and azimuth are
    /// replaced) passes through <paramref name="target"/>, ignoring obstacles. Returns null if
    /// the target is out of reach within the gun's elevation limits.
    /// </summary>
    public static double? ElevationFor(TrajectorySimulator simulator, Shot shot, Vec3 target)
    {
        var azimuth = AzimuthTowards(shot.Pivot, target);
        var distance = Math.Sqrt((target.X - shot.Pivot.X) * (target.X - shot.Pivot.X) + (target.Z - shot.Pivot.Z) * (target.Z - shot.Pivot.Z));

        double? HeightAt(double elevation)
        {
            var aimed = shot with { ElevationDegrees = elevation, AzimuthDegrees = azimuth };
            var state = aimed.LaunchState();
            while (state.Time < 60)
            {
                var next = simulator.Step(aimed.Projectile, state);
                var d0 = HorizontalDistance(shot.Pivot, state.Position);
                var d1 = HorizontalDistance(shot.Pivot, next.Position);
                if (d1 >= distance)
                {
                    var t = (distance - d0) / (d1 - d0);
                    return state.Position.Y + (next.Position.Y - state.Position.Y) * t;
                }
                if (next.Position.Y < target.Y - 200) return null;
                state = next;
            }
            return null;
        }

        double low = shot.Gun.MinElevation, high = shot.Gun.MaxElevation;
        var highHeight = HeightAt(high);
        if (highHeight is null || highHeight < target.Y) return null;
        var lowHeight = HeightAt(low);
        if (lowHeight is not null && lowHeight > target.Y) return null;

        for (var i = 0; i < 50; i++)
        {
            var mid = (low + high) / 2;
            var height = HeightAt(mid);
            if (height is not null && height >= target.Y) high = mid; else low = mid;
        }
        return high;
    }

    private static double HorizontalDistance(Vec3 a, Vec3 b) =>
        Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z));
}
