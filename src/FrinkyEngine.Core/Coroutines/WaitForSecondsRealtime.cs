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
    /// <param name="seconds">Duration to wait in real (unscaled) seconds.</param>
    public WaitForSecondsRealtime(float seconds)
    {
        _remaining = seconds;
    }

    internal override bool IsReady(float scaledTime, float unscaledDeltaTime)
    {
        _remaining -= unscaledDeltaTime;
        return _remaining <= 0f;
    }
}
