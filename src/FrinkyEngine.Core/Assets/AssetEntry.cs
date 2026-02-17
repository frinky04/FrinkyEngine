namespace FrinkyEngine.Core.Assets;

/// <summary>
/// Categorizes project assets by their file type.
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Unrecognized file extension.
    /// </summary>
    Unknown,

    /// <summary>
    /// A 3D model file (.obj, .gltf, .glb, etc.).
    /// </summary>
    Model,

    /// <summary>
    /// A scene file (.fscene).
    /// </summary>
    Scene,

    /// <summary>
    /// An image file (.png, .jpg, etc.).
    /// </summary>
    Texture,

    /// <summary>
    /// An audio file (.wav, .ogg, .mp3).
    /// </summary>
    Audio,

    /// <summary>
    /// A C# script file (.cs).
    /// </summary>
    Script,

    /// <summary>
    /// A prefab file (.fprefab).
    /// </summary>
    Prefab,

    /// <summary>
    /// A color palette file (.pal).
    /// </summary>
    Palette,

    /// <summary>
    /// A font file (.ttf, .otf).
    /// </summary>
    Font
}

/// <summary>
/// Represents a single asset file discovered in the project's assets directory.
/// </summary>
public class AssetEntry
{
    /// <summary>
    /// Path relative to the assets root, using forward slashes.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// File name with extension (e.g. "player.glb").
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Lowercase file extension including the dot (e.g. ".png").
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// The detected asset type based on file extension.
    /// </summary>
    public AssetType Type { get; }

    /// <summary>
    /// True if this asset comes from the engine content directory rather than the project.
    /// </summary>
    public bool IsEngineAsset { get; }

    /// <summary>
    /// Creates a new asset entry.
    /// </summary>
    /// <param name="relativePath">Path relative to the assets root.</param>
    /// <param name="type">The asset type classification.</param>
    /// <param name="isEngineAsset">Whether this asset is from engine content.</param>
    public AssetEntry(string relativePath, AssetType type, bool isEngineAsset = false)
    {
        RelativePath = relativePath.Replace('\\', '/');
        FileName = Path.GetFileName(relativePath);
        Extension = Path.GetExtension(relativePath).ToLowerInvariant();
        Type = type;
        IsEngineAsset = isEngineAsset;
    }
}
