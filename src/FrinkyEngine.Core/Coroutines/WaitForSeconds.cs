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
    /// <param name="seconds">Duration to wait in scaled seconds.</param>
    public WaitForSeconds(float seconds)
    {
        _remaining = seconds;
    }

    internal override bool IsReady(float scaledTime, float unscaledDeltaTime)
    {
        _remaining -= scaledTime;
        return _remaining <= 0f;
    }
}
