using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Configures the material for a single slot on a <see cref="MeshRendererComponent"/>.
/// </summary>
public class MaterialSlot
{
    /// <summary>
    /// Which material mapping mode this slot uses (defaults to <see cref="Rendering.MaterialType.SolidColor"/>).
    /// </summary>
    public MaterialType MaterialType { get; set; } = MaterialType.SolidColor;

    /// <summary>
    /// Asset-relative path to the texture file, used when <see cref="MaterialType"/> is
    /// <see cref="Rendering.MaterialType.Textured"/> or <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    [AssetFilter(AssetType.Texture)]
    public AssetReference TexturePath { get; set; } = new("");

    /// <summary>
    /// Texture coordinate scale used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    public float TriplanarScale { get; set; } = 1f;

    /// <summary>
    /// Blend sharpness used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// Higher values produce harder transitions between projection axes.
    /// </summary>
    public float TriplanarBlendSharpness { get; set; } = 4f;

    /// <summary>
    /// Whether triplanar projection uses world space (<c>true</c>) or object space (<c>false</c>).
    /// Used when <see cref="MaterialType"/> is <see cref="Rendering.MaterialType.TriplanarTexture"/>.
    /// </summary>
    public bool TriplanarUseWorldSpace { get; set; } = true;
}
