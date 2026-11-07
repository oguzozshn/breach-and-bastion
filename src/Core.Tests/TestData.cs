using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Data;

namespace BreachAndBastion.Core.Tests;

/// <summary>The real game data, copied next to the test assembly.</summary>
internal static class TestData
{
    private static string Dir(string name) => Path.Combine(AppContext.BaseDirectory, "data", name);

    public static IReadOnlyDictionary<string, GunSpec> Guns { get; } = DataLoader.LoadAll<GunSpec>(Dir("guns"), g => g.Id);
    public static IReadOnlyDictionary<string, PowderSpec> Powders { get; } = DataLoader.LoadAll<PowderSpec>(Dir("powders"), p => p.Id);
    public static IReadOnlyDictionary<string, ProjectileSpec> Projectiles { get; } = DataLoader.LoadAll<ProjectileSpec>(Dir("projectiles"), p => p.Id);

    public static GunSpec Culverin => Guns["culverin"];
    public static PowderSpec Serpentine => Powders["serpentine"];
    public static ProjectileSpec IronBall => Projectiles["iron-ball"];
    public static ProjectileSpec StoneBall => Projectiles["stone-ball"];
}
