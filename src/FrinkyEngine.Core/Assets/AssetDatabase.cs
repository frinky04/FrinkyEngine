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
        { ".pal", AssetType.Palette },
    };

    private List<AssetEntry> _assets = new();
    private HashSet<string> _pathIndex = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, List<AssetEntry>> _fileNameIndex = new(StringComparer.OrdinalIgnoreCase);
    private string _assetsPath = string.Empty;

    private List<AssetEntry> _engineAssets = new();
    private HashSet<string> _enginePathIndex = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, List<AssetEntry>> _engineFileNameIndex = new(StringComparer.OrdinalIgnoreCase);
    private string _engineContentPath = string.Empty;

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
        _pathIndex.Clear();
        _fileNameIndex.Clear();

        if (!Directory.Exists(assetsPath))
            return;

        foreach (var file in Directory.EnumerateFiles(assetsPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(assetsPath, file).Replace('\\', '/');
            var ext = Path.GetExtension(file).ToLowerInvariant();
            var type = _extensionMap.TryGetValue(ext, out var t) ? t : AssetType.Unknown;
            var entry = new AssetEntry(relativePath, type);
            _assets.Add(entry);
            _pathIndex.Add(relativePath);

            if (!_fileNameIndex.TryGetValue(entry.FileName, out var list))
            {
                list = new List<AssetEntry>();
                _fileNameIndex[entry.FileName] = list;
            }
            list.Add(entry);
        }

        _assets.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The absolute path to the engine content directory, or empty if not scanned.
    /// </summary>
    public string EngineContentPath => _engineContentPath;

    /// <summary>
    /// Scans the engine content directory and rebuilds the engine asset list.
    /// </summary>
    /// <param name="engineContentPath">Absolute path to the engine content root directory.</param>
    public void ScanEngineContent(string engineContentPath)
    {
        _engineContentPath = engineContentPath;
        _engineAssets.Clear();
        _enginePathIndex.Clear();
        _engineFileNameIndex.Clear();

        if (!Directory.Exists(engineContentPath))
            return;

        foreach (var file in Directory.EnumerateFiles(engineContentPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(engineContentPath, file).Replace('\\', '/');
            var ext = Path.GetExtension(file).ToLowerInvariant();
            var type = _extensionMap.TryGetValue(ext, out var t) ? t : AssetType.Unknown;
            var entry = new AssetEntry(relativePath, type, isEngineAsset: true);
            _engineAssets.Add(entry);
            _enginePathIndex.Add(relativePath);

            if (!_engineFileNameIndex.TryGetValue(entry.FileName, out var list))
            {
                list = new List<AssetEntry>();
                _engineFileNameIndex[entry.FileName] = list;
            }
            list.Add(entry);
        }

        _engineAssets.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all discovered engine assets, optionally filtered by type.
    /// </summary>
    /// <param name="filter">If specified, only assets of this type are returned.</param>
    /// <returns>A read-only list of matching engine asset entries.</returns>
    public IReadOnlyList<AssetEntry> GetEngineAssets(AssetType? filter = null)
    {
        if (filter == null)
            return _engineAssets;

        return _engineAssets.Where(a => a.Type == filter.Value).ToList();
    }

    /// <summary>
    /// Rescans the previously scanned assets directory.
    /// </summary>
    public void Refresh()
    {
        if (!string.IsNullOrEmpty(_assetsPath))
            Scan(_assetsPath);
        if (!string.IsNullOrEmpty(_engineContentPath))
            ScanEngineContent(_engineContentPath);
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
    /// Returns true if an asset with the given relative path exists in the database.
    /// Paths with the <c>engine:</c> prefix are checked against the engine content index.
    /// </summary>
    /// <param name="relativePath">Asset-relative path to check.</param>
    /// <returns>True if the asset exists.</returns>
    public bool AssetExists(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return false;

        if (AssetReference.HasEnginePrefix(relativePath))
            return _enginePathIndex.Contains(AssetReference.StripEnginePrefix(relativePath));

        return _pathIndex.Contains(relativePath);
    }

    /// <summary>
    /// Resolves a filename or relative path to a full relative path.
    /// Bare filenames are resolved via the filename index; paths containing separators use the path index directly.
    /// Paths with the <c>engine:</c> prefix are resolved against the engine content index and returned with the prefix intact.
    /// </summary>
    /// <param name="nameOrPath">A bare filename (e.g. "player.glb") or relative path (e.g. "Models/player.glb").</param>
    /// <returns>The full relative path if unambiguously resolved, or null.</returns>
    public string? ResolveAssetPath(string nameOrPath)
    {
        if (string.IsNullOrEmpty(nameOrPath))
            return null;

        // Handle engine: prefix
        if (AssetReference.HasEnginePrefix(nameOrPath))
        {
            var stripped = AssetReference.StripEnginePrefix(nameOrPath);
            var resolved = ResolveEngineAssetPath(stripped);
            return resolved != null ? AssetReference.EnginePrefix + resolved : null;
        }

        // If it contains a separator, treat as a full relative path
        if (nameOrPath.Contains('/') || nameOrPath.Contains('\\'))
        {
            var normalized = nameOrPath.Replace('\\', '/');
            return _pathIndex.Contains(normalized) ? normalized : null;
        }

        // Bare filename — look up in filename index
        if (_fileNameIndex.TryGetValue(nameOrPath, out var entries) && entries.Count == 1)
            return entries[0].RelativePath;

        return null;
    }

    private string? ResolveEngineAssetPath(string nameOrPath)
    {
        if (string.IsNullOrEmpty(nameOrPath))
            return null;

        if (nameOrPath.Contains('/') || nameOrPath.Contains('\\'))
        {
            var normalized = nameOrPath.Replace('\\', '/');
            return _enginePathIndex.Contains(normalized) ? normalized : null;
        }

        if (_engineFileNameIndex.TryGetValue(nameOrPath, out var entries) && entries.Count == 1)
            return entries[0].RelativePath;

        return null;
    }

    /// <summary>
    /// Returns true if the given filename maps to exactly one asset in the database.
    /// </summary>
    /// <param name="fileName">The bare filename to check.</param>
    /// <returns>True if there is exactly one asset with that filename.</returns>
    public bool IsFileNameUnique(string fileName)
    {
        return _fileNameIndex.TryGetValue(fileName, out var entries) && entries.Count == 1;
    }

    /// <summary>
    /// Returns true if the given filename maps to exactly one engine asset in the database.
    /// </summary>
    /// <param name="fileName">The bare filename to check.</param>
    /// <returns>True if there is exactly one engine asset with that filename.</returns>
    public bool IsEngineFileNameUnique(string fileName)
    {
        return _engineFileNameIndex.TryGetValue(fileName, out var entries) && entries.Count == 1;
    }

    /// <summary>
    /// Returns the shortest unambiguous name for an asset: just the filename if unique, or the full relative path if ambiguous.
    /// </summary>
    /// <param name="relativePath">The full relative path of the asset.</param>
    /// <returns>The canonical name to store in references.</returns>
    public string GetCanonicalName(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return IsFileNameUnique(fileName) ? fileName : relativePath.Replace('\\', '/');
    }

    /// <summary>
    /// Returns the shortest unambiguous name for an engine asset, prefixed with <c>engine:</c>.
    /// </summary>
    /// <param name="relativePath">The engine-relative path of the asset (without <c>engine:</c> prefix).</param>
    /// <returns>The canonical name with <c>engine:</c> prefix.</returns>
    public string GetEngineCanonicalName(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        var name = IsEngineFileNameUnique(fileName) ? fileName : relativePath.Replace('\\', '/');
        return AssetReference.EnginePrefix + name;
    }

    /// <summary>
    /// Returns true if a filename or relative path can be resolved to an existing asset.
    /// </summary>
    /// <param name="nameOrPath">A bare filename or relative path.</param>
    /// <returns>True if the asset can be resolved.</returns>
    public bool AssetExistsByName(string nameOrPath)
    {
        return ResolveAssetPath(nameOrPath) != null;
    }

    /// <summary>
    /// Clears all cached asset entries and resets the scan path.
    /// </summary>
    public void Clear()
    {
        _assets.Clear();
        _pathIndex.Clear();
        _fileNameIndex.Clear();
        _assetsPath = string.Empty;

        _engineAssets.Clear();
        _enginePathIndex.Clear();
        _engineFileNameIndex.Clear();
        _engineContentPath = string.Empty;
    }
}
