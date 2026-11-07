namespace BreachAndBastion.Core.Ballistics;

/// <summary>Drag coefficient of a sphere as a function of Mach number.</summary>
public abstract class DragModel
{
    public abstract double DragCoefficient(double mach);

    /// <summary>Same coefficient at every speed. Used by analytic golden tests.</summary>
    public sealed class Constant(double coefficient) : DragModel
    {
        public override double DragCoefficient(double mach) => coefficient;
    }

    /// <summary>
    /// Piecewise-linear sphere drag: ≈0.47 subsonic, rising through the transonic
    /// region to ≈0.95, then easing off at higher Mach numbers.
    /// </summary>
    public sealed class Sphere : DragModel
    {
        private static readonly (double Mach, double Cd)[] Table =
        [
            (0.0, 0.47), (0.6, 0.47), (0.8, 0.55), (1.0, 0.85), (1.2, 0.95), (2.0, 0.92), (3.0, 0.90),
        ];

        public static Sphere Instance { get; } = new();

        public override double DragCoefficient(double mach)
        {
            if (mach <= Table[0].Mach) return Table[0].Cd;
            for (var i = 1; i < Table.Length; i++)
            {
                if (mach <= Table[i].Mach)
                {
                    var (m0, c0) = Table[i - 1];
                    var (m1, c1) = Table[i];
                    return c0 + (c1 - c0) * (mach - m0) / (m1 - m0);
                }
            }
            return Table[^1].Cd;
        }
    }
}
