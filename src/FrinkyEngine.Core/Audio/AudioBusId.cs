namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Identifies mixer buses used for routing and volume control.
/// </summary>
public enum AudioBusId
{
    /// <summary>
    /// Master output bus.
    /// </summary>
    Master = 0,

    /// <summary>
    /// Music bus.
    /// </summary>
    Music = 1,

    /// <summary>
    /// Sound effects bus.
    /// </summary>
    Sfx = 2,

    /// <summary>
    /// User interface bus.
    /// </summary>
    Ui = 3,

    /// <summary>
    /// Voice/dialog bus.
    /// </summary>
    Voice = 4,

    /// <summary>
    /// Ambient bus.
    /// </summary>
    Ambient = 5
}
