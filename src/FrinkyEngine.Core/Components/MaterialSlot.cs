using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Configures the material for a single slot on a <see cref="MeshRendererComponent"/>.
/// </summary>
public class MaterialSlot
{
    /// <summary>
    /// Whether this slot uses a solid color or a texture (defaults to <see cref="Rendering.MaterialType.SolidColor"/>).
    /// </summary>
    public MaterialType MaterialType { get; set; } = MaterialType.SolidColor;

    /// <summary>
    /// Asset-relative path to the texture file, used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.Textured"/>.
    /// </summary>
    public string TexturePath { get; set; } = string.Empty;
}
