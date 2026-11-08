using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Data;
using BreachAndBastion.Core.Structure;

namespace BreachAndBastion.Core.Tests;

/// <summary>The real game data, copied next to the test assembly.</summary>
internal static class TestData
{
    private static string Dir(string name) => Path.Combine(AppContext.BaseDirectory, "data", name);

    public static IReadOnlyDictionary<string, GunSpec> Guns { get; } = DataLoader.LoadAll<GunSpec>(Dir("guns"), g => g.Id);
    public static IReadOnlyDictionary<string, PowderSpec> Powders { get; } = DataLoader.LoadAll<PowderSpec>(Dir("powders"), p => p.Id);
    public static IReadOnlyDictionary<string, ProjectileSpec> Projectiles { get; } = DataLoader.LoadAll<ProjectileSpec>(Dir("projectiles"), p => p.Id);
    public static IReadOnlyDictionary<string, MaterialSpec> Materials { get; } = DataLoader.LoadAll<MaterialSpec>(Dir("materials"), m => m.Id);
    public static IReadOnlyDictionary<string, ProfileSpec> Profiles { get; } = DataLoader.LoadAll<ProfileSpec>(Dir("profiles"), p => p.Id);
    public static IReadOnlyDictionary<string, FortressSpec> Fortresses { get; } = DataLoader.LoadAll<FortressSpec>(Dir("fortresses"), f => f.Id);

    public static GunSpec Culverin => Guns["culverin"];
    public static PowderSpec Serpentine => Powders["serpentine"];
    public static ProjectileSpec IronBall => Projectiles["iron-ball"];
    public static ProjectileSpec StoneBall => Projectiles["stone-ball"];

    public static Fortress BuildFortress(string id) => Fortress.Build(Fortresses[id], Profiles, Materials);
}
