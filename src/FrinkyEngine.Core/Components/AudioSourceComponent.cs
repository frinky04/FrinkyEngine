using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Audio;
using FrinkyEngine.Core.ECS;
using AudioApi = FrinkyEngine.Core.Audio.Audio;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Plays an audio asset from an entity with optional 3D spatialization.
/// </summary>
[ComponentCategory("Audio")]
public class AudioSourceComponent : Component
{
    private AssetReference _soundPath = new("");
    private float _volume = 1f;
    private float _pitch = 1f;
    private float _startTimeSeconds;
    private int _priority = 100;
    private AudioAttenuationSettings _attenuation = AudioAttenuationSettings.Default3D;
    private AudioHandle _currentHandle = AudioHandle.Invalid;

    /// <summary>
    /// Asset-relative or absolute path to the sound file.
    /// </summary>
    [AssetFilter(AssetType.Audio)]
    [InspectorLabel("Sound")]
    public AssetReference SoundPath
    {
        get => _soundPath;
        set => _soundPath = new AssetReference(value.Path?.Trim() ?? "");
    }

    /// <summary>
    /// Automatically starts playback in <see cref="Start"/>.
    /// </summary>
    public bool PlayOnStart { get; set; }

    /// <summary>
    /// Loops the voice until explicitly stopped.
    /// </summary>
    public bool Looping { get; set; }

    /// <summary>
    /// Enables 3D spatialization.
    /// </summary>
    public bool Spatialized { get; set; } = true;

    /// <summary>
    /// Clears runtime handle state when one-shot playback ends.
    /// </summary>
    public bool AutoDestroyOnFinish { get; set; } = true;

    /// <summary>
    /// Automatically resumes playback when this component is re-enabled.
    /// </summary>
    public bool AutoResumeOnEnable { get; set; } = true;

    /// <summary>
    /// Mixer bus route for this source.
    /// </summary>
    public AudioBusId Bus { get; set; } = AudioBusId.Sfx;

    /// <summary>
    /// Per-source volume multiplier.
    /// </summary>
    public float Volume
    {
        get => _volume;
        set => _volume = float.IsFinite(value) ? Math.Clamp(value, 0f, 2f) : 1f;
    }

    /// <summary>
    /// Per-source pitch multiplier.
    /// </summary>
    public float Pitch
    {
        get => _pitch;
        set => _pitch = float.IsFinite(value) ? Math.Clamp(value, 0.01f, 3f) : 1f;
    }

    /// <summary>
    /// Start offset in seconds for streamed clips.
    /// </summary>
    [InspectorLabel("Start Time (s)")]
    public float StartTimeSeconds
    {
        get => _startTimeSeconds;
        set => _startTimeSeconds = float.IsFinite(value) ? MathF.Max(0f, value) : 0f;
    }

    /// <summary>
    /// Whether this source is muted.
    /// </summary>
    public bool Mute { get; set; }

    /// <summary>
    /// Whether this source is paused.
    /// </summary>
    public bool Paused { get; set; }

    /// <summary>
    /// Priority used by voice stealing.
    /// </summary>
    public int Priority
    {
        get => _priority;
        set => _priority = Math.Clamp(value, 0, 1000);
    }

    /// <summary>
    /// Spatial attenuation settings.
    /// </summary>
    [InspectorSection("Attenuation")]
    public AudioAttenuationSettings Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            _attenuation.Normalize();
        }
    }

    /// <summary>
    /// Current runtime handle for active playback, if any.
    /// </summary>
    public AudioHandle CurrentHandle => _currentHandle;

    /// <summary>
    /// True while this source has an active voice handle.
    /// </summary>
    public bool IsPlaying => _currentHandle.IsValid && AudioApi.IsPlaying(_currentHandle);

    /// <summary>
    /// Starts playback using current source settings.
    /// </summary>
    public void Play()
    {
        if (SoundPath.IsEmpty)
            return;

        Stop();

        var playParams = new AudioPlayParams
        {
            Volume = Volume,
            Pitch = Pitch,
            Looping = Looping,
            Spatialized = Spatialized,
            Bus = Bus,
            AttenuationOverride = Attenuation,
            StartTimeSeconds = StartTimeSeconds,
            Priority = Priority
        };

        _currentHandle = Spatialized
            ? AudioApi.SpawnSoundAttached(SoundPath.Path, Entity, in playParams)
            : AudioApi.PlaySound2D(SoundPath.Path, in playParams);

        if (!_currentHandle.IsValid)
            return;

        AudioApi.ConfigureHandle(_currentHandle, Bus, Spatialized, Attenuation, Priority, Looping);
        if (Mute || Paused)
            AudioApi.SetPaused(_currentHandle, true);
    }

    /// <summary>
    /// Stops playback.
    /// </summary>
    /// <param name="fadeOutSeconds">Optional fade-out duration in seconds.</param>
    public void Stop(float fadeOutSeconds = 0f)
    {
        if (!_currentHandle.IsValid)
            return;

        AudioApi.Stop(_currentHandle, fadeOutSeconds);
        _currentHandle = AudioHandle.Invalid;
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    public void Pause()
    {
        Paused = true;
        if (_currentHandle.IsValid)
            AudioApi.SetPaused(_currentHandle, true);
    }

    /// <summary>
    /// Resumes playback.
    /// </summary>
    public void Resume()
    {
        Paused = false;
        if (_currentHandle.IsValid && !Mute)
            AudioApi.SetPaused(_currentHandle, false);
    }

    /// <summary>
    /// Assigns a new sound path.
    /// </summary>
    public void SetSound(string soundPath)
    {
        SoundPath = soundPath;
    }

    /// <summary>
    /// Starts playback with fade-in.
    /// </summary>
    public void FadeIn(float seconds)
    {
        var clamped = float.IsFinite(seconds) ? MathF.Max(0f, seconds) : 0f;
        Stop();
        var playParams = new AudioPlayParams
        {
            Volume = Volume,
            Pitch = Pitch,
            Looping = Looping,
            Spatialized = Spatialized,
            Bus = Bus,
            AttenuationOverride = Attenuation,
            StartTimeSeconds = StartTimeSeconds,
            FadeInSeconds = clamped,
            Priority = Priority
        };

        _currentHandle = Spatialized
            ? AudioApi.SpawnSoundAttached(SoundPath.Path, Entity, in playParams)
            : AudioApi.PlaySound2D(SoundPath.Path, in playParams);
    }

    /// <summary>
    /// Stops playback with fade-out.
    /// </summary>
    public void FadeOut(float seconds)
    {
        Stop(seconds);
    }

    /// <inheritdoc />
    public override void Start()
    {
        if (PlayOnStart)
            Play();
    }

    /// <inheritdoc />
    public override void OnEnable()
    {
        if (AutoResumeOnEnable && _currentHandle.IsValid && !Paused && !Mute)
            AudioApi.SetPaused(_currentHandle, false);
    }

    /// <inheritdoc />
    public override void OnDisable()
    {
        PauseDueToInactiveState();
    }

    /// <inheritdoc />
    public override void OnDestroy()
    {
        Stop();
    }

    internal void PauseDueToInactiveState()
    {
        if (_currentHandle.IsValid)
            AudioApi.SetPaused(_currentHandle, true);
    }

    internal void SyncRuntimePlayback()
    {
        if (!_currentHandle.IsValid)
            return;

        if (!AudioApi.IsPlaying(_currentHandle))
        {
            _currentHandle = AudioHandle.Invalid;
            return;
        }

        AudioApi.ConfigureHandle(_currentHandle, Bus, Spatialized, Attenuation, Priority, Looping);
        AudioApi.SetVolume(_currentHandle, Volume);
        AudioApi.SetPitch(_currentHandle, Pitch);
        AudioApi.SetPaused(_currentHandle, Paused || Mute);

        if (Spatialized)
            AudioApi.SetWorldPosition(_currentHandle, Entity.Transform.WorldPosition);
    }
}
