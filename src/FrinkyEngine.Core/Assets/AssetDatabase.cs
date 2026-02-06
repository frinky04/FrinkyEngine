namespace FrinkyEngine.Core.Assets;

public class AssetDatabase
{
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
        { ".cs", AssetType.Script },
    };

    private List<AssetEntry> _assets = new();
    private string _assetsPath = string.Empty;

    public void RegisterExtension(string ext, AssetType type)
    {
        _extensionMap[ext.StartsWith('.') ? ext : "." + ext] = type;
    }

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

    public void Refresh()
    {
        if (!string.IsNullOrEmpty(_assetsPath))
            Scan(_assetsPath);
    }

    public IReadOnlyList<AssetEntry> GetAssets(AssetType? filter = null)
    {
        if (filter == null)
            return _assets;

        return _assets.Where(a => a.Type == filter.Value).ToList();
    }

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

    public void Clear()
    {
        _assets.Clear();
        _assetsPath = string.Empty;
    }
}
