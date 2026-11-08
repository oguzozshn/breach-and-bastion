using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Mathematics;
using BreachAndBastion.Core.Structure;

namespace BreachAndBastion.Core.Siege;

/// <summary>Where a shot struck a fortress, in that slice's plane.</summary>
/// <param name="ObliquityDegrees">Plan-view angle between the shot and the face normal.</param>
/// <param name="Ricochet">True when the shot glanced off: only the normal component was transferred.</param>
/// <param name="CrushEnergy">Face-normal kinetic energy, J, that cuts the crater.</param>
/// <param name="Radius">Radius of the struck area in the slice plane, m.</param>
public sealed record StructureHit(
    Fortress Fortress,
    int Segment,
    int Slice,
    Vec3 Point,
    double Depth,
    double Height,
    double ObliquityDegrees,
    bool Ricochet,
    double ImpulseDepth,
    double ImpulseHeight,
    double CrushEnergy,
    double Radius);

public sealed record ShotResult(bool Burst, IReadOnlyList<ProjectileState> Samples, ProjectileState? Impact, StructureHit? Hit);

/// <summary>A node that fell off a slice during <see cref="SiegeSimulation.Advance"/>.</summary>
public readonly record struct RubbleEvent(Fortress Fortress, int Segment, int Slice, DetachedNode Node);

/// <summary>
/// One siege scene: fortresses, the atmosphere and a seeded random source. Everything that
/// decides an outcome happens here, deterministically (ADR 0006).
/// </summary>
public sealed class SiegeSimulation
{
    /// <summary>Plan-view obliquity above which shot glances off a face.</summary>
    public const double RicochetAngleDegrees = 65;

    /// <summary>Remaining climb, m, at which a breach counts as practicable (04-siege).</summary>
    public const double PracticableClimb = 3.0;

    private const double CollisionSpacing = 0.25;
    private readonly DeterministicRandom _random;
    private readonly List<Fortress> _fortresses;

    public SiegeSimulation(IEnumerable<Fortress> fortresses, Atmosphere atmosphere, DragModel dragModel, ulong seed)
    {
        _fortresses = fortresses.ToList();
        Ballistics = new TrajectorySimulator(atmosphere, dragModel);
        _random = new DeterministicRandom(seed);
    }

    public IReadOnlyList<Fortress> Fortresses => _fortresses;
    public TrajectorySimulator Ballistics { get; }

    /// <summary>
    /// Fires a shot. With <paramref name="applyImpact"/> false the hit is found but not yet
    /// applied, so a presentation layer can animate the flight first and call
    /// <see cref="ApplyImpact"/> when the ball arrives.
    /// </summary>
    public ShotResult Fire(Shot shot, double groundHeight = 0, double maxTime = 60, bool applyImpact = true)
    {
        var result = Trace(shot, groundHeight, maxTime);
        if (applyImpact && result.Hit is not null) ApplyImpact(result.Hit);
        return result;
    }

    /// <summary>Crushes and pushes the struck slice.</summary>
    public static void ApplyImpact(StructureHit hit)
    {
        var structure = hit.Fortress.Segments[hit.Segment].Slices[hit.Slice].Structure;
        structure.ApplyCrushing(hit.Depth, hit.Height, hit.CrushEnergy, hit.Radius);
        structure.ApplyImpulse(hit.Depth, hit.Height, hit.ImpulseDepth, hit.ImpulseHeight, hit.Radius);
    }

    private ShotResult Trace(Shot shot, double groundHeight, double maxTime)
    {
        var burstChance = InteriorBallistics.BurstProbability(shot.Gun, shot.Powder, shot.Charge);
        if (_random.NextDouble() < burstChance) return new ShotResult(true, [], null, null);

        var projectile = shot.Projectile;
        var current = shot.LaunchState();
        var samples = new List<ProjectileState> { current };
        while (current.Time < maxTime)
        {
            var next = Ballistics.Step(projectile, current);
            var travel = (next.Position - current.Position).Length;
            var parts = Math.Max(1, (int)Math.Ceiling(travel / CollisionSpacing));
            for (var k = 1; k <= parts; k++)
            {
                var t = (double)k / parts;
                var point = current.Position + (next.Position - current.Position) * t;
                var velocity = current.Velocity + (next.Velocity - current.Velocity) * t;
                var state = new ProjectileState(point, velocity, current.Time + (next.Time - current.Time) * t);

                var hit = TryHit(projectile, state);
                if (hit is not null)
                {
                    samples.Add(state);
                    return new ShotResult(false, samples, state, hit);
                }
                if (point.Y <= groundHeight && velocity.Y < 0)
                {
                    samples.Add(state);
                    return new ShotResult(false, samples, state, null);
                }
            }
            samples.Add(next);
            current = next;
        }
        return new ShotResult(false, samples, null, null);
    }

    private StructureHit? TryHit(ProjectileSpec projectile, ProjectileState state)
    {
        foreach (var fortress in _fortresses)
        {
            for (var s = 0; s < fortress.Segments.Count; s++)
            {
                var segment = fortress.Segments[s];
                var (along, depth, height) = segment.ToLocal(state.Position);
                var index = segment.SliceIndexAt(along);
                if (index < 0 || height < -0.5) continue;
                if (InFrontOfConvexNeighbour(fortress, s, state.Position)) continue;
                var structure = segment.Slices[index].Structure;
                if (structure.SolidCellAt(depth, height) < 0) continue;

                return Strike(fortress, s, index, projectile, state, depth, height);
            }
        }
        return null;
    }

    /// <summary>
    /// Slices are extruded straight inwards, so at a salient (convex) corner a segment's slab
    /// pokes out past the neighbouring face. Material there must be behind both faces.
    /// Re-entrant corners are not handled yet (none in M1).
    /// </summary>
    private static bool InFrontOfConvexNeighbour(Fortress fortress, int s, Vec3 point)
    {
        var segment = fortress.Segments[s];
        foreach (var n in new[] { s - 1, s + 1 })
        {
            if (n < 0 || n >= fortress.Segments.Count) continue;
            var neighbour = fortress.Segments[n];
            var (first, second) = n < s ? (neighbour, segment) : (segment, neighbour);
            var convex = Vec3.Dot(second.Direction, first.Outward) < 0;
            if (convex && neighbour.ToLocal(point).Depth < 0) return true;
        }
        return false;
    }

    /// <summary>
    /// Splits the shot's momentum: the part in the slice plane is transferred, the part along
    /// the wall is carried away. Above the ricochet angle only the face-normal part is.
    /// Computes the hit only; <see cref="ApplyImpact"/> applies it.
    /// </summary>
    private static StructureHit Strike(Fortress fortress, int s, int index, ProjectileSpec projectile, ProjectileState state, double depth, double height)
    {
        var segment = fortress.Segments[s];
        var velocity = state.Velocity;
        var inward = Vec3.Dot(velocity, -segment.Outward);
        var horizontal = new Vec3(velocity.X, 0, velocity.Z);
        var cosine = horizontal.Length > 0 ? Math.Abs(Vec3.Dot(horizontal.Normalized(), segment.Outward)) : 1;
        var obliquity = Math.Acos(Math.Clamp(cosine, 0, 1)) * 180 / Math.PI;
        var ricochet = obliquity > RicochetAngleDegrees;

        var impulseDepth = projectile.Mass * inward;
        var impulseHeight = ricochet ? 0 : projectile.Mass * velocity.Y;
        var radius = Math.Max(2 * projectile.Diameter, 0.6);
        var crushEnergy = 0.5 * projectile.Mass * inward * inward;

        return new StructureHit(fortress, s, index, state.Position, depth, height, obliquity, ricochet,
            impulseDepth, impulseHeight, crushEnergy, radius);
    }

    /// <summary>Advances every awake slice and turns fallen nodes into rubble.</summary>
    public IReadOnlyList<RubbleEvent> Advance(double seconds)
    {
        var events = new List<RubbleEvent>();
        foreach (var fortress in _fortresses)
        {
            for (var s = 0; s < fortress.Segments.Count; s++)
            {
                var segment = fortress.Segments[s];
                for (var i = 0; i < segment.Slices.Count; i++)
                {
                    var slice = segment.Slices[i];
                    foreach (var node in slice.Structure.Advance(seconds))
                    {
                        events.Add(new RubbleEvent(fortress, s, i, node));
                        // Material from the outer half of the revetment falls outwards; the rest falls inside.
                        if (node.X < segment.Profile.RevetmentBaseThickness / 2)
                            slice.Rubble.Deposit(node.Mass / (NodeDensity(segment.Mesh, node.Node) * segment.SliceWidth), -node.X);
                    }
                }
            }
        }
        return events;
    }

    private static double NodeDensity(SliceMesh mesh, int node)
    {
        var members = mesh.MembersOfNode[node];
        return members.Count == 0 ? 2000 : mesh.Materials[mesh.Members[members[0]].Material].Density;
    }

    /// <summary>A breach is practicable when the climb from the rubble to the crest is short enough.</summary>
    public static bool IsPracticable(Slice slice) =>
        slice.Structure.CrestHeight() - slice.Rubble.HeightAtFace <= PracticableClimb;

    public int AwakeMemberCount() =>
        _fortresses.SelectMany(f => f.Segments).SelectMany(s => s.Slices)
            .Where(s => s.Structure.Awake).Sum(s => s.Structure.Mesh.Members.Count);

    public bool AnyAwake() => _fortresses.SelectMany(f => f.Segments).SelectMany(s => s.Slices).Any(s => s.Structure.Awake);

    public ulong StateHash()
    {
        var hash = 14695981039346656037UL;
        foreach (var fortress in _fortresses) hash = fortress.StateHash(hash);
        return hash;
    }
}
