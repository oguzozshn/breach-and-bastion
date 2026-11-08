namespace BreachAndBastion.Core.Structure;

/// <summary>
/// Rest geometry of one wall slice: a 2D pin-jointed truss in the slice's vertical plane
/// (ADR 0004). Immutable; the moving state lives in <see cref="SliceStructure"/>.
/// </summary>
public sealed class SliceMesh
{
    public SliceMesh(
        double width,
        IReadOnlyList<MaterialSpec> materials,
        IReadOnlyList<SliceNode> nodes,
        IReadOnlyList<SliceMember> members,
        IReadOnlyList<SliceCell> cells)
    {
        Width = width;
        Materials = materials;
        Nodes = nodes;
        Members = members;
        Cells = cells;

        var membersOfNode = new List<int>[nodes.Count];
        for (var i = 0; i < nodes.Count; i++) membersOfNode[i] = [];
        for (var m = 0; m < members.Count; m++)
        {
            membersOfNode[members[m].A].Add(m);
            membersOfNode[members[m].B].Add(m);
        }
        MembersOfNode = membersOfNode;
    }

    /// <summary>Slice width along the wall, m. Masses and member areas scale with it.</summary>
    public double Width { get; }

    public IReadOnlyList<MaterialSpec> Materials { get; }
    public IReadOnlyList<SliceNode> Nodes { get; }
    public IReadOnlyList<SliceMember> Members { get; }

    /// <summary>Quads used for hit tests and rendering; each lists the members inside it.</summary>
    public IReadOnlyList<SliceCell> Cells { get; }

    public IReadOnlyList<List<int>> MembersOfNode { get; }

    public double TotalMass => Nodes.Sum(n => n.Mass);
}

/// <param name="X">Depth into the wall from the outer toe, m.</param>
/// <param name="Y">Height above the foundation, m.</param>
/// <param name="Fixed">Anchored to the ground (foundation, or the rampart continuing inwards).</param>
public sealed record SliceNode(double X, double Y, double Mass, bool Fixed);

/// <param name="Area">Effective cross-section, m².</param>
/// <param name="Material">Index into <see cref="SliceMesh.Materials"/>.</param>
public sealed record SliceMember(int A, int B, double RestLength, double Area, int Material);

/// <param name="Nodes">Corners, counter-clockwise: (x0,y0), (x1,y0), (x1,y1), (x0,y1).</param>
public sealed record SliceCell(int[] Nodes, int[] Members, int Material);
