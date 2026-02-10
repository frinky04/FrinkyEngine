using System.Diagnostics;
using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Runtime audio mixer and voice manager used by active scenes.
/// </summary>
public sealed class AudioEngine : IDisposable
{
    private sealed class BusState
    {
        public float Volume;
        public bool Muted;
    }

    private sealed class VoiceEntry
    {
        public AudioHandle Handle = AudioHandle.Invalid;
        public AudioBackendVoice BackendVoice = null!;
        public AudioClipResource Clip = null!;
        public AudioBusId Bus;
        public float Volume;
        public float Pitch;
        public bool Spatialized;
        public AudioAttenuationSettings Attenuation;
        public Vector3 WorldPosition;
        public Entity? AttachedEntity;
        public bool Looping;
        public int Priority;
        public int CreatedOrder;
        public float FadeInDuration;
        public float FadeInRemaining;
        public float FadeOutDuration;
        public float FadeOutRemaining;
        public bool StopRequested;
        public bool Paused;
    }

    private readonly IAudioBackend _backend;
    private readonly Dictionary<int, VoiceEntry> _voices = new();
    private readonly Dictionary<string, AudioClipResource> _preloadedClips = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AudioClipResource> _streamClips = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<AudioBusId, BusState> _busStates = new();

    private int _nextVoiceId = 1;
    private int _createdOrder;
    private int _stolenVoicesPending;
    private bool _disposed;

    private Vector3 _listenerPosition;
    private Vector3 _listenerRight = Vector3.UnitX;
    private Vector3 _listenerForward = -Vector3.UnitZ;

    private AudioFrameStats _lastStats;

    /// <summary>
    /// Additional listener-level volume scale.
    /// </summary>
    public float ListenerVolumeScale { get; set; } = 1f;

    /// <summary>
    /// Creates a new audio engine with the default Raylib backend.
    /// </summary>
    public AudioEngine()
        : this(new RaylibAudioBackend())
    {
    }

    internal AudioEngine(IAudioBackend backend)
    {
        _backend = backend;
        InitializeBusState();
    }

    /// <summary>
    /// Plays a non-spatialized 2D sound.
    /// </summary>
    public AudioHandle PlaySound2D(string soundPath, in AudioPlayParams playParams = default)
    {
        var p = playParams;
        p.Spatialized = false;
        return PlayInternal(soundPath, in p, worldPosition: null, attachedEntity: null);
    }

    /// <summary>
    /// Plays a spatialized 3D sound at a world position.
    /// </summary>
    public AudioHandle PlaySoundAtLocation(string soundPath, Vector3 worldPosition, in AudioPlayParams playParams = default)
    {
        var p = playParams;
        p.Spatialized = true;
        return PlayInternal(soundPath, in p, worldPosition, attachedEntity: null);
    }

    /// <summary>
    /// Plays a spatialized 3D sound attached to an entity transform.
    /// </summary>
    public AudioHandle SpawnSoundAttached(string soundPath, Entity attachTo, in AudioPlayParams playParams = default)
    {
        var p = playParams;
        p.Spatialized = true;
        return PlayInternal(soundPath, in p, attachTo.Transform.WorldPosition, attachTo);
    }

    /// <summary>
    /// Stops a voice immediately or after a fade-out.
    /// </summary>
    public bool Stop(AudioHandle handle, float fadeOutSeconds = 0f)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;

        if (!float.IsFinite(fadeOutSeconds) || fadeOutSeconds <= 0f)
        {
            DestroyVoice(voice.Handle.Id);
            return true;
        }

        voice.StopRequested = true;
        voice.FadeOutDuration = fadeOutSeconds;
        voice.FadeOutRemaining = fadeOutSeconds;
        return true;
    }

    /// <summary>
    /// Pauses or resumes a voice.
    /// </summary>
    public bool SetPaused(AudioHandle handle, bool paused)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;

        voice.Paused = paused;
        if (paused)
            _backend.Pause(voice.BackendVoice);
        else
            _backend.Resume(voice.BackendVoice);
        return true;
    }

    /// <summary>
    /// Sets per-voice volume.
    /// </summary>
    public bool SetVolume(AudioHandle handle, float volume)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;

        voice.Volume = ClampVolume(volume);
        return true;
    }

    /// <summary>
    /// Sets per-voice pitch.
    /// </summary>
    public bool SetPitch(AudioHandle handle, float pitch)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;

        voice.Pitch = ClampPitch(pitch);
        return true;
    }

    /// <summary>
    /// Sets world position for a voice and detaches it from any bound entity.
    /// </summary>
    public bool SetWorldPosition(AudioHandle handle, Vector3 position)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;

        voice.WorldPosition = position;
        voice.AttachedEntity = null;
        return true;
    }

    /// <summary>
    /// Returns true if a voice exists and is currently active.
    /// </summary>
    public bool IsPlaying(AudioHandle handle)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;
        return _backend.IsPlaying(voice.BackendVoice);
    }

    /// <summary>
    /// Updates routing/spatial parameters for an existing voice.
    /// </summary>
    public bool ConfigureHandle(
        AudioHandle handle,
        AudioBusId bus,
        bool spatialized,
        AudioAttenuationSettings attenuation,
        int priority,
        bool looping)
    {
        if (!TryGetVoice(handle, out var voice))
            return false;

        attenuation.Normalize();
        voice.Bus = bus;
        voice.Spatialized = spatialized;
        voice.Attenuation = attenuation;
        voice.Priority = Math.Clamp(priority, 0, 1000);
        voice.Looping = looping;
        _backend.SetLooping(voice.BackendVoice, looping);
        return true;
    }

    /// <summary>
    /// Sets bus volume.
    /// </summary>
    public bool SetBusVolume(AudioBusId bus, float volume)
    {
        if (!_busStates.TryGetValue(bus, out var state))
            return false;

        state.Volume = ClampVolume(volume);
        return true;
    }

    /// <summary>
    /// Gets bus volume.
    /// </summary>
    public float GetBusVolume(AudioBusId bus)
    {
        return _busStates.TryGetValue(bus, out var state) ? state.Volume : 1f;
    }

    /// <summary>
    /// Sets bus mute state.
    /// </summary>
    public bool SetBusMuted(AudioBusId bus, bool muted)
    {
        if (!_busStates.TryGetValue(bus, out var state))
            return false;

        state.Muted = muted;
        return true;
    }

    /// <summary>
    /// Updates listener transform used by spatialization.
    /// </summary>
    public void SetListener(Vector3 position, Vector3 right, Vector3 forward)
    {
        _listenerPosition = position;
        _listenerRight = SafeNormalize(right, Vector3.UnitX);
        _listenerForward = SafeNormalize(forward, -Vector3.UnitZ);
    }

    /// <summary>
    /// Updates active voices and mixer state.
    /// </summary>
    /// <param name="dt">Frame delta time in seconds.</param>
    public void Update(float dt)
    {
        if (_disposed)
            return;

        var sw = Stopwatch.StartNew();
        var toRemove = new List<int>();
        int streamingVoices = 0;

        foreach (var (voiceId, voice) in _voices)
        {
            if (voice.AttachedEntity != null)
            {
                if (voice.AttachedEntity.Scene == null)
                    voice.AttachedEntity = null;
                else
                    voice.WorldPosition = voice.AttachedEntity.Transform.WorldPosition;
            }

            if (voice.Clip.Streamed)
                streamingVoices++;

            _backend.Update(voice.BackendVoice);

            if (voice.FadeInRemaining > 0f)
                voice.FadeInRemaining = MathF.Max(0f, voice.FadeInRemaining - MathF.Max(0f, dt));

            if (voice.FadeOutRemaining > 0f)
                voice.FadeOutRemaining = MathF.Max(0f, voice.FadeOutRemaining - MathF.Max(0f, dt));

            ApplyVoiceMix(voice);

            if (voice.StopRequested && voice.FadeOutRemaining <= 0f)
            {
                toRemove.Add(voiceId);
                continue;
            }

            if (!voice.Looping && !voice.StopRequested && !voice.Paused && !_backend.IsPlaying(voice.BackendVoice))
                toRemove.Add(voiceId);
        }

        foreach (var id in toRemove)
            DestroyVoice(id);

        sw.Stop();
        _lastStats = new AudioFrameStats(
            Valid: true,
            ActiveVoices: _voices.Count,
            VirtualizedVoices: 0,
            StolenVoicesThisFrame: _stolenVoicesPending,
            StreamingVoices: streamingVoices,
            UpdateTimeMs: sw.Elapsed.TotalMilliseconds);
        _stolenVoicesPending = 0;
    }

    /// <summary>
    /// Returns latest frame diagnostics.
    /// </summary>
    public AudioFrameStats GetFrameStats() => _lastStats;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var voice in _voices.Values.ToList())
            _backend.DestroyVoice(voice.BackendVoice);
        _voices.Clear();

        foreach (var clip in _preloadedClips.Values)
            _backend.UnloadClip(clip);
        _preloadedClips.Clear();

        foreach (var clip in _streamClips.Values)
            _backend.UnloadClip(clip);
        _streamClips.Clear();

        _backend.Dispose();
    }

    private AudioHandle PlayInternal(string soundPath, in AudioPlayParams playParams, Vector3? worldPosition, Entity? attachedEntity)
    {
        if (_disposed)
            return AudioHandle.Invalid;

        if (string.IsNullOrWhiteSpace(soundPath))
            return AudioHandle.Invalid;

        if (!TryResolvePath(soundPath, out var fullPath, out var normalizedPath))
            return AudioHandle.Invalid;

        var priority = Math.Clamp(playParams.Priority, 0, 1000);
        if (!EnsureVoiceBudget(priority))
            return AudioHandle.Invalid;

        var stream = ShouldStream(playParams.Bus);
        AudioClipResource clip;
        try
        {
            clip = GetOrCreateClip(fullPath, stream);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Audio clip load failed: {normalizedPath} ({ex.Message})");
            return AudioHandle.Invalid;
        }

        AudioBackendVoice backendVoice;
        try
        {
            backendVoice = _backend.CreateVoice(clip, playParams.Looping);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Audio voice creation failed: {normalizedPath} ({ex.Message})");
            return AudioHandle.Invalid;
        }

        var id = _nextVoiceId++;
        var handle = new AudioHandle(id, 1);

        var attenuation = playParams.AttenuationOverride
            ?? (playParams.Spatialized ? AudioAttenuationSettings.Default3D : AudioAttenuationSettings.Default2D);
        attenuation.Normalize();

        var voice = new VoiceEntry
        {
            Handle = handle,
            BackendVoice = backendVoice,
            Clip = clip,
            Bus = playParams.Bus,
            Volume = ClampVolume(playParams.Volume),
            Pitch = ClampPitch(playParams.Pitch),
            Spatialized = playParams.Spatialized,
            Attenuation = attenuation,
            WorldPosition = worldPosition ?? _listenerPosition,
            AttachedEntity = attachedEntity,
            Looping = playParams.Looping,
            Priority = priority,
            CreatedOrder = _createdOrder++,
            FadeInDuration = SafePositive(playParams.FadeInSeconds),
            FadeInRemaining = SafePositive(playParams.FadeInSeconds),
            FadeOutDuration = 0f,
            FadeOutRemaining = 0f,
            StopRequested = false,
            Paused = false
        };

        _voices[id] = voice;
        _backend.SetLooping(voice.BackendVoice, voice.Looping);
        _backend.SetPitch(voice.BackendVoice, voice.Pitch);
        _backend.SetPan(voice.BackendVoice, 0.5f);
        _backend.Play(voice.BackendVoice, SafePositive(playParams.StartTimeSeconds));
        ApplyVoiceMix(voice);
        return handle;
    }

    private void ApplyVoiceMix(VoiceEntry voice)
    {
        float busGain = GetEffectiveBusGain(voice.Bus);
        float listenerGain = ClampVolume(ListenerVolumeScale);

        float attenuationGain = 1f;
        float pan = voice.Attenuation.PanStereo;
        if (voice.Spatialized)
        {
            var distance = Vector3.Distance(_listenerPosition, voice.WorldPosition);
            attenuationGain = voice.Attenuation.EvaluateVolume(distance);
            pan = voice.Attenuation.EvaluatePan(_listenerPosition, _listenerRight, voice.WorldPosition);
        }

        float fadeGain = 1f;
        if (voice.FadeInDuration > 0f)
        {
            float tIn = 1f - Math.Clamp(voice.FadeInRemaining / voice.FadeInDuration, 0f, 1f);
            fadeGain *= tIn;
        }

        if (voice.FadeOutDuration > 0f)
        {
            float tOut = Math.Clamp(voice.FadeOutRemaining / voice.FadeOutDuration, 0f, 1f);
            fadeGain *= tOut;
        }

        float finalVolume = ClampVolume(voice.Volume * busGain * listenerGain * attenuationGain * fadeGain);
        float finalPan01 = Math.Clamp((Math.Clamp(pan, -1f, 1f) + 1f) * 0.5f, 0f, 1f);

        _backend.SetVolume(voice.BackendVoice, finalVolume);
        _backend.SetPitch(voice.BackendVoice, voice.Pitch);
        _backend.SetPan(voice.BackendVoice, finalPan01);
    }

    private bool EnsureVoiceBudget(int requestedPriority)
    {
        if (_voices.Count < AudioProjectSettings.Current.MaxVoices)
            return true;

        if (!AudioProjectSettings.Current.EnableVoiceStealing)
            return false;

        VoiceEntry? candidate = null;
        foreach (var voice in _voices.Values)
        {
            if (candidate == null)
            {
                candidate = voice;
                continue;
            }

            if (voice.Priority < candidate.Priority ||
                (voice.Priority == candidate.Priority && voice.CreatedOrder < candidate.CreatedOrder))
            {
                candidate = voice;
            }
        }

        if (candidate == null)
            return false;

        if (candidate.Priority > requestedPriority)
            return false;

        DestroyVoice(candidate.Handle.Id);
        _stolenVoicesPending++;
        return true;
    }

    private AudioClipResource GetOrCreateClip(string fullPath, bool stream)
    {
        var cache = stream ? _streamClips : _preloadedClips;
        if (cache.TryGetValue(fullPath, out var cached))
            return cached;

        var clip = _backend.LoadClip(fullPath, stream);
        cache[fullPath] = clip;
        return clip;
    }

    private bool TryGetVoice(AudioHandle handle, out VoiceEntry voice)
    {
        voice = null!;
        if (!handle.IsValid)
            return false;

        if (!_voices.TryGetValue(handle.Id, out var found) || found == null)
            return false;

        if (found.Handle.Generation != handle.Generation)
            return false;

        voice = found;
        return true;
    }

    private void DestroyVoice(int id)
    {
        if (!_voices.Remove(id, out var voice))
            return;

        _backend.DestroyVoice(voice.BackendVoice);
    }

    private void InitializeBusState()
    {
        foreach (var bus in Enum.GetValues<AudioBusId>())
        {
            _busStates[bus] = new BusState
            {
                Volume = ClampVolume(AudioProjectSettings.Current.GetBusVolume(bus)),
                Muted = false
            };
        }
    }

    private float GetEffectiveBusGain(AudioBusId bus)
    {
        var master = _busStates[AudioBusId.Master];
        if (master.Muted)
            return 0f;

        var masterGain = master.Volume;
        if (bus == AudioBusId.Master)
            return masterGain;

        if (!_busStates.TryGetValue(bus, out var busState))
            return masterGain;

        if (busState.Muted)
            return 0f;

        return masterGain * busState.Volume;
    }

    private static bool TryResolvePath(string rawPath, out string fullPath, out string normalizedPath)
    {
        normalizedPath = rawPath.Replace('\\', '/').Trim();
        if (!Path.IsPathRooted(normalizedPath))
            normalizedPath = AssetDatabase.Instance.ResolveAssetPath(normalizedPath) ?? normalizedPath;
        fullPath = Path.IsPathRooted(normalizedPath)
            ? normalizedPath
            : AssetManager.Instance.ResolvePath(normalizedPath);

        if (File.Exists(fullPath))
            return true;

        FrinkyLog.Warning($"Audio asset not found: {normalizedPath}");
        return false;
    }

    private static bool ShouldStream(AudioBusId bus)
    {
        return bus is AudioBusId.Music or AudioBusId.Ambient;
    }

    private static float ClampVolume(float value)
    {
        if (!float.IsFinite(value))
            return 1f;
        return Math.Clamp(value, 0f, 2f);
    }

    private static float ClampPitch(float value)
    {
        if (!float.IsFinite(value))
            return 1f;
        return Math.Clamp(value, 0.01f, 3f);
    }

    private static Vector3 SafeNormalize(Vector3 v, Vector3 fallback)
    {
        var lenSq = v.LengthSquared();
        if (lenSq <= 1e-12f)
            return fallback;
        return v / MathF.Sqrt(lenSq);
    }

    private static float SafePositive(float value)
    {
        if (!float.IsFinite(value) || value <= 0f)
            return 0f;
        return value;
    }
}
