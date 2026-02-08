namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Opaque handle to a playing voice managed by the audio engine.
/// </summary>
/// <param name="Id">Unique voice identifier.</param>
/// <param name="Generation">Handle generation used for stale-handle safety.</param>
public readonly record struct AudioHandle(int Id, int Generation)
{
    /// <summary>
    /// Invalid handle value.
    /// </summary>
    public static AudioHandle Invalid => new(0, 0);

    /// <summary>
    /// Returns <c>true</c> when this handle references a potentially live voice.
    /// </summary>
    public bool IsValid => Id > 0 && Generation > 0;
}
