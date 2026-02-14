namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Manages timer invocations for a single component.
/// </summary>
internal class TimerRunner
{
    private readonly List<TimerEntry> _timers = new();

    /// <summary>
    /// Whether any timers are currently active.
    /// </summary>
    internal bool HasTimers => _timers.Count > 0;

    /// <summary>
    /// Schedules a one-shot callback after a delay.
    /// </summary>
    internal void Invoke(Action callback, float delay)
    {
        _timers.Add(new TimerEntry(callback, delay, 0f, false));
    }

    /// <summary>
    /// Schedules a repeating callback with an initial delay and interval.
    /// </summary>
    internal void InvokeRepeating(Action callback, float delay, float interval)
    {
        _timers.Add(new TimerEntry(callback, delay, interval, true));
    }

    /// <summary>
    /// Cancels all timers.
    /// </summary>
    internal void CancelAll()
    {
        foreach (var t in _timers)
            t.Cancelled = true;
        _timers.Clear();
    }

    /// <summary>
    /// Cancels all timers that reference a specific callback.
    /// </summary>
    internal void Cancel(Action callback)
    {
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            if (_timers[i].Callback == callback)
            {
                _timers[i].Cancelled = true;
                _timers.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Ticks all active timers, firing callbacks as needed. Uses scaled time.
    /// </summary>
    internal void Tick(float scaledDt)
    {
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            var timer = _timers[i];
            if (timer.Cancelled)
            {
                _timers.RemoveAt(i);
                continue;
            }

            timer.Remaining -= scaledDt;
            if (timer.Remaining <= 0f)
            {
                timer.Callback();

                if (timer.Repeating && !timer.Cancelled)
                {
                    timer.Remaining += timer.Interval;
                }
                else
                {
                    _timers.RemoveAt(i);
                }
            }
        }
    }
}
