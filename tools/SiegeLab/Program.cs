// SiegeLab: headless experiments on the Core simulation, for tuning data (not shipped).
//   dotnet run --project tools/SiegeLab -- stand <profile>   which members fail under self-weight
//   dotnet run --project tools/SiegeLab -- breach [shots]    tall wall vs bastion, same gun
using BreachAndBastion.Core.Ballistics;
using BreachAndBastion.Core.Data;
using BreachAndBastion.Core.Mathematics;
using BreachAndBastion.Core.Siege;
using BreachAndBastion.Core.Structure;

var root = FindDataRoot();
var materials = DataLoader.LoadAll<MaterialSpec>(Path.Combine(root, "materials"), m => m.Id);
var profiles = DataLoader.LoadAll<ProfileSpec>(Path.Combine(root, "profiles"), p => p.Id);
var fortresses = DataLoader.LoadAll<FortressSpec>(Path.Combine(root, "fortresses"), f => f.Id);
var gun = DataLoader.LoadAll<GunSpec>(Path.Combine(root, "guns"), g => g.Id)["culverin"];
var powder = DataLoader.LoadAll<PowderSpec>(Path.Combine(root, "powders"), p => p.Id)["serpentine"];
var ball = DataLoader.LoadAll<ProjectileSpec>(Path.Combine(root, "projectiles"), p => p.Id)["iron-ball"];

switch (args.FirstOrDefault())
{
    case "stand":
        Stand(profiles[args[1]]);
        break;
    case "breach":
        var shots = args.Length > 1 ? int.Parse(args[1]) : 60;
        Breach("tall-wall", new Vec3(0, 1, 0), new Vec3(0, 4, 300), shots);
        Breach("bastion", new Vec3(200, 1, 0), new Vec3(190, 3, 302.5), shots);
        // Worst case for the bastion: a battery placed square to the face.
        Breach("bastion", new Vec3(-44.3, 1, 115.1), new Vec3(190, 3, 302.5), shots);
        break;
    default:
        Console.WriteLine("usage: stand <profile> | breach [shots]");
        break;
}

void Stand(ProfileSpec profile)
{
    var slice = new SliceStructure(SliceMeshBuilder.Build(profile, materials, 2));
    slice.Settle();
    var worst = Enumerable.Range(0, slice.Mesh.Members.Count).OrderByDescending(j => Math.Abs(slice.StressRatio(j))).Take(10);
    Console.WriteLine($"{profile.Id}: substep {slice.Substep * 1000:0.000} ms, {slice.Mesh.Members.Count} members");
    foreach (var j in worst) Describe(slice, j);
    slice.Wake();
    slice.Advance(1);
    Console.WriteLine($"broken after 1 s: {slice.BrokenMemberCount}");
    foreach (var j in Enumerable.Range(0, slice.Mesh.Members.Count).Where(slice.IsBroken).Take(10)) Describe(slice, j);
}

void Describe(SliceStructure slice, int j)
{
    var m = slice.Mesh.Members[j];
    var a = slice.Mesh.Nodes[m.A];
    var b = slice.Mesh.Nodes[m.B];
    Console.WriteLine($"  #{j,4} {slice.Mesh.Materials[m.Material].Id,-14} ({a.X:0.00},{a.Y:0.00})-({b.X:0.00},{b.Y:0.00}) " +
                      $"stress {slice.Stress(j) / 1000,9:0.0} kPa ratio {slice.StressRatio(j),6:0.00} broken {slice.IsBroken(j)}");
}

void Breach(string fortressId, Vec3 pivot, Vec3 target, int shots)
{
    var fortress = Fortress.Build(fortresses[fortressId], profiles, materials);
    var siege = new SiegeSimulation([fortress], Atmosphere.Standard, DragModel.Sphere.Instance, seed: 1);
    var template = new Shot(gun, powder, ball, pivot, 5, 0, 0);
    var elevation = Gunnery.ElevationFor(siege.Ballistics, template, target)!.Value;
    var shot = template with { ElevationDegrees = elevation, AzimuthDegrees = Gunnery.AzimuthTowards(pivot, target) };
    var watch = System.Diagnostics.Stopwatch.StartNew();

    Console.WriteLine($"== {fortressId}: elevation {elevation:0.00}°");
    for (var i = 1; i <= shots; i++)
    {
        var result = siege.Fire(shot);
        var hit = result.Hit;
        var settle = 0;
        do { siege.Advance(1.0 / 60); settle++; } while (siege.AnyAwake() && settle < 600);

        if (hit is null) { Console.WriteLine($"shot {i,3}: miss"); continue; }
        var slice = hit.Fortress.Segments[hit.Segment].Slices[hit.Slice];
        var practicable = SiegeSimulation.IsPracticable(slice);
        Console.WriteLine($"shot {i,3}: seg {hit.Segment} slice {hit.Slice,2} h {hit.Height,5:0.0} obl {hit.ObliquityDegrees,4:0}° " +
                          $"broken {slice.Structure.BrokenMemberCount,4} crest {slice.Structure.CrestHeight(),5:0.0} " +
                          $"rubble {slice.Rubble.HeightAtFace,4:0.0}{(practicable ? "  PRACTICABLE BREACH" : "")}");
        if (practicable) break;
    }
    Console.WriteLine($"   {watch.Elapsed.TotalSeconds:0.0} s wall time");
}

static string FindDataRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "data", "materials"))) dir = dir.Parent;
    return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("data/ not found"), "data");
}
