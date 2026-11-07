using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Ballistics;

/// <summary>Position (m), velocity (m/s) and flight time (s) of a projectile.</summary>
public readonly record struct ProjectileState(Vec3 Position, Vec3 Velocity, double Time);
