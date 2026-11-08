namespace BreachAndBastion.Core.Structure;

/// <summary>
/// Parametric wall cross-section (04-siege: no freeform editing in the MVP), loaded from
/// <c>data/profiles/*.json</c>. Slice coordinates: x is depth into the wall from the outer
/// toe (x = 0), y is height above the foundation.
/// </summary>
/// <param name="Height">Height of the revetment and the earth core, m.</param>
/// <param name="RevetmentBaseThickness">Masonry thickness at the foot, m.</param>
/// <param name="RevetmentTopThickness">Masonry thickness at the top, m. The outer face is battered; the inner face is vertical.</param>
/// <param name="EarthThickness">Earth core behind the revetment, m. Zero for a free-standing wall.</param>
/// <param name="ParapetHeight">Parapet height above <paramref name="Height"/>, m. Zero for none.</param>
/// <param name="ParapetThickness">Parapet thickness from the outer edge, m (snapped to the mesh).</param>
/// <param name="Spacing">Mesh spacing in masonry and vertically, m.</param>
/// <param name="EarthSpacing">Horizontal mesh spacing in the earth core, m.</param>
public sealed record ProfileSpec(
    string Id,
    string Name,
    double Height,
    double RevetmentBaseThickness,
    double RevetmentTopThickness,
    string RevetmentMaterial,
    double EarthThickness,
    string? EarthMaterial,
    double ParapetHeight,
    double ParapetThickness,
    string? ParapetMaterial,
    double Spacing = 0.5,
    double EarthSpacing = 1.0);
