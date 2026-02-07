using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrinkyEngine.Core.Assets;

public class AssetTag
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#FFFFFF";
}

public class AssetTagData
{
    public List<AssetTag> Tags { get; set; } = new();
    public Dictionary<string, List<string>> AssetTags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class AssetTagDatabase
{
    public const string FileName = "asset_tags.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AssetTagData _data = new();

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

    public List<AssetTag> GetAllTags()
    {
        return _data.Tags;
    }

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

    public void SetAssetTags(string relativePath, List<string> tagNames)
    {
        if (tagNames.Count == 0)
            _data.AssetTags.Remove(relativePath);
        else
            _data.AssetTags[relativePath] = tagNames.ToList();
    }

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

    public void CreateTag(string name, string hexColor)
    {
        if (_data.Tags.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            return;

        _data.Tags.Add(new AssetTag { Name = name, Color = hexColor });
    }

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

    public void UpdateTagColor(string name, string hexColor)
    {
        var tag = _data.Tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        if (tag != null)
            tag.Color = hexColor;
    }

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

    public bool AssetHasTag(string relativePath, string tagName)
    {
        if (!_data.AssetTags.TryGetValue(relativePath, out var tags))
            return false;

        return tags.Any(t => string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase));
    }

    public void CleanupStaleEntries(IReadOnlySet<string> existingAssetPaths)
    {
        var staleKeys = _data.AssetTags.Keys
            .Where(k => !existingAssetPaths.Contains(k))
            .ToList();

        foreach (var key in staleKeys)
            _data.AssetTags.Remove(key);
    }
}
