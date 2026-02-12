namespace FrinkyEngine.Core.Rendering.Profiling;

/// <summary>
/// Categories of work tracked by the frame profiler.
/// </summary>
public enum ProfileCategory : byte
{
    Game,
    GameLate,
    Physics,
    Audio,
    Rendering,
    Skinning,
    PostProcessing,
    UI,
    Editor,
    Count
}
