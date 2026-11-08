namespace BreachAndBastion.Core.Structure;

/// <summary>A node that lost its connection to the ground and left the structure as rubble.</summary>
public readonly record struct DetachedNode(int Node, double X, double Y, double Mass, double VelocityX, double VelocityY);

/// <summary>
/// Moving state of one wall slice. Explicit dynamics (symplectic Euler) at a fixed substep
/// derived from the mesh, so results depend only on inputs (ADR 0006).
/// </summary>
public sealed class SliceStructure
{
    public const double Gravity = 9.81;

    /// <summary>Velocity damping while playing, 1/s (joints, air).</summary>
    public const double PlayDamping = 0.5;

    /// <summary>Velocity damping while settling under self-weight, 1/s.</summary>
    public const double SettleDamping = 20;

    private const double SleepSpeed = 0.005;
    private const double SleepAfterSeconds = 0.5;

    private readonly double[] _x, _y, _vx, _vy, _fx, _fy;
    private readonly double[] _stiffness, _dashpot, _damage, _stress;
    private readonly bool[] _broken, _detached;
    private double _quietTime;
    private bool _connectivityDirty;

    public SliceStructure(SliceMesh mesh)
    {
        Mesh = mesh;
        var n = mesh.Nodes.Count;
        var m = mesh.Members.Count;
        _x = new double[n]; _y = new double[n]; _vx = new double[n]; _vy = new double[n];
        _fx = new double[n]; _fy = new double[n];
        _stiffness = new double[m]; _dashpot = new double[m]; _damage = new double[m]; _stress = new double[m];
        _broken = new bool[m]; _detached = new bool[n];

        for (var i = 0; i < n; i++) { _x[i] = mesh.Nodes[i].X; _y[i] = mesh.Nodes[i].Y; }

        var stiffnessAtNode = new double[n];
        var dashpotAtNode = new double[n];
        for (var j = 0; j < m; j++)
        {
            var member = mesh.Members[j];
            var material = mesh.Materials[member.Material];
            _stiffness[j] = material.Stiffness * member.Area / member.RestLength;
            var mass = Math.Min(mesh.Nodes[member.A].Mass, mesh.Nodes[member.B].Mass);
            _dashpot[j] = 2 * material.DampingRatio * Math.Sqrt(_stiffness[j] * mass);
            stiffnessAtNode[member.A] += _stiffness[j]; stiffnessAtNode[member.B] += _stiffness[j];
            dashpotAtNode[member.A] += _dashpot[j]; dashpotAtNode[member.B] += _dashpot[j];
        }

        // Gershgorin bound on the highest natural frequency, plus the dashpot limit.
        var step = double.MaxValue;
        for (var i = 0; i < n; i++)
        {
            var node = mesh.Nodes[i];
            if (node.Fixed || node.Mass <= 0 || stiffnessAtNode[i] <= 0) continue;
            var omega = Math.Sqrt(2 * stiffnessAtNode[i] / node.Mass);
            step = Math.Min(step, 0.8 * 2 / omega);
            if (dashpotAtNode[i] > 0) step = Math.Min(step, 0.8 * node.Mass / dashpotAtNode[i]);
        }
        Substep = step == double.MaxValue ? 1.0 / 240 : step;
    }

    public SliceMesh Mesh { get; }

    /// <summary>Integration step, s. Frames are split into whole substeps of at most this length.</summary>
    public double Substep { get; }

    public bool Awake { get; private set; } = true;

    /// <summary>False while settling: self-weight must not count as battle damage.</summary>
    public bool DamageEnabled { get; private set; }

    public double X(int node) => _x[node];
    public double Y(int node) => _y[node];
    public double VelocityX(int node) => _vx[node];
    public double VelocityY(int node) => _vy[node];
    public bool IsBroken(int member) => _broken[member];
    public bool IsDetached(int node) => _detached[node];
    public double Damage(int member) => _damage[member];

    /// <summary>Axial stress of the last substep, Pa. Positive is tension.</summary>
    public double Stress(int member) => _stress[member];

    /// <summary>Stress as a fraction of the member's current (damaged) strength, signed like <see cref="Stress"/>.</summary>
    public double StressRatio(int member)
    {
        var material = Mesh.Materials[Mesh.Members[member].Material];
        var strength = (_stress[member] >= 0 ? material.TensileStrength : material.CompressiveStrength) * (1 - _damage[member]);
        return strength > 0 ? _stress[member] / strength : 0;
    }

    public int BrokenMemberCount => _broken.Count(b => b);

    public double KineticEnergy()
    {
        var energy = 0.0;
        for (var i = 0; i < _x.Length; i++)
            if (!_detached[i]) energy += 0.5 * Mesh.Nodes[i].Mass * (_vx[i] * _vx[i] + _vy[i] * _vy[i]);
        return energy;
    }

    /// <summary>Lets the slice sag under its own weight with heavy damping, then enables damage.</summary>
    public void Settle(double seconds = 3)
    {
        DamageEnabled = false;
        var detached = new List<DetachedNode>();
        RunSubsteps(seconds, SettleDamping, detached);
        for (var i = 0; i < _vx.Length; i++) { _vx[i] = 0; _vy[i] = 0; }
        DamageEnabled = true;
        _quietTime = 0;
        Awake = false;
    }

    /// <summary>Copies the moving state of an identical slice (same mesh), e.g. a settled prototype.</summary>
    public void CopyStateFrom(SliceStructure other)
    {
        if (!ReferenceEquals(other.Mesh, Mesh)) throw new ArgumentException("Slices must share a mesh.");
        Array.Copy(other._x, _x, _x.Length); Array.Copy(other._y, _y, _y.Length);
        Array.Copy(other._vx, _vx, _vx.Length); Array.Copy(other._vy, _vy, _vy.Length);
        Array.Copy(other._damage, _damage, _damage.Length); Array.Copy(other._stress, _stress, _stress.Length);
        Array.Copy(other._broken, _broken, _broken.Length); Array.Copy(other._detached, _detached, _detached.Length);
        Awake = other.Awake; DamageEnabled = other.DamageEnabled; _quietTime = other._quietTime;
        _connectivityDirty = other._connectivityDirty;
    }

    /// <summary>
    /// Pushes the nodes within <paramref name="radius"/> of (x, y) so that their total momentum
    /// changes by (jx, jy), weighted towards the centre. Wakes the slice.
    /// </summary>
    public void ApplyImpulse(double x, double y, double jx, double jy, double radius)
    {
        var weights = new List<(int Node, double Weight)>();
        var weightedMass = 0.0;
        for (var i = 0; i < _x.Length; i++)
        {
            if (_detached[i] || Mesh.Nodes[i].Fixed) continue;
            var d = Math.Sqrt((_x[i] - x) * (_x[i] - x) + (_y[i] - y) * (_y[i] - y));
            if (d >= radius) continue;
            var w = 1 - d / radius;
            weights.Add((i, w));
            weightedMass += w * Mesh.Nodes[i].Mass;
        }
        if (weightedMass <= 0) return;

        foreach (var (node, w) in weights)
        {
            _vx[node] += jx * w / weightedMass;
            _vy[node] += jy * w / weightedMass;
        }
        Wake();
    }

    /// <summary>
    /// Cuts a shot's crater: <paramref name="energy"/> (J, face-normal kinetic energy) is shared
    /// between intact members whose midpoints lie within <paramref name="radius"/> of (x, y),
    /// weighted towards the centre. Each member's damage rises by its share divided by the
    /// energy needed to crush its volume; fully damaged members break. Wakes the slice.
    /// </summary>
    public void ApplyCrushing(double x, double y, double energy, double radius)
    {
        var hits = new List<(int Member, double Weight)>();
        var totalWeight = 0.0;
        for (var j = 0; j < _broken.Length; j++)
        {
            if (_broken[j]) continue;
            var member = Mesh.Members[j];
            var mx = (_x[member.A] + _x[member.B]) / 2;
            var my = (_y[member.A] + _y[member.B]) / 2;
            var d = Math.Sqrt((mx - x) * (mx - x) + (my - y) * (my - y));
            if (d >= radius) continue;
            var w = 1 - d / radius;
            hits.Add((j, w));
            totalWeight += w;
        }
        if (totalWeight <= 0) return;

        foreach (var (j, w) in hits)
        {
            var member = Mesh.Members[j];
            var volume = member.Area * member.RestLength;
            var share = energy * w / totalWeight;
            _damage[j] = Math.Min(1, _damage[j] + share / (Mesh.Materials[member.Material].CrushEnergy * volume));
            if (_damage[j] >= 1)
            {
                _broken[j] = true;
                _connectivityDirty = true;
            }
        }
        Wake();
    }

    public void Wake()
    {
        Awake = true;
        _quietTime = 0;
    }

    /// <summary>Advances by <paramref name="seconds"/>; returns nodes that fell off as rubble.</summary>
    public IReadOnlyList<DetachedNode> Advance(double seconds)
    {
        var detached = new List<DetachedNode>();
        if (!Awake) return detached;
        if (_connectivityDirty)
        {
            detached.AddRange(DetachUnsupported());
            _connectivityDirty = false;
        }
        RunSubsteps(seconds, PlayDamping, detached);

        var maxSpeedSquared = 0.0;
        for (var i = 0; i < _x.Length; i++)
            if (!_detached[i]) maxSpeedSquared = Math.Max(maxSpeedSquared, _vx[i] * _vx[i] + _vy[i] * _vy[i]);
        _quietTime = maxSpeedSquared < SleepSpeed * SleepSpeed ? _quietTime + seconds : 0;
        if (_quietTime >= SleepAfterSeconds) Awake = false;
        return detached;
    }

    private void RunSubsteps(double seconds, double damping, List<DetachedNode> detached)
    {
        var count = (int)Math.Ceiling(seconds / Substep - 1e-9);
        if (count <= 0) return;
        var h = seconds / count;
        for (var s = 0; s < count; s++)
        {
            if (Integrate(h, damping)) detached.AddRange(DetachUnsupported());
        }
    }

    /// <summary>One integration step. Returns true if a member broke.</summary>
    private bool Integrate(double h, double damping)
    {
        var nodes = Mesh.Nodes;
        for (var i = 0; i < _x.Length; i++) { _fx[i] = 0; _fy[i] = -Gravity * nodes[i].Mass; }

        var anyBroken = false;
        for (var j = 0; j < _stiffness.Length; j++)
        {
            if (_broken[j]) continue;
            var member = Mesh.Members[j];
            int a = member.A, b = member.B;
            var dx = _x[b] - _x[a];
            var dy = _y[b] - _y[a];
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length <= 0) continue;
            var ux = dx / length;
            var uy = dy / length;

            var elastic = _stiffness[j] * (length - member.RestLength);
            var closing = (_vx[b] - _vx[a]) * ux + (_vy[b] - _vy[a]) * uy;
            var force = elastic + _dashpot[j] * closing;
            _fx[a] += force * ux; _fy[a] += force * uy;
            _fx[b] -= force * ux; _fy[b] -= force * uy;

            var stress = elastic / member.Area;
            _stress[j] = stress;
            if (DamageEnabled && UpdateDamage(j, stress, h))
            {
                _broken[j] = true;
                anyBroken = true;
            }
        }

        var keep = 1 - damping * h;
        for (var i = 0; i < _x.Length; i++)
        {
            if (nodes[i].Fixed || _detached[i]) continue;
            _vx[i] = (_vx[i] + _fx[i] / nodes[i].Mass * h) * keep;
            _vy[i] = (_vy[i] + _fy[i] / nodes[i].Mass * h) * keep;
            _x[i] += _vx[i] * h;
            _y[i] += _vy[i] * h;
        }
        return anyBroken;
    }

    /// <summary>Accumulates damage above the onset ratio; returns true when the member fails.</summary>
    private bool UpdateDamage(int member, double stress, double h)
    {
        var material = Mesh.Materials[Mesh.Members[member].Material];
        var strength = stress >= 0 ? material.TensileStrength : material.CompressiveStrength;
        var ratio = Math.Abs(stress) / strength;
        if (ratio > material.DamageOnset)
            _damage[member] = Math.Min(1, _damage[member] + material.DamageRate * (ratio - material.DamageOnset) * h);
        return ratio >= 1 - _damage[member];
    }

    /// <summary>Removes every node no longer connected to a fixed node through intact members.</summary>
    private List<DetachedNode> DetachUnsupported()
    {
        var supported = new bool[_x.Length];
        var queue = new Queue<int>();
        for (var i = 0; i < _x.Length; i++)
        {
            if (Mesh.Nodes[i].Fixed && !_detached[i]) { supported[i] = true; queue.Enqueue(i); }
        }
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var j in Mesh.MembersOfNode[node])
            {
                if (_broken[j]) continue;
                var member = Mesh.Members[j];
                var other = member.A == node ? member.B : member.A;
                if (supported[other] || _detached[other]) continue;
                supported[other] = true;
                queue.Enqueue(other);
            }
        }

        var result = new List<DetachedNode>();
        for (var i = 0; i < _x.Length; i++)
        {
            if (supported[i] || _detached[i]) continue;
            _detached[i] = true;
            foreach (var j in Mesh.MembersOfNode[i]) _broken[j] = true;
            result.Add(new DetachedNode(i, _x[i], _y[i], Mesh.Nodes[i].Mass, _vx[i], _vy[i]));
        }
        return result;
    }

    /// <summary>
    /// A cell is solid while all its corners are attached and at most one of its members is
    /// broken. Shots pass through non-solid cells.
    /// </summary>
    public bool IsCellSolid(int cell)
    {
        var c = Mesh.Cells[cell];
        foreach (var node in c.Nodes) if (_detached[node]) return false;
        var broken = 0;
        foreach (var member in c.Members) if (_broken[member]) broken++;
        return broken <= 1;
    }

    /// <summary>Index of the solid cell containing (x, y) in current coordinates, or -1.</summary>
    public int SolidCellAt(double x, double y)
    {
        for (var c = 0; c < Mesh.Cells.Count; c++)
        {
            if (IsCellSolid(c) && ContainsPoint(Mesh.Cells[c].Nodes, x, y)) return c;
        }
        return -1;
    }

    private bool ContainsPoint(int[] corners, double x, double y)
    {
        // Convex quad, counter-clockwise: inside if left of every edge.
        for (var k = 0; k < 4; k++)
        {
            int a = corners[k], b = corners[(k + 1) % 4];
            var cross = (_x[b] - _x[a]) * (y - _y[a]) - (_y[b] - _y[a]) * (x - _x[a]);
            if (cross < 0) return false;
        }
        return true;
    }

    /// <summary>Highest attached node, m: what an assault must climb over.</summary>
    public double CrestHeight()
    {
        var crest = 0.0;
        for (var i = 0; i < _y.Length; i++) if (!_detached[i]) crest = Math.Max(crest, _y[i]);
        return crest;
    }

    /// <summary>FNV-1a over positions and broken flags, for determinism tests.</summary>
    public ulong StateHash(ulong hash = 14695981039346656037UL)
    {
        static ulong Mix(ulong h, ulong v)
        {
            for (var k = 0; k < 8; k++) { h ^= (v >> (8 * k)) & 0xFF; h *= 1099511628211UL; }
            return h;
        }
        for (var i = 0; i < _x.Length; i++)
        {
            hash = Mix(hash, (ulong)BitConverter.DoubleToInt64Bits(_x[i]));
            hash = Mix(hash, (ulong)BitConverter.DoubleToInt64Bits(_y[i]));
        }
        for (var j = 0; j < _broken.Length; j++) hash = Mix(hash, _broken[j] ? 1UL : 0UL);
        return hash;
    }
}
