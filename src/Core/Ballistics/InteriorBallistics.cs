namespace BreachAndBastion.Core.Ballistics;

/// <summary>What happens inside the barrel: powder charge to muzzle velocity, and burst risk.</summary>
public static class InteriorBallistics
{
    /// <summary>
    /// Muzzle velocity, m/s. Usable charge saturates towards <see cref="GunSpec.SaturationCharge"/>
    /// (diminishing returns); a third of the charge mass is accelerated with the shot
    /// (Lagrange gas correction).
    /// </summary>
    public static double MuzzleVelocity(GunSpec gun, PowderSpec powder, ProjectileSpec projectile, double charge)
    {
        if (charge <= 0) return 0;
        if (projectile.Diameter > gun.Bore)
            throw new ArgumentException($"Shot '{projectile.Id}' ({projectile.Diameter} m) does not fit gun '{gun.Id}' ({gun.Bore} m).");

        var usableCharge = gun.SaturationCharge * (1 - Math.Exp(-charge / gun.SaturationCharge));
        var energy = gun.Efficiency * powder.SpecificEnergy * usableCharge;
        return Math.Sqrt(2 * energy / (projectile.Mass + charge / 3));
    }

    /// <summary>
    /// Chance, 0–1, that the barrel bursts on this shot. Zero up to the safe charge, rising
    /// quadratically to one at the burst charge.
    /// </summary>
    public static double BurstProbability(GunSpec gun, PowderSpec powder, double charge)
    {
        var safe = gun.SafeCharge * powder.BurstFactor;
        var burst = gun.BurstCharge * powder.BurstFactor;
        if (charge <= safe) return 0;
        if (charge >= burst) return 1;
        var t = (charge - safe) / (burst - safe);
        return t * t;
    }
}
