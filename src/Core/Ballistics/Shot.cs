using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Ballistics;

/// <summary>
/// One firing: the player-controlled inputs (charge, elevation, azimuth, shot type) plus the
/// gun's position. Azimuth 0 points along +Z; positive azimuth turns towards +X.
/// </summary>
/// <param name="Pivot">Trunnion position, m. The muzzle is one barrel length along the aim direction.</param>
/// <param name="Charge">Powder charge, kg.</param>
/// <param name="ElevationDegrees">Barrel elevation above horizontal.</param>
/// <param name="AzimuthDegrees">Barrel heading.</param>
public sealed record Shot(
    GunSpec Gun,
    PowderSpec Powder,
    ProjectileSpec Projectile,
    Vec3 Pivot,
    double Charge,
    double ElevationDegrees,
    double AzimuthDegrees)
{
    public Vec3 AimDirection
    {
        get
        {
            var elevation = ElevationDegrees * Math.PI / 180;
            var azimuth = AzimuthDegrees * Math.PI / 180;
            return new Vec3(
                Math.Cos(elevation) * Math.Sin(azimuth),
                Math.Sin(elevation),
                Math.Cos(elevation) * Math.Cos(azimuth));
        }
    }

    public ProjectileState LaunchState()
    {
        var direction = AimDirection;
        var speed = InteriorBallistics.MuzzleVelocity(Gun, Powder, Projectile, Charge);
        return new ProjectileState(Pivot + direction * Gun.BarrelLength, direction * speed, 0);
    }
}
