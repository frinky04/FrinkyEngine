using Raylib_cs;

namespace FrinkyEngine.Core.Audio;

internal sealed class AudioClipResource
{
    public AudioClipResource(string fullPath, bool streamed, Sound baseSound, bool hasBaseSound)
    {
        FullPath = fullPath;
        Streamed = streamed;
        BaseSound = baseSound;
        HasBaseSound = hasBaseSound;
    }

    public string FullPath { get; }
    public bool Streamed { get; }
    public Sound BaseSound { get; }
    public bool HasBaseSound { get; }
}
