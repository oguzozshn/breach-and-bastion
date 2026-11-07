using System.Collections.Generic;
using System.IO;
using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Data;
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

    public static IReadOnlyDictionary<string, GunSpec> Guns { get; } =
        DataLoader.LoadAll<GunSpec>(Path.Combine(DataRoot, "guns"), g => g.Id);

    public static IReadOnlyDictionary<string, PowderSpec> Powders { get; } =
        DataLoader.LoadAll<PowderSpec>(Path.Combine(DataRoot, "powders"), p => p.Id);

    public static IReadOnlyDictionary<string, ProjectileSpec> Projectiles { get; } =
        DataLoader.LoadAll<ProjectileSpec>(Path.Combine(DataRoot, "projectiles"), p => p.Id);
}
