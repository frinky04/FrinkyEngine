using System.Diagnostics;

namespace FrinkyEngine.Core.Rendering.Profiling;

/// <summary>
/// Disposable scope that accumulates elapsed time into a profiler category.
/// Stack-only (<c>ref struct</c>) to avoid heap allocation.
/// Scopes are exclusive: entering a new scope pauses the previous one.
/// </summary>
public ref struct ProfileScope
{
    private readonly Stopwatch? _sw;
    private readonly Stopwatch? _subSw;
    private readonly bool _managed;

    /// <summary>
    /// Standard scope: stops <paramref name="sw"/> on dispose and pops the scope stack.
    /// </summary>
    internal ProfileScope(Stopwatch? sw, bool managed)
    {
        _sw = sw;
        _subSw = null;
        _managed = managed;
        _sw?.Start();
    }

    /// <summary>
    /// Named sub-scope: stops both <paramref name="subSw"/> and <paramref name="categorySw"/>
    /// on dispose and pops the scope stack. <paramref name="categorySw"/> is already started by the caller.
    /// </summary>
    internal ProfileScope(Stopwatch? subSw, Stopwatch? categorySw, bool managed)
    {
        _sw = categorySw;
        _subSw = subSw;
        _managed = managed;
        _subSw?.Start();
    }

    public void Dispose()
    {
        _subSw?.Stop();
        _sw?.Stop();
        if (_managed)
            FrameProfiler.PopScope();
    }
}
