namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Per-frame audio diagnostics snapshot for profiling.
/// </summary>
public readonly record struct AudioFrameStats(
    bool Valid,
    int ActiveVoices,
    int VirtualizedVoices,
    int StolenVoicesThisFrame,
    int StreamingVoices,
    double UpdateTimeMs);
