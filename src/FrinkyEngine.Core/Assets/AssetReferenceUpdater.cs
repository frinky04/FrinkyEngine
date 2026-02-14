using System.Reflection;

namespace FrinkyEngine.Core.Assets;

/// <summary>
/// Updates asset references when an asset is renamed, both on disk and in memory.
/// </summary>
public static class AssetReferenceUpdater
{
    /// <summary>
    /// Scans all <c>.fscene</c> and <c>.fprefab</c> files under the project directory,
    /// replacing occurrences of the old asset path with the new one.
    /// Returns the number of files modified.
    /// </summary>
    public static int UpdateReferencesOnDisk(string assetsDirectory, string oldPath, string newPath)
    {
        if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
            return 0;

        // Build all search/replace pairs: we need to handle both the bare filename
        // and the full relative path forms, since GetCanonicalName may store either.
        var replacements = BuildReplacementPairs(oldPath, newPath);
        int modifiedCount = 0;

        var projectDir = Directory.GetParent(assetsDirectory)?.FullName ?? assetsDirectory;
        var patterns = new[] { "*.fscene", "*.fprefab" };

        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.EnumerateFiles(projectDir, pattern, SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                var modified = text;

                foreach (var (search, replace) in replacements)
                {
                    modified = modified.Replace(search, replace, StringComparison.Ordinal);
                }

                if (!ReferenceEquals(modified, text) && !string.Equals(modified, text, StringComparison.Ordinal))
                {
                    File.WriteAllText(file, modified);
                    modifiedCount++;
                }
            }
        }

        return modifiedCount;
    }

    /// <summary>
    /// Updates all in-memory <see cref="AssetReference"/> properties on entities in the scene
    /// that match the old path, replacing them with the new path.
    /// Also updates <c>PrefabInstanceMetadata.AssetPath</c>.
    /// </summary>
    public static void UpdateReferencesInScene(Scene.Scene scene, string oldPath, string newPath)
    {
        if (scene == null) return;

        var oldFileName = Path.GetFileName(oldPath);
        var newFileName = Path.GetFileName(newPath);

        foreach (var entity in scene.Entities)
        {
            // Update prefab asset path
            if (entity.Prefab != null && !entity.Prefab.AssetPath.IsEmpty)
            {
                if (PathMatches(entity.Prefab.AssetPath.Path, oldPath, oldFileName))
                    entity.Prefab.AssetPath = new AssetReference(
                        IsBareName(entity.Prefab.AssetPath.Path) ? newFileName : newPath);
            }

            // Update all AssetReference properties on components
            foreach (var component in entity.Components)
            {
                UpdateAssetReferencesOnObject(component, oldPath, newPath, oldFileName, newFileName);
            }
        }
    }

    private static void UpdateAssetReferencesOnObject(object obj, string oldPath, string newPath, string oldFileName, string newFileName)
    {
        var type = obj.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            if (prop.PropertyType == typeof(AssetReference))
            {
                var val = (AssetReference)prop.GetValue(obj)!;
                if (!val.IsEmpty && PathMatches(val.Path, oldPath, oldFileName))
                {
                    prop.SetValue(obj, new AssetReference(
                        IsBareName(val.Path) ? newFileName : newPath));
                }
            }
            else if (prop.PropertyType.IsGenericType
                     && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                     && prop.PropertyType.GetGenericArguments()[0] == typeof(AssetReference))
            {
                if (prop.GetValue(obj) is not List<AssetReference> list) continue;
                for (int i = 0; i < list.Count; i++)
                {
                    var val = list[i];
                    if (!val.IsEmpty && PathMatches(val.Path, oldPath, oldFileName))
                    {
                        list[i] = new AssetReference(IsBareName(val.Path) ? newFileName : newPath);
                    }
                }
            }
        }
    }

    private static bool PathMatches(string current, string oldRelPath, string oldFileName)
    {
        return string.Equals(current, oldRelPath, StringComparison.OrdinalIgnoreCase)
            || string.Equals(current, oldFileName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBareName(string path)
    {
        return !path.Contains('/') && !path.Contains('\\');
    }

    /// <summary>
    /// Scans all <c>.fscene</c> and <c>.fprefab</c> files under the project directory,
    /// returning the relative paths of files that contain references to the given asset path.
    /// </summary>
    public static List<string> FindReferencesOnDisk(string assetsDirectory, string assetPath)
    {
        var results = new List<string>();
        var projectDir = Directory.GetParent(assetsDirectory)?.FullName ?? assetsDirectory;
        var patterns = new[] { "*.fscene", "*.fprefab" };

        // Build search strings: JSON-quoted full path + bare filename
        var searchStrings = new List<string>();
        var normalized = assetPath.Replace('\\', '/');
        searchStrings.Add($"\"{normalized}\"");
        var fileName = Path.GetFileName(assetPath);
        if (!string.Equals(fileName, normalized, StringComparison.Ordinal))
            searchStrings.Add($"\"{fileName}\"");

        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.EnumerateFiles(projectDir, pattern, SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                foreach (var search in searchStrings)
                {
                    if (text.Contains(search, StringComparison.Ordinal))
                    {
                        results.Add(Path.GetRelativePath(projectDir, file).Replace('\\', '/'));
                        break;
                    }
                }
            }
        }

        return results;
    }

    private static List<(string search, string replace)> BuildReplacementPairs(string oldPath, string newPath)
    {
        var pairs = new List<(string, string)>();
        var oldFileName = Path.GetFileName(oldPath);
        var newFileName = Path.GetFileName(newPath);

        // JSON-quoted full relative path (forward slashes)
        var oldNorm = oldPath.Replace('\\', '/');
        var newNorm = newPath.Replace('\\', '/');
        pairs.Add(($"\"{oldNorm}\"", $"\"{newNorm}\""));

        // JSON-quoted bare filename (if different from full path)
        if (!string.Equals(oldFileName, oldNorm, StringComparison.Ordinal))
            pairs.Add(($"\"{oldFileName}\"", $"\"{newFileName}\""));

        return pairs;
    }
}
