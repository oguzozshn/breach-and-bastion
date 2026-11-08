using BreachAndBastion.Core.Structure;

namespace BreachAndBastion.Core.Tests.Structure;

public class SliceMeshBuilderTests
{
    [Fact]
    public void Tall_wall_mass_matches_its_cross_section()
    {
        var profile = TestData.Profiles["tall-medieval-wall"];
        var mesh = SliceMeshBuilder.Build(profile, TestData.Materials, 2);

        // Trapezoid body plus the parapet, which snaps to one mesh column (0.5 m) of the top.
        var body = (profile.RevetmentBaseThickness + profile.RevetmentTopThickness) / 2 * profile.Height;
        var parapet = 0.5 * profile.ParapetHeight;
        var expected = (body + parapet) * 2 * TestData.Materials["stone-masonry"].Density;
        Assert.Equal(expected, mesh.TotalMass, expected * 0.001);
    }

    [Fact]
    public void Foundation_nodes_are_fixed_and_only_they_are_for_a_free_standing_wall()
    {
        var mesh = SliceMeshBuilder.Build(TestData.Profiles["tall-medieval-wall"], TestData.Materials, 2);

        Assert.All(mesh.Nodes, n => Assert.Equal(n.Y == 0, n.Fixed));
    }

    [Fact]
    public void Bastion_has_masonry_revetment_and_earth_core()
    {
        var mesh = SliceMeshBuilder.Build(TestData.Profiles["bastion-face"], TestData.Materials, 2);

        Assert.Contains(mesh.Cells, c => mesh.Materials[c.Material].Id == "stone-masonry");
        Assert.Contains(mesh.Cells, c => mesh.Materials[c.Material].Id == "earth");
        Assert.Equal(13, mesh.Nodes.Max(n => n.X), 1e-9);
        Assert.Equal(8.5, mesh.Nodes.Max(n => n.Y), 1e-9);
    }

    [Fact]
    public void Every_cell_is_cross_braced()
    {
        var mesh = SliceMeshBuilder.Build(TestData.Profiles["bastion-face"], TestData.Materials, 2);

        Assert.All(mesh.Cells, c => Assert.Equal(6, c.Members.Distinct().Count()));
    }
}
