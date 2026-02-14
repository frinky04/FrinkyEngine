namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Represents a pending timer invocation (one-shot or repeating).
/// </summary>
internal class TimerEntry
{
    internal Action Callback { get; }
    internal float Remaining { get; set; }
    internal float Interval { get; }
    internal bool Repeating { get; }
    internal bool Cancelled { get; set; }

    internal TimerEntry(Action callback, float delay, float interval, bool repeating)
    {
        Callback = callback;
        Remaining = delay;
        Interval = interval;
        Repeating = repeating;
    }
}
