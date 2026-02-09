using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FrinkyEngine.Core.Rendering.Profiling;

/// <summary>
/// Central frame profiler that collects per-category timing via <see cref="Stopwatch"/>-based scopes
/// and stores a rolling history of <see cref="FrameSnapshot"/> instances.
/// Scopes are exclusive: entering a child scope pauses the parent so categories never double-count.
/// </summary>
public static class FrameProfiler
{
    public const int HistorySize = 240;
    private const int MaxSubTimings = 32;
    private const int MaxScopeDepth = 16;

    private static bool _enabled;
    private static readonly Stopwatch _frameStopwatch = new();
    private static readonly Stopwatch[] _categoryStopwatches = new Stopwatch[(int)ProfileCategory.Count];

    // Sub-category timing pool (re-used each frame)
    private static readonly Stopwatch[] _subStopwatches = new Stopwatch[MaxSubTimings];
    private static readonly ProfileCategory[] _subParents = new ProfileCategory[MaxSubTimings];
    private static readonly string[] _subNames = new string[MaxSubTimings];
    private static int _subCount;

    // Exclusive scope stack — tracks which category is active so we can pause/resume
    private static readonly int[] _scopeStack = new int[MaxScopeDepth]; // category index, or -1
    private static int _scopeDepth;

    // Ring buffer
    private static readonly FrameSnapshot[] _history = new FrameSnapshot[HistorySize];
    private static int _historyHead;
    private static int _historyCount;

    // GPU stats for current frame
    private static GpuFrameStats _currentGpuStats;

    // GPU info strings (queried once)
    private static string? _gpuVendor;
    private static string? _gpuRenderer;
    private static bool _gpuInfoQueried;

    static FrameProfiler()
    {
        for (int i = 0; i < _categoryStopwatches.Length; i++)
            _categoryStopwatches[i] = new Stopwatch();
        for (int i = 0; i < _subStopwatches.Length; i++)
            _subStopwatches[i] = new Stopwatch();
    }

    /// <summary>
    /// Enables or disables profiling. When disabled, <see cref="Scope"/> returns a zero-cost no-op.
    /// </summary>
    public static bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// GPU vendor string (e.g. "NVIDIA Corporation"). Queried once via OpenGL.
    /// </summary>
    public static string GpuVendor => _gpuVendor ?? "Unknown";

    /// <summary>
    /// GPU renderer string (e.g. "NVIDIA GeForce RTX 3080"). Queried once via OpenGL.
    /// </summary>
    public static string GpuRenderer => _gpuRenderer ?? "Unknown";

    /// <summary>
    /// Resets all category timers and starts the frame timer. Call at the start of each frame.
    /// </summary>
    public static void BeginFrame()
    {
        if (!_enabled) return;

        if (!_gpuInfoQueried)
        {
            QueryGpuInfo();
            _gpuInfoQueried = true;
        }

        _frameStopwatch.Restart();
        for (int i = 0; i < _categoryStopwatches.Length; i++)
            _categoryStopwatches[i].Reset();
        for (int i = 0; i < _subCount; i++)
            _subStopwatches[i].Reset();
        _subCount = 0;
        _scopeDepth = 0;
        _currentGpuStats = default;
    }

    /// <summary>
    /// Stops the frame timer, computes the snapshot, and pushes it into the ring buffer.
    /// Call at the end of each frame.
    /// </summary>
    public static void EndFrame()
    {
        if (!_enabled) return;

        _frameStopwatch.Stop();
        double totalMs = _frameStopwatch.Elapsed.TotalMilliseconds;

        var categoryMs = new double[(int)ProfileCategory.Count];
        for (int i = 0; i < categoryMs.Length; i++)
            categoryMs[i] = _categoryStopwatches[i].Elapsed.TotalMilliseconds;

        var subTimings = new SubCategoryTiming[_subCount];
        for (int i = 0; i < _subCount; i++)
            subTimings[i] = new SubCategoryTiming(_subParents[i], _subNames[i], _subStopwatches[i].Elapsed.TotalMilliseconds);

        var snapshot = new FrameSnapshot(categoryMs, totalMs, subTimings, _currentGpuStats);

        _history[_historyHead] = snapshot;
        _historyHead = (_historyHead + 1) % HistorySize;
        if (_historyCount < HistorySize)
            _historyCount++;

        _scopeDepth = 0;
    }

    /// <summary>
    /// Returns a <see cref="ProfileScope"/> that accumulates time into the given category.
    /// Entering a scope pauses any currently active scope; exiting resumes it.
    /// When profiling is disabled, returns a no-op scope.
    /// </summary>
    public static ProfileScope Scope(ProfileCategory category)
    {
        if (!_enabled)
            return default;

        PauseActiveScope();
        PushScope((int)category);

        var sw = _categoryStopwatches[(int)category];
        return new ProfileScope(sw, managed: true);
    }

    /// <summary>
    /// Returns a <see cref="ProfileScope"/> that accumulates time into both the parent category
    /// and a named sub-timing entry (e.g. per post-process effect).
    /// </summary>
    public static ProfileScope ScopeNamed(ProfileCategory parent, string name)
    {
        if (!_enabled)
            return default;

        PauseActiveScope();
        PushScope((int)parent);

        var categorySw = _categoryStopwatches[(int)parent];
        categorySw.Start();

        if (_subCount < MaxSubTimings)
        {
            int idx = _subCount++;
            _subParents[idx] = parent;
            _subNames[idx] = name;
            _subStopwatches[idx].Reset();
            return new ProfileScope(_subStopwatches[idx], categorySw, managed: true);
        }

        // Overflow: just track the parent
        return new ProfileScope(null, categorySw, managed: true);
    }

    /// <summary>
    /// Pops the scope stack and resumes the previous scope. Called by <see cref="ProfileScope.Dispose"/>.
    /// </summary>
    internal static void PopScope()
    {
        if (_scopeDepth > 0)
            _scopeDepth--;

        // Resume previous active scope
        if (_scopeDepth > 0)
        {
            int prevCat = _scopeStack[_scopeDepth - 1];
            if (prevCat >= 0)
                _categoryStopwatches[prevCat].Start();
        }
    }

    /// <summary>
    /// Reports GPU-related stats for the current frame.
    /// </summary>
    public static void ReportGpuStats(GpuFrameStats stats)
    {
        _currentGpuStats = stats;
    }

    /// <summary>
    /// Returns the history buffer as a span, oldest first.
    /// </summary>
    public static ReadOnlySpan<FrameSnapshot> GetHistory()
    {
        if (_historyCount == 0)
            return ReadOnlySpan<FrameSnapshot>.Empty;

        // Copy into a temporary array for correct ordering (oldest first)
        var ordered = new FrameSnapshot[_historyCount];
        if (_historyCount < HistorySize)
        {
            Array.Copy(_history, 0, ordered, 0, _historyCount);
        }
        else
        {
            int start = _historyHead;
            for (int i = 0; i < HistorySize; i++)
                ordered[i] = _history[(start + i) % HistorySize];
        }
        return ordered;
    }

    /// <summary>
    /// Returns the most recent frame snapshot.
    /// </summary>
    public static FrameSnapshot GetLatest()
    {
        if (_historyCount == 0)
            return default;
        int idx = (_historyHead - 1 + HistorySize) % HistorySize;
        return _history[idx];
    }

    /// <summary>
    /// Total number of frames collected.
    /// </summary>
    public static int FrameCount => _historyCount;

    private static void PauseActiveScope()
    {
        if (_scopeDepth > 0)
        {
            int activeCat = _scopeStack[_scopeDepth - 1];
            if (activeCat >= 0)
                _categoryStopwatches[activeCat].Stop();
        }
    }

    private static void PushScope(int categoryIndex)
    {
        if (_scopeDepth < MaxScopeDepth)
            _scopeStack[_scopeDepth++] = categoryIndex;
    }

    private static void QueryGpuInfo()
    {
        try
        {
            const uint GL_VENDOR = 0x1F00;
            const uint GL_RENDERER = 0x1F01;

            var vendorPtr = GlGetString(GL_VENDOR);
            var rendererPtr = GlGetString(GL_RENDERER);

            if (vendorPtr != IntPtr.Zero)
                _gpuVendor = Marshal.PtrToStringAnsi(vendorPtr);
            if (rendererPtr != IntPtr.Zero)
                _gpuRenderer = Marshal.PtrToStringAnsi(rendererPtr);
        }
        catch
        {
            // Silently fail — GPU info is non-critical
        }
    }

    [DllImport("opengl32.dll", EntryPoint = "glGetString")]
    private static extern IntPtr GlGetString(uint name);
}
