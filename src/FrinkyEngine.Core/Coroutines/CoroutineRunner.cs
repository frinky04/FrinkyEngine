using System.Collections;

namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Manages coroutines for a single component. Handles starting, stopping, pausing, and ticking.
/// </summary>
internal class CoroutineRunner
{
    private readonly List<Coroutine> _coroutines = new();

    /// <summary>
    /// Whether any coroutines are currently active.
    /// </summary>
    internal bool HasCoroutines => _coroutines.Count > 0;

    /// <summary>
    /// Starts a new coroutine and returns a handle to it.
    /// </summary>
    internal Coroutine Start(IEnumerator enumerator)
    {
        var coroutine = new Coroutine(enumerator);
        _coroutines.Add(coroutine);
        return coroutine;
    }

    /// <summary>
    /// Stops a specific coroutine.
    /// </summary>
    internal void Stop(Coroutine coroutine)
    {
        coroutine.IsFinished = true;
    }

    /// <summary>
    /// Stops all running coroutines.
    /// </summary>
    internal void StopAll()
    {
        foreach (var c in _coroutines)
            c.IsFinished = true;
        _coroutines.Clear();
    }

    /// <summary>
    /// Pauses all coroutines (e.g. when the component is disabled).
    /// </summary>
    internal void PauseAll()
    {
        foreach (var c in _coroutines)
            c.IsPaused = true;
    }

    /// <summary>
    /// Resumes all paused coroutines (e.g. when the component is re-enabled).
    /// </summary>
    internal void ResumeAll()
    {
        foreach (var c in _coroutines)
            c.IsPaused = false;
    }

    /// <summary>
    /// Ticks all active coroutines, removing finished ones.
    /// </summary>
    internal void Tick(float scaledDt, float unscaledDt)
    {
        for (int i = _coroutines.Count - 1; i >= 0; i--)
        {
            var coroutine = _coroutines[i];
            if (!coroutine.Tick(scaledDt, unscaledDt))
            {
                _coroutines.RemoveAt(i);
            }
        }
    }
}
