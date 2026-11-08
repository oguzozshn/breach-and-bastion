using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Structure;

/// <summary>One slice of a segment with its rubble.</summary>
public sealed class Slice(SliceStructure structure, double start, double end)
{
    public SliceStructure Structure { get; } = structure;
    public RubblePile Rubble { get; } = new();

    /// <summary>Position along the segment, m.</summary>
    public double Start { get; } = start;
    public double End { get; } = end;
}

/// <summary>
/// A straight wall segment cut into slices. Local frame: <see cref="Direction"/> along the
/// wall, <see cref="Outward"/> towards the attacker, Y up. Slice x = depth inwards from the
/// outer toe line A→B.
/// </summary>
public sealed class FortressSegment
{
    internal FortressSegment(Vec3 a, Vec3 b, ProfileSpec profile, SliceMesh mesh, SliceStructure settled, double sliceWidth)
    {
        A = a;
        B = b;
        Profile = profile;
        Mesh = mesh;
        Length = (b - a).Length;
        Direction = (b - a) / Length;
        Outward = new Vec3(Direction.Z, 0, -Direction.X);

        var count = Math.Max(1, (int)Math.Round(Length / sliceWidth));
        var width = Length / count;
        var slices = new Slice[count];
        for (var i = 0; i < count; i++)
        {
            var structure = new SliceStructure(mesh);
            structure.CopyStateFrom(settled);
            slices[i] = new Slice(structure, i * width, (i + 1) * width);
        }
        Slices = slices;
        SliceWidth = width;
    }

    public Vec3 A { get; }
    public Vec3 B { get; }
    public Vec3 Direction { get; }
    public Vec3 Outward { get; }
    public double Length { get; }
    public double SliceWidth { get; }
    public ProfileSpec Profile { get; }
    public SliceMesh Mesh { get; }
    public IReadOnlyList<Slice> Slices { get; }

    /// <summary>World point → (along, depth, height) in this segment's frame.</summary>
    public (double Along, double Depth, double Height) ToLocal(Vec3 point)
    {
        var relative = point - A;
        return (Vec3.Dot(relative, Direction), -Vec3.Dot(relative, Outward), point.Y - A.Y);
    }

    /// <summary>Slice-plane point of slice <paramref name="index"/> → world point.</summary>
    public Vec3 ToWorld(int index, double depth, double height)
    {
        var slice = Slices[index];
        var along = (slice.Start + slice.End) / 2;
        return A + Direction * along - Outward * depth + Vec3.Up * height;
    }

    public int SliceIndexAt(double along) =>
        along < 0 || along > Length ? -1 : Math.Min(Slices.Count - 1, (int)(along / SliceWidth));
}

/// <summary>A built fortress: segments with settled slices.</summary>
public sealed class Fortress
{
    private Fortress(FortressSpec spec, IReadOnlyList<FortressSegment> segments)
    {
        Spec = spec;
        Segments = segments;
    }

    public FortressSpec Spec { get; }
    public IReadOnlyList<FortressSegment> Segments { get; }

    /// <summary>
    /// Builds the fortress. Each distinct (profile, slice width) is meshed and settled once;
    /// every slice starts from a copy of that settled prototype.
    /// </summary>
    public static Fortress Build(
        FortressSpec spec,
        IReadOnlyDictionary<string, ProfileSpec> profiles,
        IReadOnlyDictionary<string, MaterialSpec> materials,
        double groundHeight = 0)
    {
        var prototypes = new Dictionary<(string, double), (SliceMesh Mesh, SliceStructure Settled)>();
        var segments = new List<FortressSegment>();
        for (var s = 0; s < spec.SegmentCount; s++)
        {
            var a = new Vec3(spec.Trace[s][0], groundHeight, spec.Trace[s][1]);
            var b = new Vec3(spec.Trace[s + 1][0], groundHeight, spec.Trace[s + 1][1]);
            var length = (b - a).Length;
            var width = length / Math.Max(1, (int)Math.Round(length / spec.SliceWidth));
            var profile = profiles[spec.ProfileOf(s)];

            var key = (profile.Id, Math.Round(width, 6));
            if (!prototypes.TryGetValue(key, out var prototype))
            {
                var mesh = SliceMeshBuilder.Build(profile, materials, width);
                var settled = new SliceStructure(mesh);
                settled.Settle();
                prototypes[key] = prototype = (mesh, settled);
            }
            segments.Add(new FortressSegment(a, b, profile, prototype.Mesh, prototype.Settled, spec.SliceWidth));
        }
        return new Fortress(spec, segments);
    }

    public ulong StateHash(ulong hash = 14695981039346656037UL)
    {
        foreach (var segment in Segments)
            foreach (var slice in segment.Slices)
                hash = slice.Structure.StateHash(hash);
        return hash;
    }
}
