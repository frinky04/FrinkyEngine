using FrinkyEngine.Core.ECS;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Marks an entity as an audio listener for 3D spatialization.
/// </summary>
[ComponentCategory("Audio")]
public class AudioListenerComponent : Component
{
    private float _masterVolumeScale = 1f;

    /// <summary>
    /// When true, this listener is preferred over others in the scene.
    /// </summary>
    [InspectorLabel("Is Primary")]
    public bool IsPrimary { get; set; } = true;

    /// <summary>
    /// Per-listener volume scale applied after bus gains.
    /// </summary>
    [InspectorLabel("Master Volume Scale")]
    [InspectorRange(0f, 2f, 0.01f)]
    public float MasterVolumeScale
    {
        get => _masterVolumeScale;
        set => _masterVolumeScale = float.IsFinite(value) ? Math.Clamp(value, 0f, 2f) : 1f;
    }
}
