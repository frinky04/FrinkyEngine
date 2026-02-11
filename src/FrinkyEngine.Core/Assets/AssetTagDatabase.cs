using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrinkyEngine.Core.Assets;

/// <summary>Represents a single tag definition with a name and display color.</summary>
public class AssetTag
{
    /// <summary>Display name of the tag.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Hex color string (e.g. "#FF0000") used for UI display.</summary>
    public string Color { get; set; } = "#FFFFFF";
}

/// <summary>Serialization container for tag definitions and per-asset tag assignments.</summary>
public class AssetTagData
{
    /// <summary>All defined tags.</summary>
    public List<AssetTag> Tags { get; set; } = new();
    /// <summary>Maps asset relative paths to lists of assigned tag names.</summary>
    public Dictionary<string, List<string>> AssetTags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Manages asset tag definitions and per-asset tag assignments, persisted as JSON.</summary>
public class AssetTagDatabase
{
    /// <summary>Default file name for the tag database.</summary>
    public const string FileName = "asset_tags.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AssetTagData _data = new();

    /// <summary>Loads the tag database from the project directory, or creates a new empty one.</summary>
    public static AssetTagDatabase LoadOrCreate(string projectDirectory)
    {
        var db = new AssetTagDatabase();
        var path = Path.Combine(projectDirectory, FileName);

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                db._data = JsonSerializer.Deserialize<AssetTagData>(json, JsonOptions) ?? new AssetTagData();
            }
            catch
            {
                db._data = new AssetTagData();
            }
        }

        return db;
    }

    /// <summary>Saves the tag database to the project directory, sorting entries for clean diffs.</summary>
    public void Save(string projectDirectory)
    {
        var path = Path.Combine(projectDirectory, FileName);

        // Sort asset tags dictionary by key for clean diffs
        var sorted = new SortedDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in _data.AssetTags)
        {
            if (kvp.Value.Count > 0)
                sorted[kvp.Key] = kvp.Value.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();
        }

        var output = new AssetTagData
        {
            Tags = _data.Tags.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            AssetTags = new Dictionary<string, List<string>>(sorted, StringComparer.OrdinalIgnoreCase)
        };

        var json = JsonSerializer.Serialize(output, JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>Returns all defined tags.</summary>
    public List<AssetTag> GetAllTags()
    {
        return _data.Tags;
    }

    /// <summary>Returns the resolved tag objects assigned to the given asset.</summary>
    public List<AssetTag> GetTagsForAsset(string relativePath)
    {
        if (!_data.AssetTags.TryGetValue(relativePath, out var tagNames))
            return new List<AssetTag>();

        return tagNames
            .Select(name => _data.Tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            .Where(t => t != null)
            .Cast<AssetTag>()
            .ToList();
    }

    /// <summary>Replaces all tag assignments for an asset.</summary>
    public void SetAssetTags(string relativePath, List<string> tagNames)
    {
        if (tagNames.Count == 0)
            _data.AssetTags.Remove(relativePath);
        else
            _data.AssetTags[relativePath] = tagNames.ToList();
    }

    /// <summary>Adds a tag to multiple assets at once.</summary>
    public void AddTagToAssets(IEnumerable<string> relativePaths, string tagName)
    {
        foreach (var path in relativePaths)
        {
            if (!_data.AssetTags.TryGetValue(path, out var tags))
            {
                tags = new List<string>();
                _data.AssetTags[path] = tags;
            }

            if (!tags.Any(t => string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase)))
                tags.Add(tagName);
        }
    }

    /// <summary>Removes a tag from multiple assets at once.</summary>
    public void RemoveTagFromAssets(IEnumerable<string> relativePaths, string tagName)
    {
        foreach (var path in relativePaths)
        {
            if (!_data.AssetTags.TryGetValue(path, out var tags))
                continue;

            tags.RemoveAll(t => string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase));
            if (tags.Count == 0)
                _data.AssetTags.Remove(path);
        }
    }

    /// <summary>Creates a new tag definition if one with the same name does not already exist.</summary>
    public void CreateTag(string name, string hexColor)
    {
        if (_data.Tags.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            return;

        _data.Tags.Add(new AssetTag { Name = name, Color = hexColor });
    }

    /// <summary>Deletes a tag definition and removes it from all asset assignments.</summary>
    public void DeleteTag(string name)
    {
        _data.Tags.RemoveAll(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

        // Remove this tag from all asset assignments
        var keysToClean = _data.AssetTags.Keys.ToList();
        foreach (var key in keysToClean)
        {
            var tags = _data.AssetTags[key];
            tags.RemoveAll(t => string.Equals(t, name, StringComparison.OrdinalIgnoreCase));
            if (tags.Count == 0)
                _data.AssetTags.Remove(key);
        }
    }

    /// <summary>Renames a tag and updates all asset assignments to use the new name.</summary>
    public void RenameTag(string oldName, string newName)
    {
        var tag = _data.Tags.FirstOrDefault(t => string.Equals(t.Name, oldName, StringComparison.OrdinalIgnoreCase));
        if (tag == null)
            return;

        tag.Name = newName;

        // Update all asset assignments
        foreach (var tags in _data.AssetTags.Values)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (string.Equals(tags[i], oldName, StringComparison.OrdinalIgnoreCase))
                    tags[i] = newName;
            }
        }
    }

    /// <summary>Updates the display color for a tag.</summary>
    public void UpdateTagColor(string name, string hexColor)
    {
        var tag = _data.Tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        if (tag != null)
            tag.Color = hexColor;
    }

    /// <summary>Returns the set of asset paths that have the given tag assigned.</summary>
    public HashSet<string> GetAssetsWithTag(string tagName)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in _data.AssetTags)
        {
            if (kvp.Value.Any(t => string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase)))
                result.Add(kvp.Key);
        }

        return result;
    }

    /// <summary>Checks whether the given asset has a specific tag assigned.</summary>
    public bool AssetHasTag(string relativePath, string tagName)
    {
        if (!_data.AssetTags.TryGetValue(relativePath, out var tags))
            return false;

        return tags.Any(t => string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Updates the key for an asset's tag assignments when the asset is renamed.</summary>
    public void RenameAssetPath(string oldPath, string newPath)
    {
        if (!_data.AssetTags.Remove(oldPath, out var tags))
            return;

        _data.AssetTags[newPath] = tags;
    }

    /// <summary>Removes tag assignments for assets that no longer exist.</summary>
    public void CleanupStaleEntries(IReadOnlySet<string> existingAssetPaths)
    {
        var staleKeys = _data.AssetTags.Keys
            .Where(k => !existingAssetPaths.Contains(k))
            .ToList();

        foreach (var key in staleKeys)
            _data.AssetTags.Remove(key);
    }
}
