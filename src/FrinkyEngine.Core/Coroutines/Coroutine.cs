using System.Collections;

namespace FrinkyEngine.Core.Coroutines;

/// <summary>
/// A handle to a running coroutine. Returned by <c>StartCoroutine</c> and used to stop it.
/// </summary>
public class Coroutine
{
    internal IEnumerator Enumerator { get; }
    internal bool IsFinished { get; set; }
    internal bool IsPaused { get; set; }
    internal YieldInstruction? CurrentYield { get; set; }

    internal Coroutine(IEnumerator enumerator)
    {
        Enumerator = enumerator;
    }

    /// <summary>
    /// Advances the coroutine by one tick. Returns <c>true</c> if the coroutine is still running.
    /// </summary>
    internal bool Tick(float scaledDt, float unscaledDt)
    {
        if (IsFinished || IsPaused)
            return !IsFinished;

        // If we have a pending yield instruction, check if it's ready
        if (CurrentYield != null)
        {
            if (!CurrentYield.IsReady(scaledDt, unscaledDt))
                return true; // Still waiting
            CurrentYield = null;
        }

        // Advance the enumerator
        if (!Enumerator.MoveNext())
        {
            IsFinished = true;
            return false;
        }

        // Process the yielded value
        var yielded = Enumerator.Current;
        if (yielded is YieldInstruction instruction)
        {
            CurrentYield = instruction;
        }
        // yield return null means resume next frame (no yield instruction set)

        return true;
    }
}
