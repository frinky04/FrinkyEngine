namespace FrinkyEngine.Core.Rendering.Profiling;

/// <summary>
/// Immutable snapshot of one frame's profiling data.
/// </summary>
public readonly struct FrameSnapshot
{
    private readonly double[] _categoryMs;

    public double TotalFrameMs { get; }
    public double OtherMs { get; }
    public SubCategoryTiming[] SubTimings { get; }
    public GpuFrameStats GpuStats { get; }

    internal FrameSnapshot(double[] categoryMs, double totalFrameMs, SubCategoryTiming[] subTimings, GpuFrameStats gpuStats)
    {
        _categoryMs = categoryMs;
        TotalFrameMs = totalFrameMs;
        SubTimings = subTimings;
        GpuStats = gpuStats;

        double sum = 0;
        for (int i = 0; i < (int)ProfileCategory.Count; i++)
            sum += categoryMs[i];
        OtherMs = Math.Max(0, totalFrameMs - sum);
    }

    public double GetCategoryMs(ProfileCategory cat)
    {
        if (_categoryMs == null) return 0;
        int index = (int)cat;
        return index >= 0 && index < _categoryMs.Length ? _categoryMs[index] : 0;
    }
}
