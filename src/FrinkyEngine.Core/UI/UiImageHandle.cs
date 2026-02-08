using Raylib_cs;

namespace FrinkyEngine.Core.UI;

/// <summary>
/// Opaque texture handle used by the UI wrapper API.
/// </summary>
/// <param name="TextureId">Underlying GPU texture identifier.</param>
public readonly record struct UiImageHandle(uint TextureId)
{
    /// <summary>
    /// Gets whether this handle references a valid texture.
    /// </summary>
    public bool IsValid => TextureId != 0;

    /// <summary>
    /// Creates a UI image handle from a Raylib texture.
    /// </summary>
    /// <param name="texture">Source texture.</param>
    /// <returns>A UI image handle for the texture.</returns>
    public static UiImageHandle FromTexture(Texture2D texture) => new(texture.Id);
}

