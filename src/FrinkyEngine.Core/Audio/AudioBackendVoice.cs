using Raylib_cs;

namespace FrinkyEngine.Core.Audio;

internal sealed class AudioBackendVoice
{
    public AudioBackendVoice(AudioClipResource clip, bool looping, Sound soundAlias)
    {
        Clip = clip;
        Looping = looping;
        SoundAlias = soundAlias;
        IsStream = false;
    }

    public AudioBackendVoice(AudioClipResource clip, bool looping, Music musicStream)
    {
        Clip = clip;
        Looping = looping;
        MusicStream = musicStream;
        IsStream = true;
    }

    public AudioClipResource Clip { get; }
    public bool IsStream { get; }
    public bool Looping { get; set; }
    public bool Paused { get; set; }
    public bool Stopped { get; set; }

    public Sound SoundAlias { get; }
    public Music MusicStream { get; set; }
}
