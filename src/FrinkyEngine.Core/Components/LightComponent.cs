using FrinkyEngine.Core.ECS;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;


/// <summary>
/// Determines the behavior of a light source in the scene.
/// </summary>
public enum LightType
{
    /// <summary>
    /// Casts parallel light rays in the entity's forward direction (like sunlight).
    /// </summary>
    Directional = 0,

    /// <summary>
    /// Emits light in all directions from the entity's position, attenuated by <see cref="LightComponent.Range"/>.
    /// </summary>
    Point = 1,

    /// <summary>
    /// Provides uniform ambient illumination across the entire scene.
    /// </summary>
    Skylight = 2
}

/// <summary>
/// Adds a light source to the scene. The light's position and direction come from the entity's <see cref="TransformComponent"/>.
/// </summary>
[ComponentCategory("Rendering")]
public class LightComponent : Component
{
    /// <summary>
    /// The type of light (directional, point, or skylight).
    /// </summary>
    public LightType LightType { get; set; } = LightType.Directional;

    /// <summary>
    /// The color of the emitted light (defaults to white).
    /// </summary>
    public Color LightColor { get; set; } = new(255, 255, 255, 255);

    /// <summary>
    /// Brightness multiplier applied to the light color (defaults to 1).
    /// </summary>
    public float Intensity { get; set; } = 1.0f;

    /// <summary>
    /// Maximum distance for point light attenuation, in world units (defaults to 10).
    /// </summary>
    public float Range { get; set; } = 10.0f;
}
