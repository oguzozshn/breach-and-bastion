namespace BreachAndBastion.Core.Structure;

/// <summary>
/// A fortress as a plan-view trace plus one profile per segment, loaded from
/// <c>data/fortresses/*.json</c>. Trace points are (x, z) on the ground, m.
/// </summary>
/// <param name="Trace">Polyline points. Each segment's outward normal is (dz, 0, −dx) of its direction.</param>
/// <param name="Profiles">One profile id per segment, or a single id used for all segments.</param>
/// <param name="SliceWidth">Target slice width, m (ADR 0004).</param>
public sealed record FortressSpec(string Id, string Name, double[][] Trace, string[] Profiles, double SliceWidth = 2.0)
{
    public int SegmentCount => Trace.Length - 1;

    public string ProfileOf(int segment) => Profiles.Length == 1 ? Profiles[0] : Profiles[segment];
}
