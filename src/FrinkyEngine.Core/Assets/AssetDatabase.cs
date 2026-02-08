namespace FrinkyEngine.Core.Assets;

/// <summary>
/// Singleton that scans a project's assets directory and provides filtered access to discovered files.
/// </summary>
public class AssetDatabase
{
    /// <summary>
    /// The global asset database instance.
    /// </summary>
    public static AssetDatabase Instance { get; } = new();

    private readonly Dictionary<string, AssetType> _extensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".obj", AssetType.Model },
        { ".gltf", AssetType.Model },
        { ".glb", AssetType.Model },
        { ".iqm", AssetType.Model },
        { ".vox", AssetType.Model },
        { ".m3d", AssetType.Model },
        { ".fscene", AssetType.Scene },
        { ".png", AssetType.Texture },
        { ".jpg", AssetType.Texture },
        { ".jpeg", AssetType.Texture },
        { ".bmp", AssetType.Texture },
        { ".tga", AssetType.Texture },
        { ".wav", AssetType.Audio },
        { ".ogg", AssetType.Audio },
        { ".mp3", AssetType.Audio },
        { ".cs", AssetType.Script },
        { ".fprefab", AssetType.Prefab },
    };

    private List<AssetEntry> _assets = new();
    private string _assetsPath = string.Empty;

    /// <summary>
    /// Registers a custom file extension to be recognized as a specific asset type during scanning.
    /// </summary>
    /// <param name="ext">File extension (with or without leading dot).</param>
    /// <param name="type">The asset type to associate with the extension.</param>
    public void RegisterExtension(string ext, AssetType type)
    {
        _extensionMap[ext.StartsWith('.') ? ext : "." + ext] = type;
    }

    /// <summary>
    /// Scans the given directory recursively and rebuilds the asset list.
    /// </summary>
    /// <param name="assetsPath">Absolute path to the assets root directory.</param>
    public void Scan(string assetsPath)
    {
        _assetsPath = assetsPath;
        _assets.Clear();

        if (!Directory.Exists(assetsPath))
            return;

        foreach (var file in Directory.EnumerateFiles(assetsPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(assetsPath, file).Replace('\\', '/');
            var ext = Path.GetExtension(file).ToLowerInvariant();
            var type = _extensionMap.TryGetValue(ext, out var t) ? t : AssetType.Unknown;
            _assets.Add(new AssetEntry(relativePath, type));
        }

        _assets.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Rescans the previously scanned assets directory.
    /// </summary>
    public void Refresh()
    {
        if (!string.IsNullOrEmpty(_assetsPath))
            Scan(_assetsPath);
    }

    /// <summary>
    /// Gets all discovered assets, optionally filtered by type.
    /// </summary>
    /// <param name="filter">If specified, only assets of this type are returned.</param>
    /// <returns>A read-only list of matching asset entries.</returns>
    public IReadOnlyList<AssetEntry> GetAssets(AssetType? filter = null)
    {
        if (filter == null)
            return _assets;

        return _assets.Where(a => a.Type == filter.Value).ToList();
    }

    /// <summary>
    /// Gets assets that are direct children of the specified directory (non-recursive).
    /// </summary>
    /// <param name="relativeDir">Relative directory path (empty string for root).</param>
    /// <param name="filter">If specified, only assets of this type are returned.</param>
    /// <returns>A read-only list of matching asset entries.</returns>
    public IReadOnlyList<AssetEntry> GetAssetsInDirectory(string relativeDir, AssetType? filter = null)
    {
        var prefix = string.IsNullOrEmpty(relativeDir) ? "" : relativeDir.TrimEnd('/') + "/";

        return _assets.Where(a =>
        {
            if (!a.RelativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            // Only direct children (no further slashes after the prefix)
            var remainder = a.RelativePath.Substring(prefix.Length);
            if (remainder.Contains('/'))
                return false;

            return filter == null || a.Type == filter.Value;
        }).ToList();
    }

    /// <summary>
    /// Gets the names of immediate subdirectories under the specified directory.
    /// </summary>
    /// <param name="relativeDir">Relative directory path (empty string for root).</param>
    /// <returns>An alphabetically sorted list of subdirectory names.</returns>
    public IReadOnlyList<string> GetSubdirectories(string relativeDir)
    {
        var prefix = string.IsNullOrEmpty(relativeDir) ? "" : relativeDir.TrimEnd('/') + "/";
        var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var asset in _assets)
        {
            if (!asset.RelativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            var remainder = asset.RelativePath.Substring(prefix.Length);
            var slashIndex = remainder.IndexOf('/');
            if (slashIndex > 0)
            {
                dirs.Add(remainder.Substring(0, slashIndex));
            }
        }

        return dirs.OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Clears all cached asset entries and resets the scan path.
    /// </summary>
    public void Clear()
    {
        _assets.Clear();
        _assetsPath = string.Empty;
    }
}
