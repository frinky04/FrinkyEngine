namespace FrinkyEngine.Core.Audio;

internal interface IAudioBackend : IDisposable
{
    AudioClipResource LoadClip(string fullPath, bool stream);
    void UnloadClip(AudioClipResource clip);
    AudioBackendVoice CreateVoice(AudioClipResource clip, bool looping);
    void DestroyVoice(AudioBackendVoice voice);
    void Play(AudioBackendVoice voice, float startTimeSeconds);
    void Stop(AudioBackendVoice voice);
    void Pause(AudioBackendVoice voice);
    void Resume(AudioBackendVoice voice);
    bool IsPlaying(AudioBackendVoice voice);
    void SetVolume(AudioBackendVoice voice, float volume);
    void SetPitch(AudioBackendVoice voice, float pitch);
    void SetPan(AudioBackendVoice voice, float pan01);
    void SetLooping(AudioBackendVoice voice, bool looping);
    void Update(AudioBackendVoice voice);
}
