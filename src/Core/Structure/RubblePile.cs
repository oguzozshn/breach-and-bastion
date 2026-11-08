namespace BreachAndBastion.Core.Structure;

/// <summary>
/// Deterministic rubble at the outer foot of one slice (ADR 0006): a height profile over the
/// distance in front of the wall, relaxed to the angle of repose. This, not the visual
/// debris, builds the breach ramp.
/// </summary>
public sealed class RubblePile
{
    public const double BinWidth = 0.5;
    public const double ReposeAngleDegrees = 35;

    /// <summary>Loose rubble takes more room than the wall it came from.</summary>
    public const double Bulking = 1.4;

    private readonly double[] _height;
    private readonly double _maxStep = Math.Tan(ReposeAngleDegrees * Math.PI / 180) * BinWidth;

    public RubblePile(double length = 20) => _height = new double[(int)Math.Ceiling(length / BinWidth)];

    public int BinCount => _height.Length;

    /// <summary>Rubble height, m, in bin <paramref name="bin"/> (bin 0 touches the wall face).</summary>
    public double Height(int bin) => _height[bin];

    public double HeightAtFace => _height[0];

    /// <summary>Cross-section area of the pile, m² (volume per metre of wall).</summary>
    public double Area => _height.Sum() * BinWidth;

    /// <summary>
    /// Adds fallen material. <paramref name="area"/> is its cross-section in m² (volume divided
    /// by slice width) before bulking; <paramref name="distance"/> is how far in front of the
    /// face it lands, m.
    /// </summary>
    public void Deposit(double area, double distance)
    {
        var bin = Math.Clamp((int)(Math.Max(0, distance) / BinWidth), 0, _height.Length - 1);
        _height[bin] += area * Bulking / BinWidth;
        Relax();
    }

    /// <summary>Slides material outwards until no step is steeper than the angle of repose.</summary>
    private void Relax()
    {
        for (var iteration = 0; iteration < 10_000; iteration++)
        {
            var moved = false;
            for (var k = 0; k < _height.Length - 1; k++)
            {
                var excess = _height[k] - _height[k + 1] - _maxStep;
                if (excess <= 1e-9) continue;
                var transfer = excess / 2;
                _height[k] -= transfer;
                _height[k + 1] += transfer;
                moved = true;
            }
            if (!moved) return;
        }
    }
}
