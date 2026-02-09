namespace FrinkyEngine.Core.Rendering.Profiling;

/// <summary>
/// A named timing entry within a parent <see cref="ProfileCategory"/>, used for per-effect detail.
/// </summary>
public readonly struct SubCategoryTiming(ProfileCategory parent, string name, double elapsedMs)
{
    public ProfileCategory Parent { get; } = parent;
    public string Name { get; } = name;
    public double ElapsedMs { get; } = elapsedMs;
}
