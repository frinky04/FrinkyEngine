using Raylib_cs;

namespace FrinkyEngine.Core.Audio;

internal sealed class RaylibAudioBackend : IAudioBackend
{
    private bool _disposed;

    public RaylibAudioBackend()
    {
        AudioDeviceService.EnsureInitialized();
    }

    public AudioClipResource LoadClip(string fullPath, bool stream)
    {
        if (stream)
            return new AudioClipResource(fullPath, streamed: true, baseSound: default, hasBaseSound: false);

        var sound = Raylib.LoadSound(fullPath);
        if (!Raylib.IsSoundValid(sound))
            throw new InvalidOperationException($"Failed to load audio clip: {fullPath}");

        return new AudioClipResource(fullPath, streamed: false, baseSound: sound, hasBaseSound: true);
    }

    public void UnloadClip(AudioClipResource clip)
    {
        if (!clip.HasBaseSound)
            return;

        Raylib.UnloadSound(clip.BaseSound);
    }

    public AudioBackendVoice CreateVoice(AudioClipResource clip, bool looping)
    {
        if (clip.Streamed)
        {
            var music = Raylib.LoadMusicStream(clip.FullPath);
            if (!Raylib.IsMusicValid(music))
                throw new InvalidOperationException($"Failed to load streamed audio: {clip.FullPath}");
            return new AudioBackendVoice(clip, looping, music);
        }

        var alias = Raylib.LoadSoundAlias(clip.BaseSound);
        if (!Raylib.IsSoundValid(alias))
            throw new InvalidOperationException($"Failed to create sound alias: {clip.FullPath}");
        return new AudioBackendVoice(clip, looping, alias);
    }

    public void DestroyVoice(AudioBackendVoice voice)
    {
        if (voice.IsStream)
        {
            Raylib.StopMusicStream(voice.MusicStream);
            Raylib.UnloadMusicStream(voice.MusicStream);
            return;
        }

        Raylib.StopSound(voice.SoundAlias);
        Raylib.UnloadSoundAlias(voice.SoundAlias);
    }

    public void Play(AudioBackendVoice voice, float startTimeSeconds)
    {
        voice.Stopped = false;
        voice.Paused = false;

        if (voice.IsStream)
        {
            if (startTimeSeconds > 0f)
            {
                var seek = float.IsFinite(startTimeSeconds) ? MathF.Max(0f, startTimeSeconds) : 0f;
                Raylib.SeekMusicStream(voice.MusicStream, seek);
            }

            Raylib.PlayMusicStream(voice.MusicStream);
            return;
        }

        Raylib.PlaySound(voice.SoundAlias);
    }

    public void Stop(AudioBackendVoice voice)
    {
        voice.Stopped = true;
        voice.Paused = false;

        if (voice.IsStream)
        {
            Raylib.StopMusicStream(voice.MusicStream);
            return;
        }

        Raylib.StopSound(voice.SoundAlias);
    }

    public void Pause(AudioBackendVoice voice)
    {
        if (voice.Stopped)
            return;

        voice.Paused = true;
        if (voice.IsStream)
            Raylib.PauseMusicStream(voice.MusicStream);
        else
            Raylib.PauseSound(voice.SoundAlias);
    }

    public void Resume(AudioBackendVoice voice)
    {
        if (voice.Stopped)
            return;

        voice.Paused = false;
        if (voice.IsStream)
            Raylib.ResumeMusicStream(voice.MusicStream);
        else
            Raylib.ResumeSound(voice.SoundAlias);
    }

    public bool IsPlaying(AudioBackendVoice voice)
    {
        if (voice.Stopped)
            return false;
        if (voice.Paused)
            return true;

        if (voice.IsStream)
            return Raylib.IsMusicStreamPlaying(voice.MusicStream);

        if (voice.Looping)
            return true;

        return Raylib.IsSoundPlaying(voice.SoundAlias);
    }

    public void SetVolume(AudioBackendVoice voice, float volume)
    {
        var v = Math.Clamp(volume, 0f, 2f);
        if (voice.IsStream)
            Raylib.SetMusicVolume(voice.MusicStream, v);
        else
            Raylib.SetSoundVolume(voice.SoundAlias, v);
    }

    public void SetPitch(AudioBackendVoice voice, float pitch)
    {
        var p = Math.Clamp(pitch, 0.01f, 3f);
        if (voice.IsStream)
            Raylib.SetMusicPitch(voice.MusicStream, p);
        else
            Raylib.SetSoundPitch(voice.SoundAlias, p);
    }

    public void SetPan(AudioBackendVoice voice, float pan01)
    {
        var pan = Math.Clamp(pan01, 0f, 1f);
        if (voice.IsStream)
            Raylib.SetMusicPan(voice.MusicStream, pan);
        else
            Raylib.SetSoundPan(voice.SoundAlias, pan);
    }

    public void SetLooping(AudioBackendVoice voice, bool looping)
    {
        voice.Looping = looping;
    }

    public void Update(AudioBackendVoice voice)
    {
        if (voice.Stopped)
            return;

        if (voice.IsStream)
        {
            Raylib.UpdateMusicStream(voice.MusicStream);
            if (voice.Looping && !voice.Paused && !Raylib.IsMusicStreamPlaying(voice.MusicStream))
                Raylib.PlayMusicStream(voice.MusicStream);
            return;
        }

        if (voice.Looping && !voice.Paused && !Raylib.IsSoundPlaying(voice.SoundAlias))
            Raylib.PlaySound(voice.SoundAlias);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        AudioDeviceService.ShutdownIfUnused();
    }
}
