namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// Suspends a coroutine until the supplied predicate returns <c>true</c>.
/// </summary>
public class WaitUntil : YieldInstruction
{
    private readonly Func<bool> _predicate;

    /// <summary>
    /// Creates a new wait instruction that resumes when <paramref name="predicate"/> returns <c>true</c>.
    /// </summary>
    /// <param name="predicate">The condition to evaluate each frame.</param>
    public WaitUntil(Func<bool> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    internal override bool IsReady(float scaledDt, float unscaledDt)
    {
        return _predicate();
    }
}
