using FrinkyEngine.Core.Assets;

namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Project-level audio settings applied at runtime startup.
/// </summary>
public class AudioProjectSettings
{
    /// <summary>
    /// Global singleton instance.
    /// </summary>
    public static AudioProjectSettings Current { get; private set; } = new();

    /// <summary>
    /// Master bus volume.
    /// </summary>
    public float MasterVolume { get; set; } = 1f;

    /// <summary>
    /// Music bus volume.
    /// </summary>
    public float MusicVolume { get; set; } = 1f;

    /// <summary>
    /// Sound effects bus volume.
    /// </summary>
    public float SfxVolume { get; set; } = 1f;

    /// <summary>
    /// UI bus volume.
    /// </summary>
    public float UiVolume { get; set; } = 1f;

    /// <summary>
    /// Voice bus volume.
    /// </summary>
    public float VoiceVolume { get; set; } = 1f;

    /// <summary>
    /// Ambient bus volume.
    /// </summary>
    public float AmbientVolume { get; set; } = 1f;

    /// <summary>
    /// Maximum simultaneously active voices.
    /// </summary>
    public int MaxVoices { get; set; } = 128;

    /// <summary>
    /// Reserved scalar for Doppler-like effects.
    /// </summary>
    public float DopplerScale { get; set; } = 1f;

    /// <summary>
    /// Whether low-priority voices can be replaced when voice budget is full.
    /// </summary>
    public bool EnableVoiceStealing { get; set; } = true;

    /// <summary>
    /// Applies values from runtime project settings.
    /// </summary>
    public static void ApplyFrom(RuntimeProjectSettings runtime)
    {
        Current = new AudioProjectSettings
        {
            MasterVolume = runtime.AudioMasterVolume,
            MusicVolume = runtime.AudioMusicVolume,
            SfxVolume = runtime.AudioSfxVolume,
            UiVolume = runtime.AudioUiVolume,
            VoiceVolume = runtime.AudioVoiceVolume,
            AmbientVolume = runtime.AudioAmbientVolume,
            MaxVoices = runtime.AudioMaxVoices,
            DopplerScale = runtime.AudioDopplerScale,
            EnableVoiceStealing = runtime.AudioEnableVoiceStealing
        };
        Current.Normalize();
    }

    /// <summary>
    /// Applies values from export manifest settings.
    /// </summary>
    public static void ApplyFrom(ExportManifest manifest)
    {
        Current = new AudioProjectSettings
        {
            MasterVolume = manifest.AudioMasterVolume ?? 1f,
            MusicVolume = manifest.AudioMusicVolume ?? 1f,
            SfxVolume = manifest.AudioSfxVolume ?? 1f,
            UiVolume = manifest.AudioUiVolume ?? 1f,
            VoiceVolume = manifest.AudioVoiceVolume ?? 1f,
            AmbientVolume = manifest.AudioAmbientVolume ?? 1f,
            MaxVoices = manifest.AudioMaxVoices ?? 128,
            DopplerScale = manifest.AudioDopplerScale ?? 1f,
            EnableVoiceStealing = manifest.AudioEnableVoiceStealing ?? true
        };
        Current.Normalize();
    }

    /// <summary>
    /// Returns the configured base volume for a bus.
    /// </summary>
    public float GetBusVolume(AudioBusId bus)
    {
        return bus switch
        {
            AudioBusId.Master => MasterVolume,
            AudioBusId.Music => MusicVolume,
            AudioBusId.Sfx => SfxVolume,
            AudioBusId.Ui => UiVolume,
            AudioBusId.Voice => VoiceVolume,
            AudioBusId.Ambient => AmbientVolume,
            _ => 1f
        };
    }

    /// <summary>
    /// Sets the configured base volume for a bus.
    /// </summary>
    public void SetBusVolume(AudioBusId bus, float volume)
    {
        switch (bus)
        {
            case AudioBusId.Master:
                MasterVolume = volume;
                break;
            case AudioBusId.Music:
                MusicVolume = volume;
                break;
            case AudioBusId.Sfx:
                SfxVolume = volume;
                break;
            case AudioBusId.Ui:
                UiVolume = volume;
                break;
            case AudioBusId.Voice:
                VoiceVolume = volume;
                break;
            case AudioBusId.Ambient:
                AmbientVolume = volume;
                break;
        }
    }

    /// <summary>
    /// Clamps settings into safe ranges.
    /// </summary>
    public void Normalize()
    {
        MasterVolume = ClampVolume(MasterVolume, 1f);
        MusicVolume = ClampVolume(MusicVolume, 1f);
        SfxVolume = ClampVolume(SfxVolume, 1f);
        UiVolume = ClampVolume(UiVolume, 1f);
        VoiceVolume = ClampVolume(VoiceVolume, 1f);
        AmbientVolume = ClampVolume(AmbientVolume, 1f);
        MaxVoices = ClampInt(MaxVoices, 16, 512, 128);
        DopplerScale = ClampFloat(DopplerScale, 0f, 10f, 1f);
    }

    private static float ClampVolume(float value, float fallback) => ClampFloat(value, 0f, 2f, fallback);

    private static int ClampInt(int value, int min, int max, int fallback)
    {
        if (value < min || value > max)
            return fallback;
        return value;
    }

    private static float ClampFloat(float value, float min, float max, float fallback)
    {
        if (!float.IsFinite(value) || value < min || value > max)
            return fallback;
        return value;
    }
}
