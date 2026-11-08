using BreachAndBastion.Core.Structure;
using Godot;

namespace BreachAndBastion.Game;

/// <summary>Renders a Core <see cref="Fortress"/>; rebuilds only the slices that moved.</summary>
public partial class FortressView : Node3D
{
    private Fortress _fortress = null!;
    private MeshInstance3D[][] _slices = [];
    private bool[][] _wasAwake = [];
    private bool _stressOverlay;
    private Material _material = null!;
    private Material _overlayMaterial = null!;

    public Fortress Fortress => _fortress;

    public bool StressOverlay
    {
        get => _stressOverlay;
        set
        {
            if (_stressOverlay == value) return;
            _stressOverlay = value;
            foreach (var segment in _slices)
                foreach (var instance in segment) instance.MaterialOverride = value ? _overlayMaterial : _material;
            Refresh(force: true);
        }
    }

    public static FortressView Create(Fortress fortress, Material material, Material overlayMaterial)
    {
        var view = new FortressView
        {
            Name = fortress.Spec.Id, _fortress = fortress, _material = material, _overlayMaterial = overlayMaterial,
        };
        view._slices = new MeshInstance3D[fortress.Segments.Count][];
        view._wasAwake = new bool[fortress.Segments.Count][];
        for (var s = 0; s < fortress.Segments.Count; s++)
        {
            var count = fortress.Segments[s].Slices.Count;
            view._slices[s] = new MeshInstance3D[count];
            view._wasAwake[s] = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var instance = new MeshInstance3D { MaterialOverride = material };
                view._slices[s][i] = instance;
                view.AddChild(instance);
            }
        }
        view.Refresh(force: true);
        return view;
    }

    /// <summary>Rebuilds slices that are awake, or fell asleep since the last call.</summary>
    public void Refresh(bool force = false)
    {
        for (var s = 0; s < _fortress.Segments.Count; s++)
        {
            var segment = _fortress.Segments[s];
            for (var i = 0; i < segment.Slices.Count; i++)
            {
                var awake = segment.Slices[i].Structure.Awake;
                if (force || awake || _wasAwake[s][i])
                    _slices[s][i].Mesh = SliceMeshFactory.Build(segment, i, _stressOverlay);
                _wasAwake[s][i] = awake;
            }
        }
    }

    /// <summary>Section view: show only one slice. Pass segment -1 to show everything.</summary>
    public void Isolate(int segment, int slice)
    {
        for (var s = 0; s < _slices.Length; s++)
            for (var i = 0; i < _slices[s].Length; i++)
                _slices[s][i].Visible = segment < 0 || (s == segment && i == slice);
    }
}
