using Raylib_cs;

namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Shared audio-device lifetime management for editor and runtime.
/// </summary>
public static class AudioDeviceService
{
    private static readonly object Sync = new();
    private static int _refCount;

    /// <summary>
    /// Ensures the audio device is initialized and increments the usage count.
    /// </summary>
    public static void EnsureInitialized()
    {
        lock (Sync)
        {
            if (_refCount == 0 && !Raylib.IsAudioDeviceReady())
                Raylib.InitAudioDevice();
            _refCount++;
        }
    }

    /// <summary>
    /// Decrements the usage count and closes the audio device when no users remain.
    /// </summary>
    public static void ShutdownIfUnused()
    {
        lock (Sync)
        {
            if (_refCount <= 0)
                return;

            _refCount--;
            if (_refCount == 0 && Raylib.IsAudioDeviceReady())
                Raylib.CloseAudioDevice();
        }
    }
}
