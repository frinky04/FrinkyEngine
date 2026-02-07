namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Determines how a material surface is rendered.
/// </summary>
public enum MaterialType
{
    /// <summary>
    /// Renders using a flat tint color with no texture.
    /// </summary>
    SolidColor,

    /// <summary>
    /// Renders using a texture image mapped onto the surface.
    /// </summary>
    Textured
}
