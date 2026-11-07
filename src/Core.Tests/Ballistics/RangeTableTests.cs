using BreachAndBastion.Core.Ballistics;

namespace BreachAndBastion.Core.Tests.Ballistics;

public class RangeTableTests
{
    private static IReadOnlyList<RangeTableEntry> Table() =>
        RangeTable.Build(TestData.Culverin, TestData.Serpentine, TestData.IronBall, 5, Atmosphere.Standard, DragModel.Sphere.Instance);

    [Fact]
    public void Covers_every_whole_degree_up_to_max_elevation()
    {
        var table = Table();

        Assert.Equal(1, table[0].ElevationDegrees);
        Assert.Equal(TestData.Culverin.MaxElevation, table[^1].ElevationDegrees);
        Assert.Equal(20, table.Count);
    }

    [Fact]
    public void Range_grows_with_elevation_below_the_optimum()
    {
        var table = Table();

        for (var i = 1; i < table.Count; i++)
            Assert.True(table[i].Range > table[i - 1].Range, $"range at {table[i].ElevationDegrees}° did not grow");
    }

    [Fact]
    public void Air_drag_shortens_range_compared_with_vacuum()
    {
        var withAir = Table()[9];
        var vacuum = RangeTable.Build(TestData.Culverin, TestData.Serpentine, TestData.IronBall, 5, Atmosphere.Vacuum, DragModel.Sphere.Instance)[9];

        Assert.True(withAir.Range < 0.9 * vacuum.Range);
    }
}
