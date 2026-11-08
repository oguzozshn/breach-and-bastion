using System.Collections.Generic;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>Visual-only effects: dust, smoke and debris chunks (ADR 0006: outcomes stay in Core).</summary>
public partial class Effects : Node3D
{
    private const int MaxDebris = 400;
    private const double DebrisLifetime = 10;

    private readonly Queue<(RigidBody3D Body, double SpawnTime)> _debris = new();
    private readonly Dictionary<Color, StandardMaterial3D> _debrisMaterials = new();
    private double _time;

    public override void _Process(double delta)
    {
        _time += delta;
        while (_debris.Count > 0 && (_time - _debris.Peek().SpawnTime > DebrisLifetime || _debris.Count > MaxDebris))
            _debris.Dequeue().Body.QueueFree();
    }

    public void Dust(Vector3 position, Color colour, int amount = 60, float size = 0.6f) =>
        Burst(position, colour, amount, size, lifetime: 2.5f, speed: 4f, gravity: new Vector3(0, -1.5f, 0));

    public void Smoke(Vector3 position, Vector3 direction) =>
        Burst(position, new Color(0.85f, 0.85f, 0.82f, 0.8f), 50, 0.9f, lifetime: 3f, speed: 6f, gravity: new Vector3(0, 0.6f, 0), direction);

    public void Explosion(Vector3 position)
    {
        Burst(position, new Color(1f, 0.55f, 0.15f), 80, 0.7f, lifetime: 0.8f, speed: 12f, gravity: Vector3.Zero);
        Smoke(position, Vector3.Up);
    }

    /// <summary>A visual chunk falling from where a Core node detached.</summary>
    public void Debris(Vector3 position, Vector3 velocity, Color colour)
    {
        if (!_debrisMaterials.TryGetValue(colour, out var material))
            _debrisMaterials[colour] = material = new StandardMaterial3D { AlbedoColor = colour, Roughness = 0.95f };

        var size = new Vector3(0.45f, 0.4f, 0.5f);
        var body = new RigidBody3D { Position = position, LinearVelocity = velocity, Mass = 50 };
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = size }, MaterialOverride = material });
        AddChild(body);
        _debris.Enqueue((body, _time));
    }

    private void Burst(Vector3 position, Color colour, int amount, float size, float lifetime, float speed, Vector3 gravity, Vector3? direction = null)
    {
        var particles = new CpuParticles3D
        {
            Position = position,
            Amount = amount,
            Lifetime = lifetime,
            OneShot = true,
            Explosiveness = 0.95f,
            Direction = direction ?? Vector3.Up,
            Spread = direction is null ? 70 : 25,
            InitialVelocityMin = speed * 0.3f,
            InitialVelocityMax = speed,
            Gravity = gravity,
            DampingMin = 1.5f,
            DampingMax = 2.5f,
            ScaleAmountMin = size * 0.5f,
            ScaleAmountMax = size,
            Mesh = new SphereMesh { Radius = 0.5f, Height = 1f, RadialSegments = 6, Rings = 3 },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = colour,
                Transparency = colour.A < 1 ? BaseMaterial3D.TransparencyEnum.Alpha : BaseMaterial3D.TransparencyEnum.Disabled,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            },
            Emitting = true,
        };
        AddChild(particles);
        GetTree().CreateTimer(lifetime + 0.5).Timeout += particles.QueueFree;
    }
}
