namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Suspends a coroutine for the specified number of seconds using scaled time.
/// </summary>
public class WaitForSeconds : YieldInstruction
{
    private float _remaining;

    /// <summary>
    /// Creates a new wait instruction that pauses for <paramref name="seconds"/> of scaled time.
    /// </summary>
    /// <param name="seconds">Duration to wait in scaled seconds. Negative values are treated as zero.</param>
    public WaitForSeconds(float seconds)
    {
        _remaining = seconds < 0f ? 0f : seconds;
    }

    internal override bool IsReady(float scaledDt, float unscaledDt)
    {
        _remaining -= scaledDt;
        return _remaining <= 0f;
    }
}
