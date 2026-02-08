using FrinkyEngine.Core.Prefabs;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Serialization;

namespace FrinkyEngine.Core.Assets;

public class PrefabDatabase
{
    public static PrefabDatabase Instance { get; } = new();

    private readonly Dictionary<string, PrefabAssetData> _cache = new(StringComparer.OrdinalIgnoreCase);

    public PrefabAssetData? Load(string relativePath, bool resolveVariants = true)
    {
        var normalized = NormalizePath(relativePath);
        if (_cache.TryGetValue(normalized, out var cached))
            return cached.Clone();

        var absolutePath = ResolveAbsolutePath(normalized);
        var loaded = PrefabSerializer.Load(absolutePath);
        if (loaded == null)
            return null;

        PrefabAssetData result = loaded;
        if (resolveVariants && !string.IsNullOrWhiteSpace(loaded.SourcePrefab))
        {
            var resolved = ResolveVariant(normalized, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            if (resolved != null)
                result = resolved;
        }

        _cache[normalized] = result.Clone();
        return result.Clone();
    }

    public bool Save(string relativePath, PrefabAssetData prefab)
    {
        try
        {
            var normalized = NormalizePath(relativePath);
            var absolutePath = ResolveAbsolutePath(normalized);
            PrefabSerializer.Save(prefab, absolutePath);
            _cache[normalized] = prefab.Clone();
            return true;
        }
        catch (Exception ex)
        {
            FrinkyLog.Error($"Failed to save prefab '{relativePath}': {ex.Message}");
            return false;
        }
    }

    public void Invalidate(string relativePath)
    {
        _cache.Remove(NormalizePath(relativePath));
    }

    public void Clear()
    {
        _cache.Clear();
    }

    private PrefabAssetData? ResolveVariant(string relativePath, HashSet<string> stack)
    {
        var normalized = NormalizePath(relativePath);
        if (!stack.Add(normalized))
            return null;

        var absolutePath = ResolveAbsolutePath(normalized);
        var variant = PrefabSerializer.Load(absolutePath);
        if (variant == null)
            return null;

        if (string.IsNullOrWhiteSpace(variant.SourcePrefab))
            return variant;

        var source = ResolveVariant(variant.SourcePrefab, stack);
        if (source == null)
            return null;

        var merged = source.Clone();
        PrefabOverrideUtility.ApplyOverrides(merged.Root, variant.VariantOverrides);
        if (!string.IsNullOrWhiteSpace(variant.Name))
            merged.Name = variant.Name;
        merged.SourcePrefab = variant.SourcePrefab;
        merged.VariantOverrides = variant.VariantOverrides.Clone();
        return merged;
    }

    private static string ResolveAbsolutePath(string relativePath)
    {
        return Path.Combine(AssetManager.Instance.AssetsPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string NormalizePath(string path)
    {
        return path.Trim().Replace('\\', '/');
    }
}
