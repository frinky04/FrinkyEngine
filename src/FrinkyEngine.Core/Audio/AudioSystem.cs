using System.Numerics;
using FrinkyEngine.Core.Components;

namespace FrinkyEngine.Core.Audio;

/// <summary>
/// Scene-level bridge that synchronizes ECS audio components with the audio engine.
/// </summary>
public sealed class AudioSystem : IDisposable
{
    private readonly FrinkyEngine.Core.Scene.Scene _scene;
    private readonly AudioEngine _engine;
    private bool _disposed;
    private AudioFrameStats _lastStats;

    /// <summary>
    /// Creates a new scene audio system.
    /// </summary>
    /// <param name="scene">Owning scene.</param>
    public AudioSystem(FrinkyEngine.Core.Scene.Scene scene)
    {
        _scene = scene;
        _engine = new AudioEngine();
        Audio.BindEngine(_engine);
    }

    /// <summary>
    /// Updates listener state, source sync, and mixer state.
    /// </summary>
    /// <param name="dt">Frame delta time in seconds.</param>
    public void Update(float dt)
    {
        if (_disposed)
            return;

        SyncListener();
        SyncSources();
        _engine.Update(dt);
        _lastStats = _engine.GetFrameStats();
    }

    /// <summary>
    /// Returns latest frame audio diagnostics.
    /// </summary>
    public AudioFrameStats GetFrameStats() => _lastStats;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Audio.UnbindEngine(_engine);
        _engine.Dispose();
    }

    private void SyncListener()
    {
        var listeners = _scene.GetComponents<AudioListenerComponent>();
        AudioListenerComponent? listener = listeners.FirstOrDefault(l => l.Enabled && l.Entity.Active && l.IsPrimary);
        listener ??= listeners.FirstOrDefault(l => l.Enabled && l.Entity.Active);

        if (listener != null)
        {
            var transform = listener.Entity.Transform;
            _engine.SetListener(transform.WorldPosition, transform.Right, transform.Forward);
            _engine.ListenerVolumeScale = listener.MasterVolumeScale;
            return;
        }

        if (_scene.MainCamera?.Entity != null)
        {
            var transform = _scene.MainCamera.Entity.Transform;
            _engine.SetListener(transform.WorldPosition, transform.Right, transform.Forward);
            _engine.ListenerVolumeScale = 1f;
            return;
        }

        _engine.SetListener(Vector3.Zero, Vector3.UnitX, -Vector3.UnitZ);
        _engine.ListenerVolumeScale = 1f;
    }

    private void SyncSources()
    {
        var sources = _scene.GetComponents<AudioSourceComponent>();
        foreach (var source in sources)
        {
            if (!source.Enabled || !source.Entity.Active)
            {
                source.PauseDueToInactiveState();
                continue;
            }

            source.SyncRuntimePlayback();
        }
    }
}
