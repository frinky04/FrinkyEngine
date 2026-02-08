namespace FrinkyEngine.Core.Components;

/// <summary>
/// Controls how visual interpolation is applied to this rigidbody.
/// </summary>
public enum RigidbodyInterpolationMode
{
    /// <summary>
    /// Uses project-level interpolation settings.
    /// </summary>
    Inherit,

    /// <summary>
    /// Disables visual interpolation for this body.
    /// </summary>
    None,

    /// <summary>
    /// Forces visual interpolation for this body.
    /// </summary>
    Interpolate
}
