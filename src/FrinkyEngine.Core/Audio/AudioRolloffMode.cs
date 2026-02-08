namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Determines how 3D audio volume decays over distance.
/// </summary>
public enum AudioRolloffMode
{
    /// <summary>
    /// Linear distance falloff from min to max distance.
    /// </summary>
    Linear = 0,

    /// <summary>
    /// Logarithmic-style distance falloff.
    /// </summary>
    Logarithmic = 1,

    /// <summary>
    /// Reserved for curve-driven rolloff.
    /// </summary>
    CustomCurve = 2
}
