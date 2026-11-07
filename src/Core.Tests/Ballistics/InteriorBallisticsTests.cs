using BreachAndBastion.Core.Ballistics;

namespace BreachAndBastion.Core.Tests.Ballistics;

public class InteriorBallisticsTests
{
    private static double Velocity(double charge, ProjectileSpec? ball = null) =>
        InteriorBallistics.MuzzleVelocity(TestData.Culverin, TestData.Serpentine, ball ?? TestData.IronBall, charge);

    [Fact]
    public void Culverin_with_nominal_charge_fires_iron_ball_at_period_speed()
    {
        Assert.InRange(TestData.IronBall.Mass, 7.9, 8.1);
        Assert.InRange(Velocity(5), 420, 480);
    }

    [Fact]
    public void More_powder_is_faster_but_with_diminishing_returns()
    {
        Assert.True(Velocity(2) > Velocity(1));
        Assert.True(Velocity(6) > Velocity(5));
        Assert.True(Velocity(2) - Velocity(1) > Velocity(6) - Velocity(5));
    }

    [Fact]
    public void Lighter_stone_ball_leaves_the_muzzle_faster()
    {
        Assert.True(Velocity(5, TestData.StoneBall) > Velocity(5, TestData.IronBall));
    }

    [Fact]
    public void No_charge_means_no_velocity()
    {
        Assert.Equal(0, Velocity(0));
    }

    [Fact]
    public void Shot_larger_than_the_bore_is_rejected()
    {
        var oversized = TestData.IronBall with { Diameter = TestData.Culverin.Bore + 0.01 };
        Assert.Throws<ArgumentException>(() => Velocity(5, oversized));
    }

    [Theory]
    [InlineData(5.0, 0.0)]
    [InlineData(6.0, 0.0)]
    [InlineData(7.5, 0.25)]
    [InlineData(9.0, 1.0)]
    [InlineData(12.0, 1.0)]
    public void Burst_risk_starts_above_safe_charge(double charge, double expected)
    {
        Assert.Equal(expected, InteriorBallistics.BurstProbability(TestData.Culverin, TestData.Serpentine, charge), 1e-12);
    }
}
