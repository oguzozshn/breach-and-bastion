using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Tests.Ballistics;

public class TrajectoryTests
{
    [Fact]
    public void Vacuum_range_matches_analytic_solution()
    {
        const double speed = 100, angle = 30 * Math.PI / 180, g = 9.81;
        var simulator = new TrajectorySimulator(Atmosphere.Vacuum, DragModel.Sphere.Instance);
        var launch = new ProjectileState(Vec3.Zero, new Vec3(0, Math.Sin(angle), Math.Cos(angle)) * speed, 0);

        var flight = simulator.FlyUntilGround(TestData.IronBall, launch);

        var expectedRange = speed * speed * Math.Sin(2 * angle) / g;
        var expectedTime = 2 * speed * Math.Sin(angle) / g;
        Assert.True(flight.HitGround);
        Assert.Equal(expectedRange, flight.Impact.Position.Z, 0.05);
        Assert.Equal(expectedTime, flight.Impact.Time, 0.001);
    }

    [Fact]
    public void Quadratic_drag_without_gravity_matches_analytic_solution()
    {
        // With only quadratic drag, x(t) = ln(1 + k·v0·t) / k, where k = ½·ρ·Cd·A / m.
        const double cd = 0.47, v0 = 300;
        var atmosphere = Atmosphere.Standard with { Gravity = 0 };
        var simulator = new TrajectorySimulator(atmosphere, new DragModel.Constant(cd));
        var ball = TestData.IronBall;
        var k = 0.5 * atmosphere.AirDensity * cd * ball.CrossSectionArea / ball.Mass;

        var state = new ProjectileState(Vec3.Zero, new Vec3(v0, 0, 0), 0);
        for (var i = 0; i < 240; i++) state = simulator.Step(ball, state);

        var t = state.Time;
        Assert.Equal(Math.Log(1 + k * v0 * t) / k, state.Position.X, 1e-6);
        Assert.Equal(v0 / (1 + k * v0 * t), state.Velocity.X, 1e-6);
    }

    [Fact]
    public void Same_shot_gives_bit_identical_impact()
    {
        var shot = new Shot(TestData.Culverin, TestData.Serpentine, TestData.IronBall, new Vec3(0, 1, 0), 5, 4, 12);
        var atmosphere = Atmosphere.Standard.WithWind(new Vec3(3, 0, -2));

        var first = new TrajectorySimulator(atmosphere, DragModel.Sphere.Instance).Fire(shot).Impact;
        var second = new TrajectorySimulator(atmosphere, DragModel.Sphere.Instance).Fire(shot).Impact;

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(-10, false)]
    public void Wind_along_the_shot_changes_range(double windAlongShot, bool longer)
    {
        var shot = new Shot(TestData.Culverin, TestData.Serpentine, TestData.StoneBall, Vec3.Zero, 5, 8, 0);
        var still = new TrajectorySimulator(Atmosphere.Standard, DragModel.Sphere.Instance).Fire(shot).Impact.Position.Z;
        var windy = new TrajectorySimulator(Atmosphere.Standard.WithWind(new Vec3(0, 0, windAlongShot)), DragModel.Sphere.Instance)
            .Fire(shot).Impact.Position.Z;

        Assert.Equal(longer, windy > still);
    }

    [Fact]
    public void Azimuth_turns_from_plus_z_towards_plus_x()
    {
        var shot = new Shot(TestData.Culverin, TestData.Serpentine, TestData.IronBall, Vec3.Zero, 5, 0, 90);

        Assert.Equal(1, shot.AimDirection.X, 1e-12);
        Assert.Equal(0, shot.AimDirection.Z, 1e-12);
    }

    [Fact]
    public void Iron_ball_keeps_more_energy_downrange_than_stone_ball()
    {
        // Denser shot keeps its speed better: the reason iron shot replaced stone.
        var simulator = new TrajectorySimulator(Atmosphere.Standard, DragModel.Sphere.Instance);
        double ImpactEnergy(ProjectileSpec ball)
        {
            var impact = simulator.Fire(new Shot(TestData.Culverin, TestData.Serpentine, ball, Vec3.Zero, 5, 5, 0)).Impact;
            return 0.5 * ball.Mass * impact.Velocity.LengthSquared;
        }

        Assert.True(ImpactEnergy(TestData.IronBall) > 2 * ImpactEnergy(TestData.StoneBall));
    }
}
