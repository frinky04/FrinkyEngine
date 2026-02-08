namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Optional playback overrides used when spawning an audio voice.
/// </summary>
public struct AudioPlayParams
{
    /// <summary>
    /// Creates playback params with engine defaults.
    /// </summary>
    public AudioPlayParams()
    {
        Volume = 1f;
        Pitch = 1f;
        Looping = false;
        Spatialized = false;
        Bus = AudioBusId.Sfx;
        AttenuationOverride = null;
        StartTimeSeconds = 0f;
        FadeInSeconds = 0f;
        Priority = 100;
    }

    /// <summary>
    /// Per-voice volume multiplier.
    /// </summary>
    public float Volume { get; set; } = 1f;

    /// <summary>
    /// Per-voice pitch multiplier.
    /// </summary>
    public float Pitch { get; set; } = 1f;

    /// <summary>
    /// Whether the voice should loop.
    /// </summary>
    public bool Looping { get; set; }

    /// <summary>
    /// Whether playback should be spatialized in 3D.
    /// </summary>
    public bool Spatialized { get; set; }

    /// <summary>
    /// Destination bus.
    /// </summary>
    public AudioBusId Bus { get; set; } = AudioBusId.Sfx;

    /// <summary>
    /// Optional attenuation override.
    /// </summary>
    public AudioAttenuationSettings? AttenuationOverride { get; set; }

    /// <summary>
    /// Start offset in seconds (streamed clips only).
    /// </summary>
    public float StartTimeSeconds { get; set; }

    /// <summary>
    /// Fade-in time in seconds.
    /// </summary>
    public float FadeInSeconds { get; set; }

    /// <summary>
    /// Priority used by voice stealing.
    /// </summary>
    public int Priority { get; set; } = 100;
}
