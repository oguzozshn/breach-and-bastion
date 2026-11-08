using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Mathematics;
using BreachAndBastion.Core.Siege;

namespace BreachAndBastion.Core.Tests.Siege;

public class SiegeSimulationTests
{
    private static SiegeSimulation NewSiege(ulong seed = 1) =>
        new([TestData.BuildFortress("tall-wall"), TestData.BuildFortress("bastion")], Atmosphere.Standard, DragModel.Sphere.Instance, seed);

    private static Shot AimedShot(SiegeSimulation siege, Vec3 pivot, Vec3 target)
    {
        var shot = new Shot(TestData.Culverin, TestData.Serpentine, TestData.IronBall, pivot, 5, 0, 0);
        var elevation = Gunnery.ElevationFor(siege.Ballistics, shot, target) ?? throw new InvalidOperationException("Out of range");
        return shot with { ElevationDegrees = elevation, AzimuthDegrees = Gunnery.AzimuthTowards(pivot, target) };
    }

    [Fact]
    public void Straight_wall_faces_minus_z()
    {
        var wall = TestData.BuildFortress("tall-wall").Segments[0];

        Assert.Equal(new Vec3(0, 0, -1), wall.Outward);
        Assert.Equal(1, wall.ToLocal(new Vec3(0, 5, 301)).Depth, 1e-9);
    }

    [Fact]
    public void Square_shot_at_the_wall_hits_with_no_obliquity()
    {
        var siege = NewSiege();
        var pivot = new Vec3(0, 1, 0);

        var result = siege.Fire(AimedShot(siege, pivot, new Vec3(0, 5, 300)));

        Assert.NotNull(result.Hit);
        Assert.Equal("tall-wall", result.Hit!.Fortress.Spec.Id);
        Assert.InRange(result.Hit.ObliquityDegrees, 0, 2);
        Assert.InRange(result.Hit.Height, 4, 6);
        Assert.True(result.Hit.ImpulseDepth > 0, "impulse must push into the wall");
    }

    [Fact]
    public void Shot_at_a_bastion_face_strikes_obliquely()
    {
        var siege = NewSiege();
        var pivot = new Vec3(200, 1, 0);

        var result = siege.Fire(AimedShot(siege, pivot, new Vec3(190, 4, 302.5)));

        Assert.NotNull(result.Hit);
        Assert.Equal("bastion", result.Hit!.Fortress.Spec.Id);
        Assert.Equal(1, result.Hit.Segment);
        Assert.InRange(result.Hit.ObliquityDegrees, 48, 58);
    }

    /// <summary>Shots until a practicable breach, or null if none within <paramref name="maxShots"/>.</summary>
    private static int? ShotsToBreach(string fortressId, Vec3 pivot, Vec3 target, int maxShots)
    {
        var siege = new SiegeSimulation([TestData.BuildFortress(fortressId)], Atmosphere.Standard, DragModel.Sphere.Instance, 1);
        var shot = AimedShot(siege, pivot, target);
        for (var i = 1; i <= maxShots; i++)
        {
            var hit = siege.Fire(shot).Hit;
            for (var frame = 0; frame < 600 && siege.AnyAwake(); frame++) siege.Advance(1.0 / 60);
            if (hit is not null && SiegeSimulation.IsPracticable(hit.Fortress.Segments[hit.Segment].Slices[hit.Slice])) return i;
        }
        return null;
    }

    [Fact]
    public void M1_tall_wall_breaches_in_less_than_half_the_shots_a_bastion_resists()
    {
        // PLAN.md M1 definition of done, item 2: same gun, only materials, geometry and physics.
        var wall = ShotsToBreach("tall-wall", new Vec3(0, 1, 0), new Vec3(0, 4, 300), 40);
        Assert.NotNull(wall);

        var limit = 2 * wall!.Value;
        Assert.Null(ShotsToBreach("bastion", new Vec3(200, 1, 0), new Vec3(190, 3, 302.5), limit));
        // Even a battery square to the face must not do it.
        Assert.Null(ShotsToBreach("bastion", new Vec3(-44.3, 1, 115.1), new Vec3(190, 3, 302.5), limit));
    }

    [Fact]
    public void Same_seed_and_inputs_give_identical_state()
    {
        static ulong Run()
        {
            var siege = NewSiege(seed: 7);
            var targets = new[] { new Vec3(0, 5, 300), new Vec3(2, 3, 300), new Vec3(190, 4, 302.5) };
            foreach (var target in targets)
            {
                var pivot = new Vec3(target.X, 1, 0);
                siege.Fire(AimedShot(siege, pivot, target));
                for (var i = 0; i < 30; i++) siege.Advance(1.0 / 60);
            }
            return siege.StateHash();
        }

        Assert.Equal(Run(), Run());
    }
}
