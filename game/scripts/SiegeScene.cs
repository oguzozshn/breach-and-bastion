using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Mathematics;
using BreachAndBastion.Core.Siege;
using BreachAndBastion.Core.Structure;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>
/// M1 siege prototype: a tall medieval wall and a low angled bastion, the same gun facing
/// both (PLAN.md). Core decides everything; this node animates, renders and takes input.
/// </summary>
public partial class SiegeScene : Node3D
{
    private const double SimulationStep = 1.0 / 60;
    private const int MaxStepsPerFrame = 3;

    private sealed record Target(string Name, string FortressId, Vec3 Pivot, Vec3 AimPoint);

    // Same gun, same distance (300 m) to both targets. The bastion battery faces its salient.
    private static readonly Target[] Targets =
    [
        new("Tall wall", "tall-wall", new Vec3(0, 1, 0), new Vec3(0, 4, 300)),
        new("Bastion", "bastion", new Vec3(200, 1, 0), new Vec3(190, 3, 302.5)),
    ];

    private readonly GunSpec _gun = GameData.Guns["culverin"];
    private readonly PowderSpec _powder = GameData.Powders["serpentine"];
    private readonly ProjectileSpec[] _shotTypes = [GameData.Projectiles["iron-ball"], GameData.Projectiles["stone-ball"]];

    private SiegeSimulation _siege = null!;
    private readonly List<FortressView> _views = [];
    private Node3D _world = null!;
    private Effects _effects = null!;
    private GunView _gunView = null!;
    private CameraRig _cameraRig = null!;
    private Hud _hud = null!;
    private StandardMaterial3D _wallMaterial = null!;
    private StandardMaterial3D _overlayMaterial = null!;
    private MeshInstance3D _ball = null!;

    private ulong _seed = 1;
    private int _target;
    private int _shotType;
    private double _charge = 5;
    private double _elevation;
    private double _azimuth;
    private bool _stressOverlay;
    private bool _gunBurst;
    private int _shotsFired;
    private int? _shotsToBreach;
    private string _lastShot = "-";
    private (int Segment, int Slice) _sectionSlice;

    private ShotResult? _flight;
    private double _flightTime;
    private double _accumulator;
    private double _simulationMs;

    // Debug capture: -- --screenshot=<path> [--frames=N] [--auto-fire=K] [--camera=orbit|barrel|section]
    private string? _screenshotPath;
    private int _screenshotFrames = 240;
    private int _autoFire;
    private int _frame;
    private string? _initialCamera;

    public override void _Ready()
    {
        // UI text is English (repo language); keep "5.00" rather than the OS locale's "5,00".
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        ParseDebugArguments();
        BuildEnvironment();
        _wallMaterial = new StandardMaterial3D
        {
            VertexColorUseAsAlbedo = true,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            Roughness = 0.95f,
        };
        // Stress overlay reads as flat ink on parchment, independent of the lighting.
        _overlayMaterial = new StandardMaterial3D
        {
            VertexColorUseAsAlbedo = true,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
        _effects = new Effects { Name = "Effects" };
        AddChild(_effects);
        _gunView = GunView.Create(_gun);
        AddChild(_gunView);
        _ball = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 0.25f, Height = 0.5f },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.1f, 0.1f, 0.1f) },
            Visible = false,
        };
        AddChild(_ball);
        _cameraRig = new CameraRig { Name = "CameraRig" };
        AddChild(_cameraRig);
        _hud = new Hud { Name = "Hud" };
        AddChild(_hud);

        ResetSiege();
        if (_initialCamera is not null) SetCamera(_initialCamera);
    }

    private void BuildEnvironment()
    {
        var environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Sky,
            Sky = new Sky { SkyMaterial = new ProceduralSkyMaterial() },
            AmbientLightSource = Godot.Environment.AmbientSource.Sky,
            AmbientLightEnergy = 0.5f,
            TonemapMode = Godot.Environment.ToneMapper.Filmic,
        };
        AddChild(new WorldEnvironment { Environment = environment });
        AddChild(new DirectionalLight3D
        {
            ShadowEnabled = true,
            LightEnergy = 0.75f,
            // Shines towards +Z, onto the faces the attacker sees.
            RotationDegrees = new Vector3(-45, 160, 0),
            DirectionalShadowMaxDistance = 600,
        });

        var ground = new StaticBody3D { Name = "Ground" };
        ground.AddChild(new CollisionShape3D { Shape = new WorldBoundaryShape3D() });
        ground.AddChild(new MeshInstance3D
        {
            Mesh = new PlaneMesh { Size = new Vector2(3000, 3000) },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.42f, 0.47f, 0.30f), Roughness = 1 },
        });
        AddChild(ground);
    }

    /// <summary>Rebuilds both fortresses from data with a fresh, seeded wind.</summary>
    private void ResetSiege()
    {
        _world?.QueueFree();
        _world = new Node3D { Name = "Fortresses" };
        AddChild(_world);
        _views.Clear();

        var random = new DeterministicRandom(_seed);
        var windSpeed = 5 * random.NextDouble();
        var windHeading = 2 * System.Math.PI * random.NextDouble();
        var wind = new Vec3(System.Math.Sin(windHeading), 0, System.Math.Cos(windHeading)) * windSpeed;

        var fortresses = Targets.Select(t => Fortress.Build(GameData.Fortresses[t.FortressId], GameData.Profiles, GameData.Materials)).ToList();
        _siege = new SiegeSimulation(fortresses, Atmosphere.Standard.WithWind(wind), DragModel.Sphere.Instance, _seed);
        foreach (var fortress in fortresses)
        {
            var view = FortressView.Create(fortress, _wallMaterial, _overlayMaterial);
            view.StressOverlay = _stressOverlay;
            _views.Add(view);
            _world.AddChild(view);
        }

        _gunBurst = false;
        _gunView.SetBurst(false);
        _flight = null;
        _ball.Visible = false;
        _shotsFired = 0;
        _shotsToBreach = null;
        _lastShot = "-";
        SelectTarget(_target);
    }

    private void SelectTarget(int index)
    {
        _target = index;
        var target = Targets[index];
        _gunView.Position = target.Pivot.ToGodot();
        _azimuth = Gunnery.AzimuthTowards(target.Pivot, target.AimPoint);
        _elevation = 0;
        _shotsFired = 0;
        _shotsToBreach = null;

        var fortress = _siege.Fortresses[index];
        for (var s = 0; s < fortress.Segments.Count; s++)
        {
            var local = fortress.Segments[s].ToLocal(target.AimPoint);
            var slice = fortress.Segments[s].SliceIndexAt(local.Along);
            if (slice >= 0 && System.Math.Abs(local.Depth) < 1) { _sectionSlice = (s, slice); break; }
        }
        _cameraRig.OrbitTarget = target.AimPoint.ToGodot();
        ApplyCameraMode();
    }

    private Shot CurrentShot() =>
        new(_gun, _powder, _shotTypes[_shotType], Targets[_target].Pivot, _charge, _elevation, _azimuth);

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } key) return;
        var fine = key.ShiftPressed ? 10 : 1;
        switch (key.Keycode)
        {
            case Key.W: _elevation = System.Math.Min(_gun.MaxElevation, _elevation + 0.05 * fine); break;
            case Key.S: _elevation = System.Math.Max(_gun.MinElevation, _elevation - 0.05 * fine); break;
            case Key.D: _azimuth += 0.05 * fine; break;
            case Key.A: _azimuth -= 0.05 * fine; break;
            case Key.E: _charge = System.Math.Min(12, _charge + 0.25); break;
            case Key.Q: _charge = System.Math.Max(0.5, _charge - 0.25); break;
            case Key.T when !key.Echo: _shotType = (_shotType + 1) % _shotTypes.Length; break;
            case Key.Space when !key.Echo: Fire(); break;
            case Key.Tab when !key.Echo: SelectTarget((_target + 1) % Targets.Length); break;
            case Key.R when !key.Echo: _seed++; ResetSiege(); break;
            case Key.C when !key.Echo: _cameraRig.Cycle(); ApplyCameraMode(); break;
            case Key.V when !key.Echo:
                _stressOverlay = !_stressOverlay;
                foreach (var view in _views) view.StressOverlay = _stressOverlay;
                break;
        }
    }

    private void ApplyCameraMode()
    {
        var section = _cameraRig.Mode == CameraMode.Section;
        for (var i = 0; i < _views.Count; i++)
        {
            _views[i].Visible = !section || i == _target;
            _views[i].Isolate(section && i == _target ? _sectionSlice.Segment : -1, _sectionSlice.Slice);
        }
        var segment = _siege.Fortresses[_target].Segments[_sectionSlice.Segment];
        _cameraRig.Section = (segment.ToWorld(_sectionSlice.Slice, 0, 0).ToGodot(), segment.Direction.ToGodot(), -segment.Outward.ToGodot());
    }

    private void Fire()
    {
        if (_flight is not null || _gunBurst) return;
        var shot = CurrentShot();
        var muzzle = shot.LaunchState().Position.ToGodot();
        _shotsFired++;

        var result = _siege.Fire(shot, applyImpact: false);
        if (result.Burst)
        {
            _gunBurst = true;
            _gunView.SetBurst(true);
            _effects.Explosion(muzzle);
            _lastShot = "BARREL BURST - press R to reset";
            return;
        }
        _effects.Smoke(muzzle, shot.AimDirection.ToGodot());
        _flight = result;
        _flightTime = 0;
        _ball.Visible = true;
    }

    public override void _Process(double delta)
    {
        AnimateFlight(delta);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        _accumulator += delta;
        var steps = 0;
        while (_accumulator >= SimulationStep && steps < MaxStepsPerFrame)
        {
            foreach (var rubble in _siege.Advance(SimulationStep)) SpawnDebris(rubble);
            _accumulator -= SimulationStep;
            steps++;
        }
        if (steps == MaxStepsPerFrame) _accumulator = 0;  // fall behind gracefully, never spiral
        foreach (var view in _views) view.Refresh();
        _simulationMs = 0.9 * _simulationMs + 0.1 * watch.Elapsed.TotalMilliseconds;

        var shot = CurrentShot();
        _gunView.Aim(shot);
        _cameraRig.Barrel = (Targets[_target].Pivot.ToGodot(), shot.AimDirection.ToGodot());
        UpdateBreach();
        UpdateHud(shot);
        DebugCapture();
    }

    private void AnimateFlight(double delta)
    {
        if (_flight is null) return;
        _flightTime += delta;
        var samples = _flight.Samples;
        var last = samples[^1];
        if (_flightTime >= last.Time)
        {
            _ball.Visible = false;
            Arrive(_flight);
            _flight = null;
            return;
        }
        var i = 1;
        while (i < samples.Count - 1 && samples[i].Time < _flightTime) i++;
        var a = samples[i - 1];
        var b = samples[i];
        var t = (float)((_flightTime - a.Time) / System.Math.Max(1e-9, b.Time - a.Time));
        _ball.GlobalPosition = a.Position.ToGodot().Lerp(b.Position.ToGodot(), t);
    }

    private void Arrive(ShotResult result)
    {
        if (result.Hit is { } hit)
        {
            SiegeSimulation.ApplyImpact(hit);
            _sectionSlice = (hit.Segment, hit.Slice);
            if (_cameraRig.Mode == CameraMode.Section) ApplyCameraMode();
            _effects.Dust(hit.Point.ToGodot(), new Color(0.62f, 0.58f, 0.50f));
            _lastShot = $"hit at {hit.Height:0.0} m, obliquity {hit.ObliquityDegrees:0}°{(hit.Ricochet ? ", RICOCHET" : "")}";
        }
        else if (result.Impact is { } impact)
        {
            _effects.Dust(impact.Position.ToGodot(), new Color(0.45f, 0.40f, 0.30f), 30, 0.4f);
            var range = Targets[_target].Pivot;
            var distance = System.Math.Sqrt(System.Math.Pow(impact.Position.X - range.X, 2) + System.Math.Pow(impact.Position.Z - range.Z, 2));
            _lastShot = $"ground at {distance:0} m";
        }
    }

    private void SpawnDebris(RubbleEvent rubble)
    {
        var segment = rubble.Fortress.Segments[rubble.Segment];
        var position = segment.ToWorld(rubble.Slice, rubble.Node.X, rubble.Node.Y).ToGodot();
        var velocity = (-segment.Outward * rubble.Node.VelocityX + Vec3.Up * rubble.Node.VelocityY).ToGodot();
        var mesh = segment.Mesh;
        var members = mesh.MembersOfNode[rubble.Node.Node];
        var material = members.Count > 0 ? mesh.Materials[mesh.Members[members[0]].Material].Id : "stone-masonry";
        _effects.Debris(position, velocity, material == "earth" ? new Color(0.47f, 0.38f, 0.26f) : new Color(0.63f, 0.60f, 0.54f));
    }

    private void UpdateBreach()
    {
        if (_shotsToBreach is not null) return;
        var fortress = _siege.Fortresses[_target];
        if (fortress.Segments.SelectMany(s => s.Slices).Any(SiegeSimulation.IsPracticable))
            _shotsToBreach = _shotsFired;
    }

    private void UpdateHud(Shot shot)
    {
        var target = Targets[_target];
        var wind = _siege.Ballistics.Atmosphere.Wind;
        var aim = shot.AimDirection;
        var tail = wind.X * aim.X + wind.Z * aim.Z;
        var cross = wind.X * aim.Z - wind.Z * aim.X;
        var burst = InteriorBallistics.BurstProbability(_gun, _powder, _charge);

        var status = new StringBuilder()
            .AppendLine($"TARGET   {target.Name}  [Tab]")
            .AppendLine($"GUN      {_gun.Name}, {_shotTypes[_shotType].Name}  [T]")
            .AppendLine($"CHARGE   {_charge:0.00} kg  [Q/E]   burst risk {burst * 100:0} %")
            .AppendLine($"ELEV     {_elevation:0.00}°  [W/S]   AZIM {_azimuth:0.00}°  [A/D]")
            .AppendLine($"WIND     {(tail >= 0 ? "tail" : "head")} {System.Math.Abs(tail):0.0} m/s, cross {System.Math.Abs(cross):0.0} m/s {(cross >= 0 ? "to the right" : "to the left")}")
            .AppendLine($"SHOTS    {_shotsFired}")
            .AppendLine($"LAST     {_lastShot}")
            .AppendLine($"BREACH   {(_shotsToBreach is { } n ? $"PRACTICABLE after {n} shots" : "none")}")
            .AppendLine()
            .AppendLine($"[Space] fire  [C] camera: {_cameraRig.Mode}  [V] stress: {(_stressOverlay ? "on" : "off")}  [R] reset")
            .AppendLine("Shift = coarse steps. Orbit: right-drag, wheel. Barrel: wheel zooms.")
            .Append($"{Engine.GetFramesPerSecond():0} FPS, sim {_simulationMs:0.0} ms, {_siege.AwakeMemberCount()} awake members");
        _hud.SetStatus(status.ToString());
        _hud.SetRangeTable(RangeTableText());
        _hud.SetBanner(_gunBurst ? "THE BARREL BURST" : _shotsToBreach is not null ? "PRACTICABLE BREACH" : "");
    }

    private string? _rangeTableKey;
    private string _rangeTableText = "";

    /// <summary>Still-air table for the current charge and shot (02-visual-style: no trajectory preview).</summary>
    private string RangeTableText()
    {
        var key = $"{_charge}|{_shotType}";
        if (key == _rangeTableKey) return _rangeTableText;
        var table = RangeTable.Build(_gun, _powder, _shotTypes[_shotType], _charge, Atmosphere.Standard, DragModel.Sphere.Instance, 0.5);
        var text = new StringBuilder($"RANGE TABLE  {_charge:0.00} kg, {_shotTypes[_shotType].Name}\nstill air, flat ground\n\n elev   range   time\n");
        foreach (var entry in table.Where(e => e.ElevationDegrees <= 6 || e.ElevationDegrees % 2 == 0))
            text.AppendLine($"{entry.ElevationDegrees,5:0.0}° {entry.Range,6:0} m {entry.FlightTime,5:0.0} s");
        _rangeTableKey = key;
        _rangeTableText = text.ToString();
        return _rangeTableText;
    }

    private void ParseDebugArguments()
    {
        foreach (var argument in OS.GetCmdlineUserArgs())
        {
            var parts = argument.TrimStart('-').Split('=', 2);
            if (parts.Length != 2) continue;
            switch (parts[0])
            {
                case "screenshot": _screenshotPath = parts[1]; break;
                case "frames": _screenshotFrames = int.Parse(parts[1]); break;
                case "auto-fire": _autoFire = int.Parse(parts[1]); break;
                case "target": _target = int.Parse(parts[1]); break;
                case "stress": _stressOverlay = parts[1] == "on"; break;
                case "camera": _initialCamera = parts[1]; break;
            }
        }
    }

    private void SetCamera(string mode)
    {
        while (_cameraRig.Mode.ToString().ToLowerInvariant() != mode.ToLowerInvariant()) _cameraRig.Cycle();
        ApplyCameraMode();
    }

    /// <summary>Auto-aims and fires, then saves a screenshot and quits. For automated checks only.</summary>
    private void DebugCapture()
    {
        if (_screenshotPath is null) return;
        _frame++;
        if (_autoFire > 0 && _flight is null && _frame % 45 == 0)
        {
            var target = Targets[_target];
            _elevation = Gunnery.ElevationFor(_siege.Ballistics, CurrentShot(), target.AimPoint) ?? 0;
            Fire();
            _autoFire--;
        }
        if (_frame < _screenshotFrames) return;
        GetViewport().GetTexture().GetImage().SavePng(_screenshotPath);
        GD.Print($"screenshot saved: {_screenshotPath}");
        GetTree().Quit();
    }
}
