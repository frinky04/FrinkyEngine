namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Suspends a coroutine for the specified number of seconds using unscaled (real) time.
/// </summary>
public class WaitForSecondsRealtime : YieldInstruction
{
    private float _remaining;

    /// <summary>
    /// Creates a new wait instruction that pauses for <paramref name="seconds"/> of real time.
    /// </summary>
    /// <param name="seconds">Duration to wait in real (unscaled) seconds. Negative values are treated as zero.</param>
    public WaitForSecondsRealtime(float seconds)
    {
        _remaining = seconds < 0f ? 0f : seconds;
    }

    internal override bool IsReady(float scaledDt, float unscaledDt)
    {
        _remaining -= unscaledDt;
        return _remaining <= 0f;
    }
}
