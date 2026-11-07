using BreachAndBastion.Core.Ballistics;

namespace BreachAndBastion.Core.Tests.Ballistics;

public class DragModelTests
{
    [Theory]
    [InlineData(0.3, 0.47)]
    [InlineData(1.2, 0.95)]
    [InlineData(0.9, 0.70)]
    [InlineData(5.0, 0.90)]
    public void Sphere_drag_follows_table(double mach, double expected)
    {
        Assert.Equal(expected, DragModel.Sphere.Instance.DragCoefficient(mach), 1e-9);
    }
}
