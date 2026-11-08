using System.Collections.Generic;
using BreachAndBastion.Core.Structure;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>
/// Builds a render mesh for one slice: every solid cell is extruded across the slice width.
/// Geometry comes straight from the simulation (10-technical: no hand-modelled walls).
/// </summary>
public static class SliceMeshFactory
{
    // 02-visual-style: muted natural tones; stone, earth and timber clearly distinct.
    private static readonly Dictionary<string, Color> MaterialColours = new()
    {
        ["stone-masonry"] = new Color(0.63f, 0.60f, 0.54f),
        ["earth"] = new Color(0.47f, 0.38f, 0.26f),
        ["timber"] = new Color(0.52f, 0.36f, 0.21f),
    };

    private static readonly Color Parchment = new(0.92f, 0.88f, 0.77f);
    private static readonly Color InkRed = new(0.72f, 0.10f, 0.08f);
    private static readonly Color InkBlue = new(0.10f, 0.20f, 0.62f);
    private static readonly Color InkBlack = new(0.08f, 0.07f, 0.06f);

    public static ArrayMesh? Build(FortressSegment segment, int index, bool stressOverlay)
    {
        var slice = segment.Slices[index].Structure;
        var mesh = slice.Mesh;
        var half = (float)(segment.SliceWidth / 2);
        var centre = segment.ToWorld(index, 0, 0).ToGodot();
        var along = segment.Direction.ToGodot();
        var inward = -segment.Outward.ToGodot();

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var colours = new List<Color>();

        Vector3 World(double depth, double height, float offset) =>
            centre + inward * (float)depth + Vector3.Up * (float)height + along * offset;

        for (var c = 0; c < mesh.Cells.Count; c++)
        {
            if (!slice.IsCellSolid(c)) continue;
            var cell = mesh.Cells[c];
            var colour = CellColour(slice, cell, stressOverlay);

            var front = new Vector3[4];
            var back = new Vector3[4];
            for (var k = 0; k < 4; k++)
            {
                front[k] = World(slice.X(cell.Nodes[k]), slice.Y(cell.Nodes[k]), -half);
                back[k] = World(slice.X(cell.Nodes[k]), slice.Y(cell.Nodes[k]), half);
            }

            AddQuad(vertices, normals, colours, front[0], front[1], front[2], front[3], -along, colour);
            AddQuad(vertices, normals, colours, back[0], back[1], back[2], back[3], along, colour);
            for (var k = 0; k < 4; k++)
            {
                var n = (k + 1) % 4;
                // Outward normal of a counter-clockwise edge in the (depth, height) plane.
                var ex = slice.X(cell.Nodes[n]) - slice.X(cell.Nodes[k]);
                var ey = slice.Y(cell.Nodes[n]) - slice.Y(cell.Nodes[k]);
                var normal = (inward * (float)ey + Vector3.Up * (float)-ex).Normalized();
                AddQuad(vertices, normals, colours, front[k], front[n], back[n], back[k], normal, colour);
            }
        }
        if (vertices.Count == 0) return null;

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = colours.ToArray();
        var result = new ArrayMesh();
        result.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return result;
    }

    private static Color CellColour(SliceStructure slice, SliceCell cell, bool stressOverlay)
    {
        if (!stressOverlay)
        {
            var baseColour = MaterialColours.GetValueOrDefault(slice.Mesh.Materials[cell.Material].Id, Colors.Gray);
            var damage = 0.0;
            foreach (var m in cell.Members) damage = System.Math.Max(damage, slice.Damage(m));
            return baseColour.Darkened((float)(0.6 * damage));
        }

        // 02-visual-style: ink red for tension, ink blue for compression, on parchment.
        var worst = 0.0;
        var worstDamage = 0.0;
        foreach (var m in cell.Members)
        {
            worstDamage = System.Math.Max(worstDamage, slice.Damage(m));
            if (slice.IsBroken(m)) continue;
            var ratio = slice.StressRatio(m);
            if (System.Math.Abs(ratio) > System.Math.Abs(worst)) worst = ratio;
        }
        // Square root so that working stresses (10–30 % of strength) are already visible;
        // accumulated damage darkens towards black ink, so a crater's progress reads too.
        var ink = worst >= 0 ? InkRed : InkBlue;
        var stressed = Parchment.Lerp(ink, (float)System.Math.Min(1, System.Math.Sqrt(System.Math.Abs(worst))));
        return stressed.Lerp(InkBlack, (float)(0.8 * worstDamage));
    }

    private static void AddQuad(List<Vector3> v, List<Vector3> n, List<Color> c, Vector3 a, Vector3 b, Vector3 d, Vector3 e, Vector3 normal, Color colour)
    {
        foreach (var p in new[] { a, b, d, a, d, e })
        {
            v.Add(p);
            n.Add(normal);
            c.Add(colour);
        }
    }
}
