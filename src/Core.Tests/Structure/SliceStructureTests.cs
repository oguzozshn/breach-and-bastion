using BreachAndBastion.Core.Structure;

namespace BreachAndBastion.Core.Tests.Structure;

public class SliceStructureTests
{
    private static readonly MaterialSpec Rod = new("rod", "Test rod", 1000, 1e7, 1e9, 1e9, 0.1, 0.5, 10, 1e6);

    private static SliceMesh Mesh(MaterialSpec material, SliceNode[] nodes, params (int A, int B)[] members) =>
        new(1, [material], nodes,
            members.Select(m => new SliceMember(m.A, m.B,
                Math.Sqrt(Math.Pow(nodes[m.B].X - nodes[m.A].X, 2) + Math.Pow(nodes[m.B].Y - nodes[m.A].Y, 2)), 0.01, 0)).ToList(),
            []);

    [Fact]
    public void Hanging_mass_stretches_rod_by_weight_over_stiffness()
    {
        // k = E·A/L = 1e7 · 0.01 / 1 = 1e5 N/m; δ = m·g / k.
        const double mass = 100;
        var mesh = Mesh(Rod, [new(0, 1, 0, true), new(0, 0, mass, false)], (0, 1));
        var slice = new SliceStructure(mesh);

        slice.Settle(5);

        Assert.Equal(-mass * SliceStructure.Gravity / 1e5, slice.Y(1), 1e-5);
        Assert.Equal(mass * SliceStructure.Gravity / 0.01, slice.Stress(0), 1.0);
    }

    [Fact]
    public void Two_bar_truss_carries_load_with_statics_tension()
    {
        // Supports at (±1, 1), load at the origin: each bar's tension is m·g / (2·sin 45°).
        const double mass = 100;
        var mesh = Mesh(Rod, [new(-1, 1, 0, true), new(1, 1, 0, true), new(0, 0, mass, false)], (0, 2), (1, 2));
        var slice = new SliceStructure(mesh);

        slice.Settle(5);

        var expected = mass * SliceStructure.Gravity / (2 * Math.Sin(Math.PI / 4)) / 0.01;
        Assert.Equal(expected, slice.Stress(0), expected * 0.01);
        Assert.Equal(expected, slice.Stress(1), expected * 0.01);
    }

    [Fact]
    public void Overloaded_rod_breaks_and_its_mass_falls_off()
    {
        var weak = Rod with { TensileStrength = 5e4 };  // weight gives ~9.8e4 Pa
        var mesh = Mesh(weak, [new(0, 1, 0, true), new(0, 0, 100, false)], (0, 1));
        var slice = new SliceStructure(mesh);
        slice.Settle();

        slice.Wake();
        var fallen = slice.Advance(0.1);

        Assert.True(slice.IsBroken(0));
        Assert.Single(fallen);
        Assert.True(slice.IsDetached(1));
    }

    [Fact]
    public void Impulse_changes_total_momentum_by_exactly_the_impulse()
    {
        var slice = new SliceStructure(SliceMeshBuilder.Build(TestData.Profiles["tall-medieval-wall"], TestData.Materials, 2));
        slice.Settle();

        slice.ApplyImpulse(0, 6, 2400, -300, 0.6);

        double px = 0, py = 0;
        for (var i = 0; i < slice.Mesh.Nodes.Count; i++)
        {
            px += slice.Mesh.Nodes[i].Mass * slice.VelocityX(i);
            py += slice.Mesh.Nodes[i].Mass * slice.VelocityY(i);
        }
        Assert.Equal(2400, px, 1e-6);
        Assert.Equal(-300, py, 1e-6);
        Assert.True(slice.Awake);
    }

    [Theory]
    [InlineData("tall-medieval-wall")]
    [InlineData("bastion-face")]
    public void M1_profiles_stand_under_their_own_weight(string profile)
    {
        var slice = new SliceStructure(SliceMeshBuilder.Build(TestData.Profiles[profile], TestData.Materials, 2));
        slice.Settle();

        slice.Wake();
        slice.Advance(1.0);

        // No member may sit above its damage onset at rest, or walls would decay on their own.
        Assert.Equal(0, slice.BrokenMemberCount);
        for (var j = 0; j < slice.Mesh.Members.Count; j++)
        {
            var onset = slice.Mesh.Materials[slice.Mesh.Members[j].Material].DamageOnset;
            Assert.True(Math.Abs(slice.StressRatio(j)) < onset, $"member {j} at {slice.StressRatio(j):0.00} of its strength");
        }
    }

    [Fact]
    public void Crushing_breaks_members_at_the_impact_only()
    {
        var slice = new SliceStructure(SliceMeshBuilder.Build(TestData.Profiles["tall-medieval-wall"], TestData.Materials, 2));
        slice.Settle();

        slice.ApplyCrushing(0.2, 4, 2e7, 0.6);

        var broken = Enumerable.Range(0, slice.Mesh.Members.Count).Where(slice.IsBroken).ToList();
        Assert.NotEmpty(broken);
        Assert.All(broken, j =>
        {
            var m = slice.Mesh.Members[j];
            var y = (slice.Mesh.Nodes[m.A].Y + slice.Mesh.Nodes[m.B].Y) / 2;
            Assert.InRange(y, 3.4, 4.6);
        });
    }

    [Fact]
    public void Sleeps_once_still_and_wakes_on_impulse()
    {
        var slice = new SliceStructure(SliceMeshBuilder.Build(TestData.Profiles["tall-medieval-wall"], TestData.Materials, 2));
        slice.Settle();
        Assert.False(slice.Awake);

        slice.ApplyImpulse(0, 6, 100, 0, 0.6);
        Assert.True(slice.Awake);

        for (var i = 0; i < 600 && slice.Awake; i++) slice.Advance(1.0 / 60);
        Assert.False(slice.Awake);
    }
}
