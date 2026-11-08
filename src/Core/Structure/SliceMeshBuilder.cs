namespace BreachAndBastion.Core.Structure;

/// <summary>
/// Turns a <see cref="ProfileSpec"/> into a cross-braced quad mesh: a masonry revetment, an
/// optional earth core behind it and an optional parapet on top.
/// </summary>
public sealed class SliceMeshBuilder
{
    private readonly List<MaterialSpec> _materials = [];
    private readonly List<(double X, double Y, bool Fixed)> _nodes = [];
    private readonly List<double> _mass = [];
    private readonly Dictionary<(int, int), int> _memberIndex = [];
    private readonly List<(int A, int B, double Area, int Material)> _members = [];
    private readonly List<SliceCell> _cells = [];
    private readonly double _width;

    private SliceMeshBuilder(double width) => _width = width;

    public static SliceMesh Build(ProfileSpec profile, IReadOnlyDictionary<string, MaterialSpec> materials, double width)
    {
        var builder = new SliceMeshBuilder(width);
        builder.BuildProfile(profile, materials);
        return builder.ToMesh();
    }

    private void BuildProfile(ProfileSpec p, IReadOnlyDictionary<string, MaterialSpec> materials)
    {
        var revetment = MaterialIndex(materials[p.RevetmentMaterial]);
        var hasEarth = p.EarthThickness > 0 && p.EarthMaterial is not null;
        var earth = hasEarth ? MaterialIndex(materials[p.EarthMaterial!]) : -1;

        var rows = Math.Max(1, (int)Math.Round(p.Height / p.Spacing));
        var revetmentColumns = Math.Max(1, (int)Math.Round((p.RevetmentBaseThickness + p.RevetmentTopThickness) / 2 / p.Spacing));
        var earthColumns = hasEarth ? Math.Max(1, (int)Math.Round(p.EarthThickness / p.EarthSpacing)) : 0;
        var columns = revetmentColumns + earthColumns;
        var batter = p.RevetmentBaseThickness - p.RevetmentTopThickness;

        double ColumnX(int i, double y)
        {
            if (i <= revetmentColumns)
            {
                var outer = batter * y / p.Height;
                return outer + (p.RevetmentBaseThickness - outer) * i / revetmentColumns;
            }
            return p.RevetmentBaseThickness + p.EarthThickness * (i - revetmentColumns) / earthColumns;
        }

        var grid = new int[columns + 1, rows + 1];
        for (var j = 0; j <= rows; j++)
        {
            var y = p.Height * j / rows;
            for (var i = 0; i <= columns; i++)
            {
                // The foundation is fixed; so is the inner end of an earth core, which in reality
                // continues into the rampart and the terreplein.
                var isFixed = j == 0 || (hasEarth && i == columns);
                grid[i, j] = AddNode(ColumnX(i, y), y, isFixed);
            }
        }

        for (var j = 0; j < rows; j++)
            for (var i = 0; i < columns; i++)
                AddCell(grid[i, j], grid[i + 1, j], grid[i + 1, j + 1], grid[i, j + 1], i < revetmentColumns ? revetment : earth);

        if (p.ParapetHeight <= 0 || p.ParapetMaterial is null) return;

        var parapet = MaterialIndex(materials[p.ParapetMaterial]);
        var topOuter = ColumnX(0, p.Height);
        var parapetColumns = 1;
        while (parapetColumns < columns && ColumnX(parapetColumns, p.Height) - topOuter < p.ParapetThickness - 1e-9)
            parapetColumns++;
        var parapetRows = Math.Max(1, (int)Math.Round(p.ParapetHeight / p.Spacing));

        var previous = new int[parapetColumns + 1];
        for (var i = 0; i <= parapetColumns; i++) previous[i] = grid[i, rows];
        for (var k = 1; k <= parapetRows; k++)
        {
            var y = p.Height + p.ParapetHeight * k / parapetRows;
            var current = new int[parapetColumns + 1];
            for (var i = 0; i <= parapetColumns; i++) current[i] = AddNode(ColumnX(i, p.Height), y, false);
            for (var i = 0; i < parapetColumns; i++)
                AddCell(previous[i], previous[i + 1], current[i + 1], current[i], parapet);
            previous = current;
        }
    }

    private int MaterialIndex(MaterialSpec material)
    {
        var index = _materials.IndexOf(material);
        if (index >= 0) return index;
        _materials.Add(material);
        return _materials.Count - 1;
    }

    private int AddNode(double x, double y, bool isFixed)
    {
        _nodes.Add((x, y, isFixed));
        _mass.Add(0);
        return _nodes.Count - 1;
    }

    /// <summary>Corners counter-clockwise from bottom-left. Adds four edges and both diagonals.</summary>
    private void AddCell(int n00, int n10, int n11, int n01, int material)
    {
        var corners = new[] { n00, n10, n11, n01 };
        var area = QuadArea(corners);
        var density = _materials[material].Density;
        foreach (var corner in corners) _mass[corner] += area * _width * density / 4;

        var horizontal = (Distance(n00, n10) + Distance(n01, n11)) / 2;
        var vertical = (Distance(n00, n01) + Distance(n10, n11)) / 2;
        var members = new[]
        {
            AddMember(n00, n10, _width * vertical / 2, material),
            AddMember(n01, n11, _width * vertical / 2, material),
            AddMember(n00, n01, _width * horizontal / 2, material),
            AddMember(n10, n11, _width * horizontal / 2, material),
            AddMember(n00, n11, _width * (horizontal + vertical) / 4, material),
            AddMember(n10, n01, _width * (horizontal + vertical) / 4, material),
        };
        _cells.Add(new SliceCell(corners, members, material));
    }

    /// <summary>Edges shared by two cells get both cells' area contributions.</summary>
    private int AddMember(int a, int b, double area, int material)
    {
        var key = a < b ? (a, b) : (b, a);
        if (_memberIndex.TryGetValue(key, out var existing))
        {
            var m = _members[existing];
            _members[existing] = m with { Area = m.Area + area };
            return existing;
        }
        _members.Add((key.Item1, key.Item2, area, material));
        _memberIndex[key] = _members.Count - 1;
        return _members.Count - 1;
    }

    private double Distance(int a, int b)
    {
        var dx = _nodes[b].X - _nodes[a].X;
        var dy = _nodes[b].Y - _nodes[a].Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private double QuadArea(int[] corners)
    {
        var sum = 0.0;
        for (var k = 0; k < 4; k++)
        {
            var (x0, y0, _) = _nodes[corners[k]];
            var (x1, y1, _) = _nodes[corners[(k + 1) % 4]];
            sum += x0 * y1 - x1 * y0;
        }
        return Math.Abs(sum) / 2;
    }

    private SliceMesh ToMesh()
    {
        var nodes = _nodes.Select((n, i) => new SliceNode(n.X, n.Y, _mass[i], n.Fixed)).ToList();
        var members = _members.Select(m => new SliceMember(m.A, m.B, Distance(m.A, m.B), m.Area, m.Material)).ToList();
        return new SliceMesh(_width, _materials, nodes, members, _cells);
    }
}
