namespace FrinkyEngine.Core.Assets;

/// <summary>
/// A reference to a project asset by its relative path.
/// </summary>
public struct AssetReference
{
    /// <summary>
    /// Prefix used in serialized paths to denote engine-provided assets.
    /// </summary>
    public const string EnginePrefix = "engine:";

    /// <summary>
    /// The asset-relative path to the referenced file.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Creates a new asset reference with the given path.
    /// </summary>
    public AssetReference(string path) => Path = path ?? string.Empty;

    /// <summary>
    /// True when no asset is referenced.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Path);

    /// <summary>
    /// True when this reference points to an engine-provided asset.
    /// </summary>
    public bool IsEngineAsset => HasEnginePrefix(Path);

    /// <summary>
    /// Returns true if the given path starts with the engine prefix.
    /// </summary>
    public static bool HasEnginePrefix(string? path)
        => path != null && path.StartsWith(EnginePrefix, StringComparison.Ordinal);

    /// <summary>
    /// Strips the engine prefix from a path. Returns the path unchanged if no prefix is present.
    /// </summary>
    public static string StripEnginePrefix(string path)
        => HasEnginePrefix(path) ? path.Substring(EnginePrefix.Length) : path;

    /// <summary>
    /// Implicit conversion from a plain string path.
    /// </summary>
    public static implicit operator AssetReference(string path) => new(path);

    /// <inheritdoc />
    public override string ToString() => Path ?? string.Empty;
}

/// <summary>
/// Restricts an <see cref="AssetReference"/> property to a specific asset type in the editor.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AssetFilterAttribute : Attribute
{
    /// <summary>
    /// The asset type filter to apply.
    /// </summary>
    public AssetType Filter { get; }

    /// <summary>
    /// Creates a new asset filter attribute.
    /// </summary>
    public AssetFilterAttribute(AssetType filter) => Filter = filter;
}
