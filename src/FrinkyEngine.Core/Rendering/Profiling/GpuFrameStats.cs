namespace FrinkyEngine.Core.Rendering.Profiling;

/// <summary>
/// GPU-related metrics captured once per frame.
/// </summary>
public readonly struct GpuFrameStats(int drawCalls, int postProcessPasses, long rtMemoryBytes)
{
    public int DrawCalls { get; } = drawCalls;
    public int PostProcessPasses { get; } = postProcessPasses;
    public long RtMemoryBytes { get; } = rtMemoryBytes;
}
