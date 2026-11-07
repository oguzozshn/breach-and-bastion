using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Ballistics;

/// <summary>A computed flight: sampled states and the final impact.</summary>
public sealed record Trajectory(IReadOnlyList<ProjectileState> Samples, ProjectileState Impact, bool HitGround);

/// <summary>
/// Exterior ballistics: gravity plus quadratic air drag relative to the wind, integrated with
/// fixed-step RK4 (deterministic, ADR 0006).
/// </summary>
public sealed class TrajectorySimulator(Atmosphere atmosphere, DragModel dragModel, double timeStep = 1.0 / 120.0)
{
    public Atmosphere Atmosphere { get; } = atmosphere;
    public double TimeStep { get; } = timeStep;

    public Vec3 Acceleration(ProjectileSpec projectile, Vec3 velocity)
    {
        var gravity = new Vec3(0, -Atmosphere.Gravity, 0);
        if (Atmosphere.AirDensity <= 0) return gravity;

        var relative = velocity - Atmosphere.Wind;
        var speed = relative.Length;
        if (speed == 0) return gravity;

        var cd = dragModel.DragCoefficient(speed / Atmosphere.SpeedOfSound);
        var k = 0.5 * Atmosphere.AirDensity * cd * projectile.CrossSectionArea / projectile.Mass;
        return gravity - relative * (k * speed);
    }

    public ProjectileState Step(ProjectileSpec projectile, ProjectileState s)
    {
        var dt = TimeStep;
        var k1v = Acceleration(projectile, s.Velocity);
        var k1x = s.Velocity;
        var k2v = Acceleration(projectile, s.Velocity + k1v * (dt / 2));
        var k2x = s.Velocity + k1v * (dt / 2);
        var k3v = Acceleration(projectile, s.Velocity + k2v * (dt / 2));
        var k3x = s.Velocity + k2v * (dt / 2);
        var k4v = Acceleration(projectile, s.Velocity + k3v * dt);
        var k4x = s.Velocity + k3v * dt;

        return new ProjectileState(
            s.Position + (k1x + 2 * k2x + 2 * k3x + k4x) * (dt / 6),
            s.Velocity + (k1v + 2 * k2v + 2 * k3v + k4v) * (dt / 6),
            s.Time + dt);
    }

    /// <summary>
    /// Flies until the shot drops below <paramref name="groundHeight"/> or
    /// <paramref name="maxTime"/> passes. The impact is interpolated between the last two steps.
    /// </summary>
    public Trajectory FlyUntilGround(ProjectileSpec projectile, ProjectileState launch, double groundHeight = 0, double maxTime = 120)
    {
        var samples = new List<ProjectileState> { launch };
        var current = launch;
        while (current.Time < maxTime)
        {
            var next = Step(projectile, current);
            samples.Add(next);
            if (next.Position.Y <= groundHeight && next.Velocity.Y < 0)
            {
                var t = (current.Position.Y - groundHeight) / (current.Position.Y - next.Position.Y);
                var impact = new ProjectileState(
                    current.Position + (next.Position - current.Position) * t,
                    current.Velocity + (next.Velocity - current.Velocity) * t,
                    current.Time + (next.Time - current.Time) * t);
                return new Trajectory(samples, impact, HitGround: true);
            }
            current = next;
        }
        return new Trajectory(samples, current, HitGround: false);
    }

    public Trajectory Fire(Shot shot, double groundHeight = 0) =>
        FlyUntilGround(shot.Projectile, shot.LaunchState(), groundHeight);
}
