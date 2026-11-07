using BreachAndBastion.Core.Ballistics;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>
/// Root of the M1 siege scene. For now it only proves the Godot ↔ Core wiring by printing
/// the culverin's range table; the scene itself is built in later M1 tasks.
/// </summary>
public partial class SiegePrototype : Node3D
{
    public override void _Ready()
    {
        var table = RangeTable.Build(
            GameData.Guns["culverin"], GameData.Powders["serpentine"], GameData.Projectiles["iron-ball"],
            charge: 5, Atmosphere.Standard, DragModel.Sphere.Instance, stepDegrees: 5);

        foreach (var entry in table)
            GD.Print($"{entry.ElevationDegrees,4:0}°  {entry.Range,7:0} m  {entry.FlightTime,5:0.0} s  {entry.ImpactSpeed,5:0} m/s");
    }
}
