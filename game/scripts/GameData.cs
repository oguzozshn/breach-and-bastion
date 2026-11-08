using System.Collections.Generic;
using System.IO;
using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Data;
using BreachAndBastion.Core.Mathematics;
using BreachAndBastion.Core.Structure;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>
/// Loads the shared <c>data/</c> folder at the repository root. During development the Godot
/// project sits next to it; export packaging is an M1 playtest-build task.
/// </summary>
public static class GameData
{
    private static string DataRoot =>
        Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "data"));

    private static IReadOnlyDictionary<string, T> Load<T>(string folder, System.Func<T, string> id) =>
        DataLoader.LoadAll(Path.Combine(DataRoot, folder), id);

    public static IReadOnlyDictionary<string, GunSpec> Guns { get; } = Load<GunSpec>("guns", g => g.Id);
    public static IReadOnlyDictionary<string, PowderSpec> Powders { get; } = Load<PowderSpec>("powders", p => p.Id);
    public static IReadOnlyDictionary<string, ProjectileSpec> Projectiles { get; } = Load<ProjectileSpec>("projectiles", p => p.Id);
    public static IReadOnlyDictionary<string, MaterialSpec> Materials { get; } = Load<MaterialSpec>("materials", m => m.Id);
    public static IReadOnlyDictionary<string, ProfileSpec> Profiles { get; } = Load<ProfileSpec>("profiles", p => p.Id);
    public static IReadOnlyDictionary<string, FortressSpec> Fortresses { get; } = Load<FortressSpec>("fortresses", f => f.Id);
}

/// <summary>Core uses double-precision vectors; Godot uses floats.</summary>
public static class VectorExtensions
{
    public static Vector3 ToGodot(this Vec3 v) => new((float)v.X, (float)v.Y, (float)v.Z);
}
