using System.Numerics;
using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Audio;

/// <summary>
/// High-level gameplay audio API inspired by modern engine static helper calls.
/// </summary>
public static class Audio
{
    private static AudioEngine? _engine;

    /// <summary>
    /// Plays a non-spatialized 2D sound.
    /// </summary>
    public static AudioHandle PlaySound2D(string soundPath, in AudioPlayParams playParams = default)
    {
        return _engine?.PlaySound2D(soundPath, in playParams) ?? AudioHandle.Invalid;
    }

    /// <summary>
    /// Plays a spatialized 3D sound at a world position.
    /// </summary>
    public static AudioHandle PlaySoundAtLocation(string soundPath, Vector3 worldPosition, in AudioPlayParams playParams = default)
    {
        return _engine?.PlaySoundAtLocation(soundPath, worldPosition, in playParams) ?? AudioHandle.Invalid;
    }

    /// <summary>
    /// Plays a spatialized 3D sound attached to an entity.
    /// </summary>
    public static AudioHandle SpawnSoundAttached(string soundPath, Entity attachTo, in AudioPlayParams playParams = default)
    {
        return _engine?.SpawnSoundAttached(soundPath, attachTo, in playParams) ?? AudioHandle.Invalid;
    }

    /// <summary>
    /// Stops a voice immediately or after a fade-out.
    /// </summary>
    public static bool Stop(AudioHandle handle, float fadeOutSeconds = 0f)
    {
        return _engine?.Stop(handle, fadeOutSeconds) ?? false;
    }

    /// <summary>
    /// Pauses or resumes a voice.
    /// </summary>
    public static bool SetPaused(AudioHandle handle, bool paused)
    {
        return _engine?.SetPaused(handle, paused) ?? false;
    }

    /// <summary>
    /// Sets per-voice volume.
    /// </summary>
    public static bool SetVolume(AudioHandle handle, float volume)
    {
        return _engine?.SetVolume(handle, volume) ?? false;
    }

    /// <summary>
    /// Sets per-voice pitch.
    /// </summary>
    public static bool SetPitch(AudioHandle handle, float pitch)
    {
        return _engine?.SetPitch(handle, pitch) ?? false;
    }

    /// <summary>
    /// Sets world position for a voice.
    /// </summary>
    public static bool SetWorldPosition(AudioHandle handle, Vector3 position)
    {
        return _engine?.SetWorldPosition(handle, position) ?? false;
    }

    /// <summary>
    /// Returns whether a voice is currently active.
    /// </summary>
    public static bool IsPlaying(AudioHandle handle)
    {
        return _engine?.IsPlaying(handle) ?? false;
    }

    /// <summary>
    /// Sets volume for a mixer bus.
    /// </summary>
    public static bool SetBusVolume(AudioBusId bus, float volume)
    {
        return _engine?.SetBusVolume(bus, volume) ?? false;
    }

    /// <summary>
    /// Sets mute for a mixer bus.
    /// </summary>
    public static bool SetBusMuted(AudioBusId bus, bool muted)
    {
        return _engine?.SetBusMuted(bus, muted) ?? false;
    }

    /// <summary>
    /// Gets current volume for a mixer bus.
    /// </summary>
    public static float GetBusVolume(AudioBusId bus)
    {
        return _engine?.GetBusVolume(bus) ?? AudioProjectSettings.Current.GetBusVolume(bus);
    }

    internal static void BindEngine(AudioEngine engine)
    {
        _engine = engine;
    }

    internal static void UnbindEngine(AudioEngine engine)
    {
        if (ReferenceEquals(_engine, engine))
            _engine = null;
    }

    internal static bool ConfigureHandle(
        AudioHandle handle,
        AudioBusId bus,
        bool spatialized,
        AudioAttenuationSettings attenuation,
        int priority,
        bool looping)
    {
        return _engine?.ConfigureHandle(handle, bus, spatialized, attenuation, priority, looping) ?? false;
    }
}
