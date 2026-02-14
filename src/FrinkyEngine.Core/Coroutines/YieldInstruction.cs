namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Base class for all coroutine yield instructions.
/// Subclasses define when a coroutine should resume execution.
/// </summary>
public abstract class YieldInstruction
{
    /// <summary>
    /// Returns <c>true</c> when the yield condition is satisfied and the coroutine should resume.
    /// </summary>
    /// <param name="scaledTime">Scaled delta time for this frame.</param>
    /// <param name="unscaledDeltaTime">Unscaled delta time for this frame.</param>
    internal abstract bool IsReady(float scaledTime, float unscaledDeltaTime);
}
