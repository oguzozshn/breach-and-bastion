using BreachAndBastion.Core.Ballistics;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>A simple procedural gun: carriage plus a barrel that follows elevation and azimuth.</summary>
public partial class GunView : Node3D
{
    private Node3D _barrelPivot = null!;
    private MeshInstance3D _barrel = null!;

    public static GunView Create(GunSpec gun)
    {
        var view = new GunView { Name = "Gun" };
        var bronze = new StandardMaterial3D { AlbedoColor = new Color(0.55f, 0.40f, 0.18f), Metallic = 0.6f, Roughness = 0.45f };
        var wood = new StandardMaterial3D { AlbedoColor = new Color(0.40f, 0.27f, 0.15f), Roughness = 0.9f };

        // The pivot (trunnions) sits at the node's origin, as in Core's Shot.Pivot.
        view.AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(1.0f, 0.5f, 2.4f) },
            MaterialOverride = wood,
            Position = new Vector3(0, -0.55f, 0.3f),
        });
        foreach (var x in new[] { -0.6f, 0.6f })
        {
            view.AddChild(new MeshInstance3D
            {
                Mesh = new CylinderMesh { TopRadius = 0.55f, BottomRadius = 0.55f, Height = 0.12f },
                MaterialOverride = wood,
                Position = new Vector3(x, -0.45f, 0),
                RotationDegrees = new Vector3(0, 0, 90),
            });
        }

        view._barrelPivot = new Node3D { Name = "BarrelPivot" };
        view.AddChild(view._barrelPivot);
        var length = (float)gun.BarrelLength;
        view._barrel = new MeshInstance3D
        {
            Mesh = new CylinderMesh { TopRadius = (float)gun.Bore, BottomRadius = (float)gun.Bore * 1.8f, Height = length },
            MaterialOverride = bronze,
            // Cylinder runs along +Y; turn it to run along -Z, the pivot's look direction.
            RotationDegrees = new Vector3(-90, 0, 0),
            Position = new Vector3(0, 0, -length / 2),
        };
        view._barrelPivot.AddChild(view._barrel);
        return view;
    }

    /// <summary>Points the barrel along a Core aim direction.</summary>
    public void Aim(Shot shot)
    {
        var direction = shot.AimDirection.ToGodot();
        // The carriage only turns horizontally; the barrel adds the elevation.
        var flat = new Vector3(direction.X, 0, direction.Z);
        if (flat.LengthSquared() > 1e-6) Basis = Basis.LookingAt(flat, Vector3.Up);
        _barrelPivot.Basis = Basis.Inverse() * Basis.LookingAt(direction, Vector3.Up);
    }

    public void SetBurst(bool burst) => _barrel.Visible = !burst;
}
