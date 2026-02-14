using System.Text.Json;
using FrinkyEngine.Core.Rendering;

namespace FrinkyEngine.Editor;

public class EditorProjectSettings
{
    public const string FileName = "editor_settings.json";

    public int TargetFps { get; set; } = 120;
    public bool VSync { get; set; }
    public bool ShowPhysicsHitboxes { get; set; }
    public bool ColliderEditMode { get; set; }
    public bool ShowBonePreview { get; set; }
    public bool HideUnrecognisedAssets { get; set; } = true;
    public HierarchyEditorSettings Hierarchy { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static EditorProjectSettings LoadOrCreate(string? projectDirectory)
    {
        var defaults = GetDefault();
        if (projectDirectory == null)
            return defaults;

        var path = GetPath(projectDirectory);
        if (!File.Exists(path))
        {
            defaults.Save(projectDirectory);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<EditorProjectSettings>(json, JsonOptions) ?? GetDefault();
            settings.Normalize();
            return settings;
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to load editor project settings: {ex.Message}");
            return defaults;
        }
    }

    public void Save(string? projectDirectory)
    {
        if (projectDirectory == null)
            return;

        try
        {
            Normalize();
            var path = GetPath(projectDirectory);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            FrinkyLog.Warning($"Failed to save editor project settings: {ex.Message}");
        }
    }

    public EditorProjectSettings Clone()
    {
        return new EditorProjectSettings
        {
            TargetFps = TargetFps,
            VSync = VSync,
            ShowPhysicsHitboxes = ShowPhysicsHitboxes,
            ColliderEditMode = ColliderEditMode,
            ShowBonePreview = ShowBonePreview,
            HideUnrecognisedAssets = HideUnrecognisedAssets,
            Hierarchy = Hierarchy.Clone()
        };
    }

    public static string GetPath(string projectDirectory)
    {
        return Path.Combine(projectDirectory, ".frinky", FileName);
    }

    public static EditorProjectSettings GetDefault()
    {
        return new EditorProjectSettings
        {
            TargetFps = 120,
            VSync = false,
            ShowPhysicsHitboxes = false,
            ColliderEditMode = false,
            ShowBonePreview = false,
            HideUnrecognisedAssets = true
        };
    }

    public void Normalize()
    {
        // 0 = uncapped, otherwise clamp to 30-500
        if (TargetFps != 0 && (TargetFps < 30 || TargetFps > 500))
            TargetFps = 120;

        Hierarchy ??= new HierarchyEditorSettings();
        Hierarchy.Normalize();
    }
}

public class HierarchyEditorSettings
{
    public Dictionary<string, HierarchySceneState> Scenes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HierarchyEditorSettings Clone()
    {
        var clone = new HierarchyEditorSettings();
        foreach (var (sceneKey, sceneState) in Scenes)
            clone.Scenes[sceneKey] = sceneState.Clone();
        return clone;
    }

    public void Normalize()
    {
        Scenes ??= new Dictionary<string, HierarchySceneState>(StringComparer.OrdinalIgnoreCase);

        var normalized = new Dictionary<string, HierarchySceneState>(StringComparer.OrdinalIgnoreCase);
        foreach (var (sceneKey, sceneState) in Scenes)
        {
            if (string.IsNullOrWhiteSpace(sceneKey) || sceneState == null)
                continue;

            sceneState.Normalize();
            normalized[sceneKey.Trim()] = sceneState;
        }

        Scenes = normalized;
    }
}

public class HierarchySceneState
{
    public string SearchQuery { get; set; } = string.Empty;
    public bool FilterActiveOnly { get; set; }
    public bool FilterInactiveOnly { get; set; }
    public HierarchyPrefabFilter PrefabFilter { get; set; } = HierarchyPrefabFilter.Any;
    public string RequiredComponentType { get; set; } = string.Empty;
    public bool ShowOnlyMatches { get; set; } = true;
    public bool AutoExpandMatches { get; set; } = true;
    public List<HierarchyFolderState> Folders { get; set; } = new();
    public Dictionary<string, string> RootEntityFolders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> ExpandedFolderIds { get; set; } = new();
    public List<string> ExpandedEntityIds { get; set; } = new();

    public HierarchySceneState Clone()
    {
        return new HierarchySceneState
        {
            SearchQuery = SearchQuery,
            FilterActiveOnly = FilterActiveOnly,
            FilterInactiveOnly = FilterInactiveOnly,
            PrefabFilter = PrefabFilter,
            RequiredComponentType = RequiredComponentType,
            ShowOnlyMatches = ShowOnlyMatches,
            AutoExpandMatches = AutoExpandMatches,
            Folders = Folders.Select(f => f.Clone()).ToList(),
            RootEntityFolders = new Dictionary<string, string>(RootEntityFolders, StringComparer.OrdinalIgnoreCase),
            ExpandedFolderIds = ExpandedFolderIds.ToList(),
            ExpandedEntityIds = ExpandedEntityIds.ToList()
        };
    }

    public void Normalize()
    {
        SearchQuery ??= string.Empty;
        RequiredComponentType = RequiredComponentType?.Trim() ?? string.Empty;

        if (FilterActiveOnly && FilterInactiveOnly)
        {
            FilterActiveOnly = false;
            FilterInactiveOnly = false;
        }

        if (!Enum.IsDefined(PrefabFilter))
            PrefabFilter = HierarchyPrefabFilter.Any;

        Folders ??= new List<HierarchyFolderState>();
        RootEntityFolders ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ExpandedFolderIds ??= new List<string>();
        ExpandedEntityIds ??= new List<string>();

        var seenFolderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedFolders = new List<HierarchyFolderState>();
        foreach (var folder in Folders)
        {
            if (folder == null)
                continue;

            folder.Normalize();
            if (!seenFolderIds.Add(folder.Id))
                continue;

            normalizedFolders.Add(folder);
        }

        var folderIds = new HashSet<string>(normalizedFolders.Select(f => f.Id), StringComparer.OrdinalIgnoreCase);
        foreach (var folder in normalizedFolders)
        {
            if (!string.IsNullOrWhiteSpace(folder.ParentFolderId) &&
                (!folderIds.Contains(folder.ParentFolderId) ||
                 string.Equals(folder.ParentFolderId, folder.Id, StringComparison.OrdinalIgnoreCase)))
            {
                folder.ParentFolderId = null;
            }
        }

        ReindexFolderOrders(normalizedFolders);
        Folders = normalizedFolders;

        var normalizedAssignments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (entityId, folderId) in RootEntityFolders)
        {
            if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(folderId))
                continue;
            if (!folderIds.Contains(folderId))
                continue;

            normalizedAssignments[entityId.Trim()] = folderId.Trim();
        }
        RootEntityFolders = normalizedAssignments;

        ExpandedFolderIds = ExpandedFolderIds
            .Where(id => !string.IsNullOrWhiteSpace(id) && folderIds.Contains(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        ExpandedEntityIds = ExpandedEntityIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void ReindexFolderOrders(List<HierarchyFolderState> folders)
    {
        var grouped = folders
            .GroupBy(f => f.ParentFolderId ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            int order = 0;
            foreach (var folder in group.OrderBy(f => f.Order).ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
                folder.Order = order++;
        }
    }
}

public enum HierarchyPrefabFilter
{
    Any = 0,
    PrefabInstances = 1,
    PrefabRoots = 2,
    NonPrefabs = 3
}

public class HierarchyFolderState
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Folder";
    public string? ParentFolderId { get; set; }
    public int Order { get; set; }

    public HierarchyFolderState Clone()
    {
        return new HierarchyFolderState
        {
            Id = Id,
            Name = Name,
            ParentFolderId = ParentFolderId,
            Order = Order
        };
    }

    public void Normalize()
    {
        Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id.Trim();
        Name = string.IsNullOrWhiteSpace(Name) ? "Folder" : Name.Trim();
        ParentFolderId = string.IsNullOrWhiteSpace(ParentFolderId) ? null : ParentFolderId.Trim();
        if (Order < 0)
            Order = 0;
    }
}
