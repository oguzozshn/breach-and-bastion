using Godot;

namespace BreachAndBastion.Game;

public enum CameraMode
{
    /// <summary>Free orbit around the target, for placement and overview.</summary>
    Orbit,

    /// <summary>Over the barrel, with a zoomable telescope, for aiming.</summary>
    Barrel,

    /// <summary>Orthographic side view of one slice, for reading stresses.</summary>
    Section,
}

/// <summary>The three siege cameras from 02-visual-style.</summary>
public partial class CameraRig : Node3D
{
    private Camera3D _camera = null!;
    private float _yaw = Mathf.Pi;
    private float _pitch = 0.35f;
    private float _distance = 90;
    private float _barrelFov = 40;

    public CameraMode Mode { get; private set; } = CameraMode.Orbit;
    public Vector3 OrbitTarget { get; set; }

    /// <summary>Muzzle position and aim direction, for the barrel camera.</summary>
    public (Vector3 Pivot, Vector3 Direction) Barrel { get; set; }

    /// <summary>Slice centre, wall direction and inward direction, for the section camera.</summary>
    public (Vector3 Centre, Vector3 Along, Vector3 Inward) Section { get; set; }

    public override void _Ready()
    {
        _camera = new Camera3D { Current = true, Far = 4000 };
        AddChild(_camera);
    }

    public CameraMode Cycle()
    {
        Mode = (CameraMode)(((int)Mode + 1) % 3);
        return Mode;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion motion when Mode == CameraMode.Orbit && (motion.ButtonMask & MouseButtonMask.Right) != 0:
                _yaw -= motion.Relative.X * 0.005f;
                _pitch = Mathf.Clamp(_pitch + motion.Relative.Y * 0.005f, 0.05f, 1.45f);
                break;
            case InputEventMouseButton { Pressed: true } button when button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown:
                var zoomIn = button.ButtonIndex == MouseButton.WheelUp;
                if (Mode == CameraMode.Orbit) _distance = Mathf.Clamp(_distance * (zoomIn ? 0.9f : 1.1f), 10, 600);
                if (Mode == CameraMode.Barrel) _barrelFov = Mathf.Clamp(_barrelFov * (zoomIn ? 0.8f : 1.25f), 2, 70);
                break;
        }
    }

    public override void _Process(double delta)
    {
        switch (Mode)
        {
            case CameraMode.Orbit:
                _camera.Projection = Camera3D.ProjectionType.Perspective;
                _camera.Fov = 60;
                var offset = new Vector3(Mathf.Sin(_yaw) * Mathf.Cos(_pitch), Mathf.Sin(_pitch), Mathf.Cos(_yaw) * Mathf.Cos(_pitch)) * _distance;
                _camera.GlobalPosition = OrbitTarget + offset;
                _camera.LookAt(OrbitTarget, Vector3.Up);
                break;

            case CameraMode.Barrel:
                _camera.Projection = Camera3D.ProjectionType.Perspective;
                _camera.Fov = _barrelFov;
                var (pivot, direction) = Barrel;
                _camera.GlobalPosition = pivot - direction * 2.5f + Vector3.Up * 0.9f;
                _camera.LookAt(_camera.GlobalPosition + direction * 100, Vector3.Up);
                break;

            case CameraMode.Section:
                _camera.Projection = Camera3D.ProjectionType.Orthogonal;
                _camera.Size = 22;
                var (centre, along, inward) = Section;
                var focus = centre + inward * 6 + Vector3.Up * 6;
                _camera.GlobalPosition = focus + along * 40;
                _camera.LookAt(focus, Vector3.Up);
                break;
        }
    }
}
